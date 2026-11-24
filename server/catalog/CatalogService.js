import { productImage, required, price } from '../input.js';
import { CatalogRepository } from './CatalogRepository.js';

import { tx, fail, positive, text } from '../db.js';
import { audit } from '../auth.js';
const repository = new CatalogRepository();

async function variantWrite(write) {
  try {
    return await write();
  } catch (error) {
    if (error.code === 'ER_DUP_ENTRY') {
      const key = /for key '([^']+)'/.exec(error.sqlMessage ?? '')?.[1].split('.').at(-1);
      if (key === 'SKU') fail('SKU đã tồn tại.', 409);
      if (key === 'SanPhamID') fail('Sản phẩm đã có biến thể với size và màu này.', 409);
    }
    throw error;
  }
}

async function productWrite(write) {
  try {
    return await write();
  } catch (error) {
    if (error.code === 'ER_DUP_ENTRY' && /for key '[^']*\bMaSanPham'/.test(error.sqlMessage ?? '')) {
      fail('Mã sản phẩm đã tồn tại.', 409);
    }
    throw error;
  }
}

function productStatus(value) {
  if (value === 'NHAP') return 'AN'; // Accept forms from before drafts were merged with hidden products.
  if (!['AN', 'DANG_BAN', 'NGUNG_BAN'].includes(value)) fail('Trạng thái sản phẩm không hợp lệ.');
  return value;
}


function taxonomyState(body, defaultActive) {
  const status = body.status ?? ((body.active ?? defaultActive) ? 'HOAT_DONG' : 'AN');
  if (!['NHAP', 'HOAT_DONG', 'AN'].includes(status)) fail('Trạng thái không hợp lệ.');
  return [Number(status === 'HOAT_DONG'), Number(status !== 'HOAT_DONG')];
}
async function deleteCatalogDraft(r, table, col) {
  const id = positive(r.params.id);
  return tx(async (c) => {
    const product = table === 'SanPham';
    const flag = product ? "TrangThai='AN'" : 'DangHoatDong=0';
    const [item] = await repository.lockHiddenItem(flag, table, col, [id], c);
    if (!item) fail('Không tìm thấy mục cần xóa.', 404);
    if (!item.draft)
      fail(product ? 'Hãy ẩn sản phẩm trước khi xóa.' : 'Hãy ẩn mục này trước khi xóa.', 409);
    try {
      if (product) {
        const stocks = await repository.productStock([id], c);
        if (stocks.some((k) => k.SoLuongTon || k.SoLuongDaPhanBo))
          fail('Sản phẩm còn tồn kho hoặc đang giữ hàng.', 409);
        await repository.deleteImages([id], c);
        await repository.deleteStock([id], c);
        await repository.deleteVariants([id], c);
      }
      await repository.removeItem(table, col, [id], c);
    } catch (error) {
      if (error.code === 'ER_ROW_IS_REFERENCED_2')
        fail('Mục này đang được tham chiếu trong cửa hàng nên chưa thể xóa.', 409);
      throw error;
    }
    await audit(
      r.user,
      product ? 'XOA_SAN_PHAM' : 'XOA_BAN_NHAP',
      table,
      id,
      product ? 'Xóa sản phẩm ẩn' : 'Xóa bản nháp',
      c,
    );
    return { ok: true };
  });
}

// CatalogController chuyển yêu cầu tới các phương thức xử lý sản phẩm, màu, size, biến thể, danh mục và thương hiệu.
export class CatalogService {
  async browse(r) {
    const params = [],
      filters = ["s.TrangThai='DANG_BAN'", 'd.DangHoatDong=1', 't.DangHoatDong=1'];
    if (r.query.q) {
      filters.push('(s.TenSanPham LIKE ? OR s.MaSanPham LIKE ? OR t.TenThuongHieu LIKE ?)');
      params.push(...Array(3).fill('%' + text(r.query.q, 100) + '%'));
    }
    for (const [field, col] of [
      ['category', 's.DanhMucID'],
      ['brand', 's.ThuongHieuID'],
    ])
      if (r.query[field]) {
        filters.push(col + '=?');
        params.push(positive(r.query[field]));
      }
    if (r.query.max) {
      filters.push('s.GiaBan<=?');
      params.push(price(r.query.max));
    }
    if (r.query.size) {
      filters.push(
        'EXISTS(SELECT 1 FROM BienTheSanPham z JOIN KichCo kc ON kc.KichCoID=z.KichCoID JOIN TonKho tk ON tk.BienTheID=z.BienTheID WHERE z.SanPhamID=s.SanPhamID AND kc.GiaTri=? AND z.DangKinhDoanh=1 AND tk.SoLuongKhaDung>0)',
      );
      params.push(text(r.query.size, 20));
    }
    if (r.query.color) {
      filters.push(
        'EXISTS(SELECT 1 FROM BienTheSanPham z WHERE z.SanPhamID=s.SanPhamID AND z.MauSacID=? AND z.DangKinhDoanh=1)',
      );
      params.push(positive(r.query.color));
    }
    const order =
      { price_asc: 's.GiaBan ASC', price_desc: 's.GiaBan DESC', newest: 's.SanPhamID DESC' }[
        r.query.sort
      ] || 's.SanPhamID ASC';
    return {
      products: await repository.browse(filters, order, params),
      categories: await repository.categories(),
      brands: await repository.brands(),
      sizes: await repository.sizes(),
      colors: await repository.colors(),
    };
  }

