import React,{useState,useEffect} from 'react';

const money=value=>new Intl.NumberFormat('vi-VN',{style:'currency',currency:'VND'}).format(Number(value||0));
const when=value=>value?new Date(value).toLocaleString('vi-VN'):'—';
const typeNames={NHAP:'Nhập hàng',DIEU_CHINH:'Điều chỉnh',NHAN_TRA:'Nhận hàng hoàn'};
const receiptNames={NHAP:'Phiếu nháp',DA_XAC_NHAN:'Đã ghi sổ',DA_HUY:'Đã hủy'};

// CheckoutReview gọi CheckoutController để báo giá và đặt đơn; CheckoutService xử lý cả hai trên cùng quy tắc tính tiền.
export function CheckoutReview({api,Form,fields,user,requestId,perform,busy,onCreated,initialPromo=''}) {
  const [draft,setDraft]=useState({name:user?.name,phone:user?.phone,address:user?.address,method:'COD',promo:initialPromo});
  const [quote,setQuote]=useState(null);
  return <>
    {!quote?<><p className="subtle">Kiểm tra giá, ưu đãi và phí giao hàng trước khi xác nhận đặt. Giỏ hàng chưa giữ hàng.</p><Form busy={busy} fields={fields} initial={draft} label="Kiểm tra tổng tiền" submit={v=>perform(async()=>{setDraft(v);setQuote(await api('/checkout/quote',v));},'')}/></>:<>
      <h3>Kiểm tra đơn hàng</h3>
      {quote.items.map(x=><div className="order-item" key={x.variantId}><div><b>{x.name}</b><p>EU {x.size} · {x.color} · {x.quantity} đôi</p></div><strong>{money(x.price*x.quantity)}</strong></div>)}
      <p>{draft.name} · {draft.phone}<br/>{draft.address}</p>
      <p>{draft.method==='COD'?'Thanh toán khi nhận hàng':'Chuyển khoản — quản lý đối soát tiền thực nhận'}</p>
      <div className="workflow-summary"><p>Tiền hàng <b>{money(quote.subtotal)}</b></p><p>Giảm giá <b>−{money(quote.discount)}</b></p><p>Phí giao hàng <b>{money(quote.shipping)}</b></p><p>Tổng thanh toán <strong>{money(quote.total)}</strong></p></div>
      <div className="actions"><button className="button primary" disabled={busy} onClick={()=>perform(async()=>{try{await onCreated(await api('/orders',{...draft,requestId,fingerprint:quote.fingerprint}));}catch(e){setQuote(null);throw e;}},'')}>Xác nhận đặt hàng</button><button className="button light" disabled={busy} onClick={()=>setQuote(null)}>Sửa thông tin</button></div>
    </>}
  </>;
}

