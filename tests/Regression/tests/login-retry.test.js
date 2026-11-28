import test from 'node:test';
import assert from 'node:assert/strict';

const base = `http://127.0.0.1:${process.env.PORT || 5173}/api`;
// Gửi yêu cầu mới mỗi lần; cookie chỉ được dùng nếu server thực sự cấp sau đăng nhập thành công.
async function login(area, email, password) {
  const response = await fetch(base + '/auth/login', {
    method: 'POST',
    headers: {'Content-Type': 'application/json', 'X-Sole-Request': '1', 'X-Sole-Area': area},
    body: JSON.stringify({login: email, password}),
  });
  return {status: response.status, data: await response.json(), cookies: response.headers.getSetCookie()};
}

// Sai khu vực và sai mật khẩu không được làm hỏng lần thử đúng sau đó.
for (const [area, email, endpoint, cookie] of [
  ['manager', 'shop@sole.vn', '/manage/products', 'sole_manager_session='],
  ['admin', 'admin@sole.vn', '/admin/system', 'sole_admin_session='],
]) {
  test(`${area}: sai khu vực → sai mật khẩu → nhập đúng vẫn vào được`, async () => {
    const wrongArea = await login('customer', email, 'SoleDemo@123');
    assert.equal(wrongArea.status, 403);
    assert.equal(wrongArea.cookies.length, 0);
    const wrongPassword = await login(area, email, 'WrongPassword-QA');
    assert.equal(wrongPassword.status, 401);
    assert.equal(wrongPassword.cookies.length, 0);
    const correct = await login(area, email, 'SoleDemo@123');
    assert.equal(correct.status, 200);
    assert.equal(correct.data.area, area);
    const session = correct.cookies.find(value => value.startsWith(cookie));
    assert.ok(session);
    const protectedResponse = await fetch(base + endpoint, {headers: {'X-Sole-Area': area, Cookie: session.split(';')[0]}});
    assert.equal(protectedResponse.status, 200);
  });
}
