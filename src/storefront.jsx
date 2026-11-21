import {watchStore} from './watch-store.js';
import React,{useEffect,useMemo,useState} from 'react';
import {ArrowRight,ArrowUpRight,Check,Copy,Heart,Plus,RefreshCw,Ticket,Truck} from 'lucide-react';

const money=n=>new Intl.NumberFormat('vi-VN',{style:'currency',currency:'VND'}).format(Number(n||0));
export const defaultFilters={q:'',category:'',brand:'',size:'',color:'',sort:'newest',max:''};
export function filtersFromUrl(search){const params=new URLSearchParams(search);return Object.fromEntries(Object.entries(defaultFilters).map(([key,value])=>[key,params.get(key)||value]));}
export function productsUrl(filters={}){const params=new URLSearchParams(Object.entries({...defaultFilters,...filters}).filter(([key,value])=>value&&value!==defaultFilters[key]));return '/products'+(params.size?'?'+params:'');}
function shuffle(items){const list=[...items];for(let i=list.length-1;i>0;i--){const j=Math.floor(Math.random()*(i+1));[list[i],list[j]]=[list[j],list[i]];}return list;}

export function ProductGrid({products,openProduct,favorite,wishlist=[]}){
 return <div className="product-grid">{products.map((p,i)=><article className="product-card" key={p.id}>
  <div className={'product-photo tint-'+i%4}>
   <button className="product-open" aria-label={'Xem chi tiết '+p.name} onClick={()=>openProduct(p.id)}><img src={p.image} alt={p.name} loading="lazy"/></button>
   <span className="product-badge">{p.stock>0?'SOLE SELECTION':'TẠM HẾT HÀNG'}</span>
   <button className={'favorite '+(wishlist.includes(p.id)?'saved':'')} aria-label={'Yêu thích '+p.name} aria-pressed={wishlist.includes(p.id)} onClick={()=>favorite(p.id)}><Heart size={18}/></button>
   <button className="quick-add" aria-label={'Chọn size '+p.name} onClick={()=>openProduct(p.id)}><Plus size={20}/></button>
  </div>
  <div className="product-meta"><span>{p.brand.toUpperCase()} <span>· {p.category.replace('Giày ','')}</span></span></div>
  <button className="product-name" onClick={()=>openProduct(p.id)}>{p.name}</button><div className="product-price">{money(p.price)}<small>{p.stock>0?'Còn hàng':'Tạm hết hàng'}</small></div>
 </article>)}</div>;
}

function categoryStory(name){
 if(/chạy|running/i.test(name))return {eyebrow:'MOVE YOUR WAY',title:'Nhẹ bước. Đi xa.',description:'Khám phá những lựa chọn cho nhịp chạy của bạn.',tone:'mint'};
 if(/classic|kinh điển/i.test(name))return {eyebrow:'ALWAYS A GOOD CHOICE',title:'Quen thuộc, luôn cuốn hút.',description:'Phom dáng kinh điển cho nhiều cách phối đồ.',tone:'sand'};
 return {eyebrow:'EVERYDAY, YOUR WAY',title:'Ra phố theo cách riêng.',description:'Một đôi giày dễ phối cho lịch trình mỗi ngày.',tone:'pink'};
}

