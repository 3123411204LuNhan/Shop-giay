import React,{useState} from 'react';

export function VariantWorkspace({rows,products,productId,chooseProduct,lookups,DataTable,hide,edit,remove}){
  const [filters,setFilters]=useState({sizeId:'',colorId:'',active:''});
  const visible=rows.filter(r=>Object.entries(filters).every(([key,value])=>!value||String(r[key])===value));
  return <>
    <div className="stock-filters"><label>Sản phẩm<select value={productId} onChange={e=>chooseProduct(e.target.value)}>
      <option value="">Chọn sản phẩm để xem biến thể</option>
      {products.map(p=><option key={p.id} value={p.id}>{p.code} · {p.name}</option>)}
    </select></label>
      {productId&&[['sizeId','Size',lookups.sizes],['colorId','Màu',lookups.colors],['active','Kinh doanh',[{id:1,name:'Đang bán'},{id:0,name:'Đã ẩn'}]]].map(([key,label,options])=><label key={key}>{label}<select value={filters[key]} onChange={e=>setFilters({...filters,[key]:e.target.value})}>
        <option value="">Tất cả</option>
        {options.map(option=><option key={option.id} value={option.id}>{option.name}</option>)}
      </select></label>)}
    </div>
    {productId&&<DataTable rows={visible} columns={[['sku','SKU'],['size','EU'],['color','Màu'],
      ['price','Giá riêng (VND)',v=>v==null?'Theo giá sản phẩm':Number(v).toLocaleString('vi-VN')],
      ['active','Kinh doanh',v=>v?'Đang bán':'Đã ẩn']]}
      actions={r=>r.active?<button className="small-button" onClick={()=>hide(r)}>Ẩn</button>:<>
        <button className="small-button" onClick={()=>edit(r)}>Sửa</button>
        <button className="small-button" onClick={()=>remove(r)}>Xóa</button>
      </>}/>}
  </>;
}

// Form gửi size, màu và SKU tới CatalogController. CatalogService kiểm tra dữ liệu và ghi thông qua CatalogRepository.
export function VariantCreate({productId,products,lookups,setLookups,api,perform,busy,onSave}){
  const [values,setValues]=useState({productId,sku:'',sizeId:'',colorId:'',price:''});
  const [adding,setAdding]=useState(null);
  const set=(key,value)=>setValues(old=>({...old,[key]:value}));
  const options=items=>items.map(x=><option key={x.id} value={x.id}>{x.name}</option>);
  const saveOption=e=>{
    e.preventDefault();
    const data=Object.fromEntries(new FormData(e.currentTarget));
    const kind=adding;
    perform(async()=>{
      const item=await api('/manage/'+kind,data);
      setLookups(old=>({...old,[kind]:old[kind].some(x=>x.id===item.id)?old[kind]:[...old[kind],item]}));
      set(kind==='sizes'?'sizeId':'colorId',item.id);
      setAdding(null);
    },'Đã chọn giá trị cho biến thể.');
  };
  return <>
    <form className="form" onSubmit={e=>{e.preventDefault();if(!adding)onSave(values);}}>
      <label>Sản phẩm<select required value={values.productId} onChange={e=>set('productId',e.target.value)}><option value="">Chọn sản phẩm</option>{options(products)}</select></label>
      <label>SKU<input required maxLength={80} value={values.sku} onChange={e=>set('sku',e.target.value)}/></label>
      {[['sizes','sizeId','Kích cỡ','size'],['colors','colorId','Màu sắc','màu']].map(([kind,key,title,label])=><div key={key}>
        <label>{title}<select required value={values[key]} onChange={e=>set(key,e.target.value)}><option value="">Chọn {label}</option>{options(lookups[kind])}</select></label>
        <button type="button" className="text-button" disabled={busy} onClick={()=>setAdding(kind)}>+ Thêm {label}</button>
      </div>)}
      <label>Giá riêng (VND, tùy chọn)<input type="number" min="1" value={values.price} onChange={e=>set('price',e.target.value)}/></label>
      <button className="button primary" disabled={busy||!!adding}>Lưu biến thể</button>
    </form>
    {adding&&<form key={adding} className="form" onSubmit={saveOption}>
      {adding==='sizes'?<label>Size EU mới<input name="name" autoFocus required placeholder="Ví dụ: 42.5" inputMode="decimal"/></label>:<>
        <label>Tên màu mới<input name="name" autoFocus required maxLength={100}/></label>
        <label>Mã màu (tùy chọn)<input name="hex" placeholder="#55796A" pattern="#[a-fA-F0-9]{6}"/></label>
      </>}
      <div><button className="button primary" disabled={busy}>Lưu và chọn</button> <button type="button" className="button light" disabled={busy} onClick={()=>setAdding(null)}>Hủy</button></div>
    </form>}
  </>;
}
