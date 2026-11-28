import React,{useState} from 'react';
import {Download} from 'lucide-react';
import {downloadReport,reportCriteria,reportTypes} from './report-export.js';

function reportTime(value){
  return new Intl.DateTimeFormat('vi-VN',{timeZone:'Asia/Ho_Chi_Minh',dateStyle:'short',timeStyle:'medium'}).format(new Date(value));
}

export function ReportWorkspace({api,DataTable,statusLabels,money}){
  const [criteria,setCriteria]=useState(()=>{
    const now=Date.now()+7*3600000;
    return {type:'summary',group:'product',rank:'quantity',
      from:new Date(now-30*86400000).toISOString().slice(0,10),to:new Date(now).toISOString().slice(0,10)};
  });
  const [report,setReport]=useState(null),[loading,setLoading]=useState(false),[error,setError]=useState('');
  const change=(key,value)=>{setCriteria(old=>({...old,[key]:value}));setReport(null);setError('');};
  const load=async event=>{
    event.preventDefault();
    if(loading)return;
    setReport(null);setError('');setLoading(true);
    try{setReport(await api('/manage/reports?'+new URLSearchParams(criteria)));}
    catch(e){setError(e.message);}
    finally{setLoading(false);}
  };
  const hasRanking=criteria.type==='summary'||criteria.type==='best-sellers';

  return <>
    <form className="panel report-form form" aria-label="Tiêu chí báo cáo" onSubmit={load}>
      <fieldset className="report-criteria" disabled={loading}>
        <label>Từ ngày<input type="date" required value={criteria.from} onChange={e=>change('from',e.target.value)}/></label>
        <label>Đến ngày<input type="date" required value={criteria.to} onChange={e=>change('to',e.target.value)}/></label>
        <label>Loại báo cáo<select value={criteria.type} onChange={e=>change('type',e.target.value)}>
          {Object.entries(reportTypes).map(([value,label])=><option key={value} value={value}>{label}</option>)}
        </select></label>
        {hasRanking&&<>
          <label>Nhóm bán chạy<select value={criteria.group} onChange={e=>change('group',e.target.value)}>
            <option value="product">Theo sản phẩm</option><option value="variant">Theo biến thể</option>
          </select></label>
          <label>Xếp hạng theo<select value={criteria.rank} onChange={e=>change('rank',e.target.value)}>
            <option value="quantity">Số lượng bán ròng</option><option value="amount">Giá trị thực thu</option>
          </select></label>
        </>}
        <button className="button dark" type="submit">{loading?'Đang tổng hợp…':'Xem báo cáo'}</button>
      </fieldset>
    </form>
    {loading&&<p role="status">Đang tổng hợp dữ liệu…</p>}
    {error&&<p className="error-box" role="alert">{error}</p>}
    {report&&<section className="panel" aria-label="Kết quả báo cáo">
      <div className="panel-head"><div><h3>{reportTypes[report.type]}</h3>
        <p>{report.from} đến {report.to} · Giờ Việt Nam · {report.currency}</p>
        <p>Tổng hợp lúc {reportTime(report.generatedAt)}</p>
      </div><button className="small-button" onClick={()=>downloadReport(report,statusLabels)}><Download size={15}/>Xuất CSV</button></div>
      {report.daily&&<section className="report-section"><h3>Doanh thu</h3><p>Tính theo ngày xác nhận khoản thu và khoản hoàn.</p>
        <dl className="report-totals"><div><dt>Thu đã xác nhận</dt><dd>{money(report.income)}</dd></div>
          <div><dt>Hoàn đã xác nhận</dt><dd>{money(report.refund)}</dd></div><div><dt>Doanh thu thuần</dt><dd>{money(report.revenue)}</dd></div></dl>
        <DataTable rows={report.daily} columns={[
          ['day','Ngày xác nhận'],['income','Thu đã xác nhận',money],['refund','Hoàn đã xác nhận',money],['revenue','Doanh thu thuần',money]
        ]}/>
      </section>}
      {report.orders&&<section className="report-section"><h3>Số đơn theo trạng thái</h3><p>Ngày tạo đơn trong kỳ · Tổng {report.orders.reduce((sum,row)=>sum+row.count,0)} đơn</p>
        <DataTable rows={report.orders} columns={[["status","Trạng thái",v=>statusLabels[v]??v],['count','Số đơn']]}/>
      </section>}
      {report.top&&<section className="report-section"><h3>Sản phẩm bán chạy</h3><p>{reportCriteria(report)}</p>
        <p>Tính theo ngày hoàn thành của đơn đã thu đủ. Giá trị hàng sau giảm giá và hoàn tiền, chưa gồm phí giao.</p>
        <DataTable rows={report.top} columns={[["code",report.group==='variant'?'SKU':'Mã sản phẩm'],['name','Tên'],['quantity','Số đôi bán ròng'],['amount','Giá trị thực thu',money]]}/>
      </section>}
    </section>}
  </>;
}