  async details(r) {
    const p = (await repository.publicProduct([positive(r.params.id)]))[0];
    if (!p) fail('Không tìm thấy sản phẩm.', 404);
    p.variants = await repository.availableVariants([p.price, p.id]);
    p.reviews = await repository.reviews([p.id]);
    return p;
  }

  async listProducts() {
    return await repository.products();
  }

  async createSize(r) {
    return tx(async (c) => {
      const raw = String(r.body.name ?? '')
        .trim()
        .replace(',', '.');
      if (!/^\d{1,3}(?:\.\d{1,2})?$/.test(raw) || Number(raw) <= 0)
        fail('Size EU phải là số dương, ví dụ 42 hoặc 42.5.');
      const value = String(Number(raw));
      await repository.addSize([value, Math.floor(Number(value))], c);
      return (await repository.sizeByValue([value], c))[0];
    });
  }

  async createColor(r) {
    return tx(async (c) => {
      const name = String(r.body.name ?? '')
        .trim()
        .replace(/\s+/g, ' ');
      const hex = String(r.body.hex ?? '').trim();
      if (!name || name.length > 100) fail('Tên màu phải từ 1 đến 100 ký tự.');
      if (hex && !/^#[a-f0-9]{6}$/i.test(hex)) fail('Mã màu cần có dạng #RRGGBB.');
      await repository.addColor([name, hex || null], c);
      return (await repository.colorByName([name], c))[0];
    });
  }

  async lookups() {
    return {
      categories: await repository.allCategories(),
      brands: await repository.allBrands(),
      sizes: await repository.sizes(),
      colors: await repository.allColors(),
    };
  }

  async createProduct(r) {
    return productWrite(() => tx(async (c) => {
      const image = productImage(r.body.image);
      const a = await repository.insertProduct(
        [
          required(r.body.code, 'mã sản phẩm'),
          required(r.body.name, 'tên sản phẩm'),
          text(r.body.description, 8000),
          price(r.body.price),
          positive(r.body.categoryId),
          positive(r.body.brandId),
          productStatus(r.body.status || 'DANG_BAN'),
          r.user.id,
        ],
        c,
      );
      if (image) await repository.insertImage([a.insertId, image], c);
      await audit(r.user, 'TAO_SAN_PHAM', 'SanPham', a.insertId, r.body.name, c);
      return { id: a.insertId };
    }));
  }

  // Sequence 3.1: đọc bản ghi hiện tại, kiểm tra dữ liệu, lưu thông tin và cập nhật trạng thái trong một giao dịch.
  async updateProduct(r) {
    return productWrite(() => tx(async (c) => {
      const id = positive(r.params.id),
        body = r.body;
      const [p] = await repository.lockProduct([id], c);
      if (!p) fail('Không tìm thấy sản phẩm.', 404);
      const status = productStatus(body.status ?? p.status);
      const [photo] = await repository.lockImage([id], c);
      p.image = photo?.image ?? '';
      const changed = ['code', 'name', 'description', 'price', 'categoryId', 'brandId', 'image'].some(
        (key) => key in body && String(body[key] ?? '') !== String(p[key] ?? ''),
      );
      if (changed) {
        const data = { ...p, ...body };
        await repository.updateProduct(
          [
            required(data.code, 'mã sản phẩm'),
            required(data.name, 'tên'),
            text(data.description, 8000),
            price(data.price),
            positive(data.categoryId),
            positive(data.brandId),
            id,
          ],
          c,
        );
        if ('image' in body && body.image !== p.image) {
          const image = productImage(body.image);
          if (photo && photo.variant == null) {
            if (image) await repository.updateImage([image, photo.id], c);
            else await repository.removeImage([photo.id], c);
          } else if (image) {
            await repository.clearPrimaryImage([id], c);
            await repository.insertImage([id, image], c);
          }
        }
      }
      await repository.setProductStatus([status, id], c);
      await audit(r.user, 'SUA_SAN_PHAM', 'SanPham', id, body.name ?? p.name, c);
      return { ok: true };
    }));
  }

  // Sequence 3.2: CatalogController nhận sản phẩm đã chọn và truy vấn đúng các biến thể qua repository.
  async listVariants(r) {
    const { productId } = r.query;
    return repository.variants(productId == null ? undefined : positive(productId));
  }

  async createVariant(r) {
    return variantWrite(() => tx(async (c) => {
      const a = await repository.insertVariant(
        [
          positive(r.body.productId),
          required(r.body.sku, 'SKU'),
          positive(r.body.sizeId),
          positive(r.body.colorId),
          r.body.price ? price(r.body.price) : null,
        ],
        c,
      );
      await repository.createStock([a.insertId], c);
      await audit(r.user, 'TAO_BIEN_THE', 'BienTheSanPham', a.insertId, r.body.sku, c);
      return { id: a.insertId };
    }));
  }

  async updateVariant(r) {
    await variantWrite(() => tx(async (c) => {
      const [old] = await repository.lockVariant([positive(r.params.id)], c);
      if (!old) fail('Không tìm thấy biến thể.', 404);
      const active = Number(r.body.active);
      if (![0, 1].includes(Number(r.body.active)) || r.body.active == null)
        fail('Trạng thái không hợp lệ.');
      if (old.DangKinhDoanh && ['sizeId', 'colorId', 'price', 'sku'].some((key) => key in r.body))
        fail('Hãy ẩn biến thể trước khi sửa.', 409);
      if (r.body.sizeId && r.body.colorId) {
        const [{ n }] = await repository.countVariantOrders([old.BienTheID], c);
        if (n && (Number(r.body.sizeId) !== old.KichCoID || Number(r.body.colorId) !== old.MauSacID))
          fail('Biến thể đã giao dịch không được đổi size/màu; hãy tạo biến thể mới.');
        await repository.updateVariant(
          [
            positive(r.body.sizeId),
            positive(r.body.colorId),
            r.body.price ? price(r.body.price) : null,
            old.BienTheID,
          ],
          c,
        );
      }
      await repository.setVariantStatus([active, old.BienTheID], c);
      await audit(r.user, 'AN_HIEN_BIEN_THE', 'BienTheSanPham', r.params.id, String(Boolean(active)), c);
    }));
    return { ok: true };
  }

  async deleteVariant(r) {
    return tx(async (c) => {
      const id = positive(r.params.id);
      const [variant] = await repository.lockVariantIdentity([id], c);
      if (!variant) fail('Không tìm thấy biến thể.', 404);
      if (variant.DangKinhDoanh) fail('Hãy ẩn biến thể trước khi xóa.', 409);
      const [stock] = await repository.lockStock([id], c);
      if (stock && (stock.SoLuongTon || stock.SoLuongDaPhanBo))
        fail('Biến thể còn tồn kho hoặc đang giữ hàng.', 409);
      try {
        await repository.removeStock([id], c);
        await repository.removeVariant([id], c);
      } catch (error) {
        if (error.code === 'ER_ROW_IS_REFERENCED_2')
          fail('Biến thể còn liên kết với dữ liệu cửa hàng nên chưa thể xóa.', 409);
        throw error;
      }
      await audit(r.user, 'XOA_BIEN_THE', 'BienTheSanPham', id, variant.SKU, c);
      return { ok: true };
    });
  }

  async deleteProduct(r) {
    return deleteCatalogDraft(r, 'SanPham', 'SanPhamID');
  }

  async createTaxonomy(r, table) {
    const a = await repository.createTaxonomy(table, [
      required(r.body.name, 'tên'),
      text(r.body.description),
      ...taxonomyState(r.body, true),
    ]);
    await audit(r.user, 'TAO_DANH_MUC', table, a.insertId, r.body.name);
    return { id: a.insertId };
  }

  async deleteTaxonomy(r, table, col) {
    return deleteCatalogDraft(r, table, col);
  }

  async updateTaxonomy(r, table, col) {
    await repository.updateTaxonomy(table, col, [
      required(r.body.name, 'tên'),
      ...taxonomyState(r.body, false),
      positive(r.params.id),
    ]);
    await audit(r.user, 'SUA_DANH_MUC', table, r.params.id, r.body.name);
    return { ok: true };
  }
}
