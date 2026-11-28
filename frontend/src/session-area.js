export function areaForPath(path) {
  if (path === '/admin' || path.startsWith('/admin/')) return 'admin';
  if (path === '/manager' || path.startsWith('/manager/')) return 'manager';
  return 'customer';
}

export const homeForArea = area => ({ customer: '/', manager: '/manager', admin: '/admin' })[area];
