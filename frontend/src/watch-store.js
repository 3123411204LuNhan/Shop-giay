// Also refresh a storefront left open on another device after a shop edit.
export function watchStore(refresh) {
  const whenVisible = () => {
    if (document.visibilityState === 'visible') refresh();
  };
  document.addEventListener('visibilitychange', whenVisible);
  const onStorage = event => {
    if (event.key === 'sole:promotions-updated') whenVisible();
  };
  window.addEventListener('storage', onStorage);
  const timer = window.setInterval(whenVisible, 30000);
  return () => {
    document.removeEventListener('visibilitychange', whenVisible);
    window.removeEventListener('storage', onStorage);
    window.clearInterval(timer);
  };
}
