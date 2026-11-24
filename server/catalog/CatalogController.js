import { imageForm, sendUploadedImage } from '../image-upload.js';
import { CatalogService } from './CatalogService.js';

// Ánh xạ API sản phẩm, màu, size, biến thể, danh mục và thương hiệu tới CatalogService.
export class CatalogController {
  constructor(service = new CatalogService()) {
    this.service = service;
  }

  registerPublic(router, { run }) {
    router.get('/product-images/:name',(req,res,next)=>sendUploadedImage('products',req.params.name,res,next));
    router.get(
      '/catalog',
      run((r) => this.service.browse(r)),
    );
    router.get(
      '/products/:id',
      run((r) => this.service.details(r)),
    );
  }

  register(router, { run, manager }) {
    router.get(
      '/manage/products',
      manager,
      run(() => this.service.listProducts()),
    );
    router.post(
      '/manage/sizes',
      manager,
      run((r) => this.service.createSize(r)),
    );
    router.post(
      '/manage/colors',
      manager,
      run((r) => this.service.createColor(r)),
    );
    router.get(
      '/manage/lookups',
      manager,
      run(() => this.service.lookups()),
    );
    router.post(
      '/manage/products',
      manager,
      imageForm,
      run((r) => this.service.createProduct(r)),
    );
    router.put(
      '/manage/products/:id',
      manager,
      imageForm,
      run((r) => this.service.updateProduct(r)),
    );
    router.get(
      '/manage/variants',
      manager,
      run((r) => this.service.listVariants(r)),
    );
    router.post(
      '/manage/variants',
      manager,
      run((r) => this.service.createVariant(r)),
    );
    router.put(
      '/manage/variants/:id',
      manager,
      run((r) => this.service.updateVariant(r)),
    );
    router.delete(
      '/manage/variants/:id',
      manager,
      run((r) => this.service.deleteVariant(r)),
    );
    router.delete(
      '/manage/products/:id',
      manager,
      run((r) => this.service.deleteProduct(r)),
    );
    for (const [route, table, col] of [
      ['categories', 'DanhMuc', 'DanhMucID'],
      ['brands', 'ThuongHieu', 'ThuongHieuID'],
    ]) {
      router.post(
        '/manage/' + route,
        manager,
        run((r) => this.service.createTaxonomy(r, table)),
      );
      router.delete(
        '/manage/' + route + '/:id',
        manager,
        run((r) => this.service.deleteTaxonomy(r, table, col)),
      );
      router.put(
        '/manage/' + route + '/:id',
        manager,
        run((r) => this.service.updateTaxonomy(r, table, col)),
      );
    }
  }
}
