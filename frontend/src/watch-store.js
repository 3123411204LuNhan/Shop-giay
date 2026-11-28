const catalogEvent = 'sole:catalog-updated';

// Sau khi lưu sản phẩm, báo cho các trang khách đang mở trong cùng trình duyệt.
export function announceStoreUpdate() {
  window.dispatchEvent(new Event(catalogEvent));
  try { localStorage.setItem(catalogEvent, crypto.randomUUID()); } catch { /* Vẫn đồng bộ trong tab khi storage bị chặn. */ }
}

// Tab khác cập nhật ngay khi hiển thị; thiết bị khác tiếp tục kiểm tra mỗi 30 giây.
export function watchStore(refresh) {
  const whenVisible = () => {
    if (document.visibilityState === 'visible') refresh();
  };
  document.addEventListener('visibilitychange', whenVisible);
  const onStorage = event => {
    if (event.key === 'sole:promotions-updated' || event.key === catalogEvent) whenVisible();
  };
  window.addEventListener('storage', onStorage);
  window.addEventListener(catalogEvent, whenVisible);
  window.addEventListener('focus', whenVisible);
  const timer = window.setInterval(whenVisible, 30000);
  return () => {
    document.removeEventListener('visibilitychange', whenVisible);
    window.removeEventListener('storage', onStorage);
    window.removeEventListener(catalogEvent, whenVisible);
    window.removeEventListener('focus', whenVisible);
    window.clearInterval(timer);
  };
}