export function ReceiptWorkspace({api,perform,busy,rows,reload,Modal,DataTable}) {
  const [receipt,setReceipt]=useState(null),[variants,setVariants]=useState([]),[editing,setEditing]=useState(false);
  useEffect(()=>{api('/manage/variants').then(setVariants).catch(()=>{});},[]);
  const create=()=>{const variantId=Number(new URLSearchParams(location.search).get('variant'))||'';setReceipt({type:'NHAP',supplier:'',reference:'',reason:'',requestId:crypto.randomUUID(),items:[{variantId,quantity:1,cost:''}]});setEditing(true);};
  const open=id=>perform(async()=>{setReceipt(await api('/manage/receipts/'+id));setEditing(false);},'');
  const mutate=(key,value)=>setReceipt({...receipt,[key]:value});
  const lineChange=(index,key,value)=>setReceipt({...receipt,items:receipt.items.map((x,i)=>i===index?{...x,[key]:value}:x)});
  const variantLabel=v=>v?v.sku+' · '+v.name+' · EU '+v.size+' · '+v.color:'';
  const selectVariant=(index,query)=>{
    const typed=query.trim().toLowerCase();
    const match=variants.find(v=>variantLabel(v).toLowerCase()===typed||v.sku.toLowerCase()===typed);
    setReceipt(old=>({...old,items:old.items.map((line,i)=>i===index?{...line,variantId:match?.id??'',variantQuery:match?variantLabel(match):query}:line)}));
  };
  const action=(path,body)=>perform(async()=>{await api('/manage/receipts/'+receipt.id+'/'+path,body);setReceipt(await api('/manage/receipts/'+receipt.id));await reload();},'Đã cập nhật phiếu kho.');
  return <>
    <div className="workflow-toolbar receipt-toolbar"><p>Nháp chưa thay đổi tồn. Xác nhận phiếu sau khi đã kiểm hàng thực nhận.</p><button className="button dark" onClick={create}>Lập phiếu kho</button></div>
    <DataTable rows={rows} columns={[["code","Mã phiếu"],['type','Nghiệp vụ',v=>typeNames[v]],['status','Trạng thái',v=><span className="badge">{receiptNames[v]}</span>],['supplier','Nhà cung cấp'],['reference','Chứng từ'],['date','Ngày lập',when],['creator','Người lập']]} actions={r=><button className="small-button" onClick={()=>open(r.id)}>Chi tiết phiếu</button>}/>
    {receipt&&<Modal title={receipt.id?'Phiếu '+receipt.code:'Lập phiếu kho'} wide close={()=>{setReceipt(null);setEditing(false);}}>
      {editing?<form className="form receipt-form" onSubmit={e=>{e.preventDefault();perform(async()=>{const result=await api('/manage/receipts'+(receipt.id?'/'+receipt.id:''),{...receipt,items:receipt.items.map(({variantId,quantity,cost})=>({variantId,quantity,cost}))},receipt.id?'PUT':'POST');setReceipt(await api('/manage/receipts/'+result.id));setEditing(false);await reload();},'Đã lưu nháp. Tồn kho chưa thay đổi.');}}>
        <label>Nghiệp vụ<select value={receipt.type} onChange={e=>mutate('type',e.target.value)}><option value="NHAP">Nhập hàng thực nhận</option><option value="DIEU_CHINH">Điều chỉnh chênh lệch (+/−)</option></select></label>
        <div className="workflow-fields"><label>Nhà cung cấp<input required={receipt.type==='NHAP'} maxLength={200} value={receipt.supplier||''} onChange={e=>mutate('supplier',e.target.value)}/></label><label>Mã chứng từ tham chiếu<input required={receipt.type==='NHAP'} maxLength={100} value={receipt.reference||''} onChange={e=>mutate('reference',e.target.value)}/></label></div>
        <label>Lý do / ghi chú kiểm hàng<textarea required value={receipt.reason} onChange={e=>mutate('reason',e.target.value)}/></label>
        <p className="fine">Nhập số lượng thực nhận đạt yêu cầu. Điều chỉnh nhập phần chênh lệch có dấu; hàng đã phân bổ không được giảm quá tồn khả dụng.</p>
        <datalist id="receipt-variant-options">{variants.map(v=><option key={v.id} value={variantLabel(v)}/>)}</datalist>
        {receipt.items.map((item,i)=><div className="receipt-line" key={i}>
          <label>Biến thể<input required list="receipt-variant-options" autoComplete="off" placeholder="Gõ SKU, tên giày, size hoặc màu…" value={item.variantQuery??variantLabel(variants.find(v=>v.id===Number(item.variantId)))} onChange={e=>selectVariant(i,e.target.value)} ref={el=>el?.setCustomValidity(item.variantQuery&&!item.variantId?'Hãy chọn một biến thể trong danh sách gợi ý.':'')}/></label>
          <label>Số lượng<input type="number" required step="1" min={receipt.type==='NHAP'?1:-100000} max="100000" value={item.quantity} onChange={e=>lineChange(i,'quantity',e.target.value)}/></label>
          <label>Đơn giá nhập<input type="number" min="0" step="1" value={item.cost??''} onChange={e=>lineChange(i,'cost',e.target.value)}/></label>
          <button type="button" className="small-button" disabled={receipt.items.length===1} onClick={()=>mutate('items',receipt.items.filter((_,n)=>n!==i))}>Bỏ dòng</button>
        </div>)}
        <div className="actions"><button type="button" className="button light" onClick={()=>mutate('items',[...receipt.items,{variantId:'',quantity:1,cost:''}])}>Thêm dòng</button><button className="button primary" disabled={busy}>Lưu phiếu nháp</button></div>
      </form>:<>
        <p><b>{receiptNames[receipt.status]}</b> · {typeNames[receipt.type]}<br/>{receipt.supplier||'—'} · Chứng từ: {receipt.reference||'—'}</p><p>{receipt.reason}</p>
        <DataTable rows={receipt.items} columns={[["sku","SKU"],['name','Sản phẩm'],['size','Size'],['color','Màu'],['quantity','Số lượng'],['cost','Giá nhập',money]]}/>
        {receipt.status==='NHAP'?<><p className="fine">Chỉ xác nhận sau khi đã kiểm đủ hàng và số lượng trên phiếu. Hệ thống ghi sổ một lần.</p><div className="actions"><button className="button primary" disabled={busy} onClick={()=>action('confirm',{})}>Xác nhận ghi sổ kho</button><button className="button light" onClick={()=>setEditing(true)}>Sửa nháp</button><button className="button danger" disabled={busy} onClick={()=>action('cancel',{reason:'Quản lý hủy phiếu nháp'})}>Hủy nháp</button></div></>:<p className="fine">Phiếu đã ghi sổ được giữ nguyên. Nếu sai số lượng, lập phiếu điều chỉnh kèm lý do và mã phiếu gốc.</p>}
      </>}
    </Modal>}
  </>;
}

export function ReturnDetail({data,Modal,DataTable,close,statusNames}) {
  return <Modal title={'Yêu cầu '+data.code} close={close} wide>
    <p><b>{statusNames[data.status]||data.status}</b> · {data.reason}</p>{data.decision&&<p>Shop phản hồi: {data.decision}</p>}
    <DataTable rows={data.items} columns={[["name","Sản phẩm"],['quantity','Yêu cầu'],['approved','Được duyệt'],['received','Đã nhận'],['sellable','Bán lại'],['refund','Tiền hoàn được duyệt',money]]}/>
    <h4>Hoàn tiền</h4>{data.refunds.length?data.refunds.map((x,i)=><p key={i}>{money(x.amount)} · {statusNames[x.status]||x.status} · {when(x.confirmedAt)}</p>):<p>Chưa có khoản hoàn. Shop sẽ xử lý sau khi nhận và kiểm hàng.</p>}
    <h4>Lịch sử xử lý</h4><div className="timeline">{data.history.map((x,i)=><div key={i}><i/><b>{statusNames[x.status]||x.status}</b><span>{when(x.date)}</span><p>{x.note}</p></div>)}</div>
  </Modal>;
}
