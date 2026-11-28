const guestKey='sole:guest-cart:v2';

export function readGuestCart(storage=sessionStorage,legacy=localStorage){
  // Never import a legacy, shared-device cart into any account.
  legacy.removeItem('sole-cart');
  try{const cart=JSON.parse(storage.getItem(guestKey)||'[]');return Array.isArray(cart)?cart:[];}catch{return [];}
}
export function writeGuestCart(cart,storage=sessionStorage){storage.setItem(guestKey,JSON.stringify(cart));}
export function clearGuestCart(storage=sessionStorage,legacy=localStorage){storage.removeItem(guestKey);legacy.removeItem('sole-cart');}
