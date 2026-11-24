import React from 'react';

export const emptyStockFilters={productId:'',color:'',size:'',reserved:'',available:'',active:'',low:false};

// Bộ lọc chạy trên danh sách biến thể do CatalogController trả về; số liệu tồn do InventoryService cập nhật qua phiếu kho.
export function matchesStock(row,f){
  return String(row.productId)===f.productId
    && (!f.color||String(row.colorId)===f.color)
    && (!f.size||String(row.sizeId)===f.size)
    && (!f.reserved||String(Number(row.reserved)>0)===f.reserved)
    && (!f.available||String(Number(row.available)>0)===f.available)
    && (!f.active||String(row.active)===f.active)
    && (!f.low||Number(row.available)<=Number(row.warning));
}

export function StockFilters({filters,change,products,lookups,reset}){
  const update=(key,value)=>change(old=>({...old,[key]:value}));
  const selectors=[
    ['color','Màu',lookups.colors],['size','Kích cỡ',lookups.sizes],
    ['reserved','Đang giữ',[{id:'true',name:'Có hàng đang giữ'},{id:'false',name:'Không giữ hàng'}]],
    ['available','Khả dụng',[{id:'true',name:'Còn hàng'},{id:'false',name:'Hết hàng'}]],
    ['active','Trạng thái',[{id:'1',name:'Đang bán'},{id:'0',name:'Ẩn'}]]
  ];
  return <div className="stock-filters">
    <label>Sản phẩm<select value={filters.productId} onChange={e=>update('productId',e.target.value)}>
      <option value="">Chọn sản phẩm để xem biến thể</option>{products.map(p=><option key={p.id} value={p.id}>{p.code} · {p.name}</option>)}
    </select></label>
    {selectors.map(([key,title,options])=><label key={key}>{title}<select value={filters[key]} onChange={e=>update(key,e.target.value)}>
      <option value="">Tất cả</option>{options.map(o=><option key={o.id} value={o.id}>{o.name}</option>)}
    </select></label>)}
    <div className="stock-filter-actions"><label><input type="checkbox" checked={filters.low} onChange={e=>update('low',e.target.checked)}/> Chỉ xem sắp hết</label>
      <button className="small-button" onClick={reset}>Xóa bộ lọc</button></div>
  </div>;
}
