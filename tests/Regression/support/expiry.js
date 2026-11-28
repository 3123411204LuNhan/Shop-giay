// Backend C# tự xử lý hết hạn bằng BackgroundService mỗi 15 giây.
export async function expireUnpaidOrders(){await new Promise(r=>setTimeout(r,17000));return 0;}
