export const SESSION_AREAS = Object.freeze({
  customer: { cookie: 'sole_customer_session', role: 'CUSTOMER' },
  manager: { cookie: 'sole_manager_session', role: 'SHOP_MANAGER' },
  admin: { cookie: 'sole_admin_session', role: 'SYSTEM_ADMIN' },
});

export function requestArea(req) {
  const area = req.get('x-sole-area');
  if (Object.hasOwn(SESSION_AREAS, area)) return area;
  if (req.path.startsWith('/manage/')) return 'manager';
  if (req.path.startsWith('/admin/')) return 'admin';
  return 'customer';
}

export function loginArea(roles, requested) {
  return canUseArea(roles,requested) ? requested : null;
}

export function canUseArea(roles, area) {
  if (!Object.hasOwn(SESSION_AREAS,area)) return false;
  if (area==='customer' && roles.some(r=>r==='SHOP_MANAGER'||r==='SYSTEM_ADMIN')) return false;
  return roles.includes(SESSION_AREAS[area].role);
}

export function sessionCookieOptions(req) {
  // NODE_ENV controls the build/runtime, not whether the request uses TLS.
  // req.secure also supports HTTPS proxies when trust proxy is configured.
  return { httpOnly: true, sameSite: 'strict', secure: Boolean(req.secure), path: '/' };
}

export function clearSessionCookie(req, res, area = req.sessionArea) {
  res.clearCookie(SESSION_AREAS[area].cookie, sessionCookieOptions(req));
}

export function validSessionToken(token) {
  return typeof token === 'string' && /^[a-f0-9]{64}$/.test(token);
}