export function Discover({api,browse,openProduct,favorite,wishlist}){
 const [data,setData]=useState(null),[error,setError]=useState(''),[edition,setEdition]=useState(0);
 const load=()=>{setError('');api('/catalog').then(setData).catch(e=>setError(e.message));};
 useEffect(()=>{let active=true;const refresh=()=>api('/catalog').then(result=>{if(active)setData(result);}).catch(e=>{if(active)setError(e.message);});refresh();const stop=watchStore(refresh);return()=>{active=false;stop();};},[api]);
 const picks=useMemo(()=>{
  if(!data)return {categories:[],products:[]};
  const categories=shuffle(data.categories.map(category=>({ ...category,products:data.products.filter(p=>String(p.categoryId)===String(category.id))})).filter(x=>x.products.length)).map(category=>({...category,feature:shuffle(category.products)[0]}));
  const available=data.products.filter(p=>p.stock>0);
  return {categories,products:shuffle(available.length?available:data.products).slice(0,6)};
 },[data,edition]);
 return <>
  <section className="discover-hero"><div className="discover-hero-copy"><div className="eyebrow">THE EVERYDAY EDIT · SOLE STUDIO</div><h1>Hôm nay,<br/>bạn muốn <em>đi đâu?</em></h1><p>Một nhịp chạy mới, một buổi dạo phố hay một phong cách quen thuộc. Bắt đầu từ đôi giày khiến bạn muốn bước ra ngoài.</p><button className="button dark" onClick={()=>document.getElementById('discover-topics')?.scrollIntoView({behavior:'smooth'})}>Tìm cảm hứng <ArrowUpRight size={18}/></button><span className="discover-caption">MỖI BƯỚC CHÂN. MỘT CHẤT RIÊNG.</span></div><div className="discover-hero-art"><span className="discover-word" aria-hidden="true">STEP<br/>OUT.</span><img src="/images/shoe-white.jpg" alt="Cảm hứng phối giày sneaker trắng"/><span className="discover-art-tag"><span className="tiny-dot"/> A NEW DAY. A NEW PAIR.</span></div></section>
  <section className="discover-section" id="discover-topics"><div className="section-top"><div><div className="eyebrow muted">CHỌN MỘT NHỊP ĐIỆU</div><h2>Điều gì hợp với bạn hôm nay?</h2><p>Ghé qua những nhóm giày khác nhau. Một lựa chọn mới có thể đang chờ bạn.</p></div><button className="button light" disabled={!data} onClick={()=>setEdition(n=>n+1)}><RefreshCw size={16}/>Đổi gợi ý</button></div>
   {error?<div className="error-box">{error}<button onClick={load}>Thử lại</button></div>:!data?<div className="topic-grid" aria-label="Đang tải gợi ý">{[1,2,3].map(n=><div className="skeleton topic-loading" key={n}/>)}</div>:picks.categories.length?<div className="topic-grid">{picks.categories.map(category=>{const story=categoryStory(category.name);return <button key={category.id} className={'topic-card '+story.tone} onClick={()=>browse({category:String(category.id)})} aria-label={'Khám phá '+category.name}><div className="topic-card-top"><span>{story.eyebrow}</span><ArrowUpRight size={22}/></div><img src={category.feature.image} alt={category.feature.name}/><div className="topic-copy"><small>{category.name} · {category.products.length} mẫu</small><h3>{story.title}</h3><p>{story.description}</p></div></button>;})}</div>:<p>Shop đang cập nhật bộ sưu tập. Hãy quay lại sau nhé.</p>}
  </section>
  {picks.products.length>0&&<section className="discover-section discovery-picks"><div className="section-top"><div><div className="eyebrow muted">MỘT CHÚT BẤT NGỜ CHO TỦ GIÀY</div><h2>Thử một lựa chọn mới.</h2><p>Những mẫu được chọn ngẫu nhiên cho lần khám phá này.</p></div><button className="text-button" onClick={()=>browse()}>Xem tất cả sản phẩm <ArrowRight size={16}/></button></div><ProductGrid products={picks.products} openProduct={openProduct} favorite={favorite} wishlist={wishlist}/></section>}
  <section className="club-invitation"><div><span className="eyebrow">RUNNING CLUB</span><h2>Cảm hứng mới.<br/>Ưu đãi cho bước tiếp theo.</h2><p>Ghé không gian quảng cáo và mã giảm giá của SOLE STUDIO.</p></div><button className="button primary" onClick={()=>browse(null,'/running-club')}>Khám phá ưu đãi <ArrowUpRight size={18}/></button></section>
 </>;
}

