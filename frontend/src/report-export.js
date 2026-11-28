export const reportTypes={
  summary:'Báo cáo kinh doanh tổng hợp',
  revenue:'Tổng hợp doanh thu',
  orders:'Số đơn theo trạng thái',
  'best-sellers':'Sản phẩm bán chạy',
};

export function reportCriteria(report){
  return (report.group==='variant'?'Theo biến thể':'Theo sản phẩm')+
    ' · '+(report.rank==='amount'?'Xếp theo giá trị thực thu':'Xếp theo số lượng bán ròng');
}

export function reportCSV(report,statusLabels){
  const rows=[['Loại báo cáo',reportTypes[report.type]],['Từ ngày',report.from],['Đến ngày',report.to],
    ['Múi giờ kỳ báo cáo','UTC+07:00'],['Thời điểm tổng hợp (UTC)',report.generatedAt],['Đơn vị tiền',report.currency]];
  if(report.daily){
    rows.push([],['Thu đã xác nhận',report.income],['Hoàn đã xác nhận',report.refund],['Doanh thu thuần',report.revenue],
      [],['Ngày xác nhận','Thu đã xác nhận','Hoàn đã xác nhận','Doanh thu thuần']);
    for(const row of report.daily)rows.push([row.day,row.income,row.refund,row.revenue]);
  }
  if(report.orders){
    rows.push([],['Tiêu chí thống kê đơn','Ngày tạo đơn trong kỳ'],['Trạng thái đơn','Số đơn']);
    for(const row of report.orders)rows.push([statusLabels[row.status]??row.status,row.count]);
  }
  if(report.top){
    rows.push([],['Tiêu chí xếp hạng',reportCriteria(report)],['Kỳ xếp hạng','Ngày hoàn thành của đơn đã thu đủ'],
      [report.group==='variant'?'SKU':'Mã sản phẩm','Tên','Số lượng bán ròng','Giá trị thực thu (VND)']);
    for(const row of report.top)rows.push([row.code,row.name,row.quantity,row.amount]);
  }
  const cell=value=>{
    let text=String(value??'');
    if(typeof value==='string'&&(/^[\s\uFEFF]*[=+@-]/.test(text)||/^[\t\r\n]/.test(text)))text="'"+text;
    return '"'+text.replaceAll('"','""')+'"';
  };
  return rows.map(row=>row.map(cell).join(',')).join('\r\n');
}

export function downloadReport(report,statusLabels){
  const bytes=new Blob(['\uFEFF',reportCSV(report,statusLabels)],{type:'text/csv;charset=utf-8'});
  const url=URL.createObjectURL(bytes);
  const link=document.createElement('a');
  link.href=url;
  link.download='bao-cao-'+report.type+'-'+report.from+'-'+report.to+'.csv';
  link.click();
  setTimeout(()=>URL.revokeObjectURL(url),1000);
}
