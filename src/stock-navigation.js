export const stockPath='/manager/stock';

export function normalizeStockPath(path){
  const legacy=/^\/manager\/(receipts|stock-history)\/?(?=[?#]|$)/.exec(path);
  if(!legacy)return path;
  const url=new URL(path,'http://localhost');
  url.searchParams.set('view',legacy[1]==='receipts'?'receipts':'history');
  return stockPath+url.search+url.hash;
}