export function RunningClub({api,browse,usePromo,notify}){
 const [promos,setPromos]=useState([]),[loading,setLoading]=useState(true),[error,setError]=useState(''),[copied,setCopied]=useState('');
 const load=()=>{setLoading(true);setError('');api('/promotions').then(setPromos).catch(e=>setError(e.message)).finally(()=>setLoading(false));};
 useEffect(()=>{let active=true;const refresh=()=>{api('/promotions').then(items=>{if(active){setPromos(items);setError('');}}).catch(e=>{if(active)setError(e.message);}).finally(()=>{if(active)setLoading(false);});};refresh();const stop=watchStore(refresh);return()=>{active=false;stop();};},[api]);
 const copy=async code=>{try{await navigator.clipboard.writeText(code);setCopied(code);notify('Đã sao chép mã '+code+'.');}catch{notify('Chưa sao chép được. Bạn có thể chọn và sao chép mã trực tiếp.');}};
 const valid=promos.filter(p=>!p.end||new Date(p.end)>new Date());
 return <div className="club-page">
  <section className="club-hero"><div className="club-hero-copy"><span className="eyebrow"><span className="tiny-dot"/> SOLE STUDIO · RUNNING CLUB</span><h1>Thêm cảm hứng.<br/><em>Thêm lý do để bước.</em></h1><p>Không gian chiến dịch, những câu chuyện phong cách và mã ưu đãi cho đôi giày tiếp theo của bạn.</p><a className="button primary" href="#club-offers">Xem mã giảm giá <Ticket size={17}/></a><small>Ưu đãi được áp dụng theo điều kiện của từng chương trình.</small></div><div className="club-hero-art"><span aria-hidden="true">KEEP<br/>MOVING.</span><img src="/images/shoe-green.jpg" alt="Giày sneaker trong chiến dịch phong cách SOLE STUDIO"/><div className="club-art-label">YOUR NEXT STEP STARTS HERE <ArrowUpRight size={22}/></div></div></section>
  <section className="club-campaigns" aria-label="Chiến dịch nổi bật"><button className="campaign-card campaign-cream" onClick={()=>browse()}><div><span className="eyebrow muted">THE EVERYDAY COLLECTION</span><h2>Mặc đơn giản.<br/>Bước thật chất.</h2><p>Gặp những đôi sneaker dễ phối trong bộ sưu tập hiện tại.</p><span className="text-button">Xem bộ sưu tập <ArrowUpRight size={17}/></span></div><img src="/images/shoe-cream.jpg" alt="Cảm hứng sneaker màu kem"/></button><div className="campaign-shipping"><Truck size={30}/><span className="eyebrow muted">ĐI THÊM MỘT BƯỚC</span><h3>Miễn phí<br/>giao hàng.</h3><p>Cho đơn từ <b>1.500.000đ</b>.<br/>Đơn dưới mức này: 30.000đ.</p><button className="text-button" onClick={()=>browse()}>Chọn đôi giày của bạn <ArrowRight size={16}/></button></div></section>
  <section className="club-offers" id="club-offers"><div className="section-top"><div><div className="eyebrow muted">MÃ NHỎ. NIỀM VUI LỚN.</div><h2>Ưu đãi đang chờ bạn.</h2><p>Sao chép mã hoặc chọn “Mua với mã này” để điền sẵn khi đặt hàng.</p></div><span className="club-offer-count">{loading?'Đang cập nhật':valid.length+' ưu đãi đang áp dụng'}</span></div>
   {error?<div className="error-box">{error}<button onClick={load}>Thử lại</button></div>:loading?<p role="status">Đang tải mã ưu đãi…</p>:valid.length?<div className="coupon-grid">{valid.map(p=><article className="coupon" key={p.code}><div className="coupon-top"><Ticket size={20}/><span>ƯU ĐÃI CỦA SHOP</span></div><h3>{p.type==='PHAN_TRAM'?'Giảm '+p.value+'%':'Giảm '+money(p.value)}</h3><p className="coupon-name">{p.name}</p><ul><li>{p.type==='PHAN_TRAM'?'Giảm '+p.value+'% trên tiền hàng đủ điều kiện':'Giảm trực tiếp '+money(p.value)+' trên tiền hàng đủ điều kiện'}</li><li>Đơn hàng từ {money(p.minimum)}</li>{p.maximum!=null&&<li>Giảm tối đa {money(p.maximum)}</li>}<li>{({TOAN_BO:'Áp dụng toàn bộ sản phẩm',SAN_PHAM:'Áp dụng sản phẩm được chọn',DANH_MUC:'Áp dụng danh mục được chọn',THUONG_HIEU:'Áp dụng thương hiệu được chọn'})[p.scope]||'Theo điều kiện chương trình'}</li>{p.perCustomer&&<li>Tối đa {p.perCustomer} lượt / khách</li>}{p.end&&<li>Đến hết thời điểm {new Date(p.end).toLocaleString('vi-VN')}</li>}</ul><div className="coupon-code"><code>{p.code}</code><button aria-label={'Sao chép mã '+p.code} onClick={()=>copy(p.code)}>{copied===p.code?<Check size={15}/>:<Copy size={15}/>}<span>{copied===p.code?'Đã sao chép':'Sao chép'}</span></button></div><button className="button dark" onClick={()=>usePromo(p.code)}>Mua với mã này <ArrowRight size={16}/></button></article>)}</div>:<div className="club-empty"><Ticket size={32}/><h3>Hẹn bạn ở ưu đãi tiếp theo.</h3><p>Hiện chưa có mã giảm giá đang áp dụng. Bạn vẫn có thể khám phá bộ sưu tập của shop.</p><button className="button light" onClick={()=>browse()}>Xem sản phẩm <ArrowRight size={16}/></button></div>}
   <p className="club-terms">Mỗi đơn dùng một mã. Ưu đãi được kiểm tra lại theo sản phẩm, tài khoản, thời hạn và lượt sử dụng ở bước đặt hàng.</p>
  </section>
 </div>;
}
