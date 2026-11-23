import { UserService } from './UserService.js';

// routes.js gắn các API này sau middleware admin. run chuyển kết quả thành JSON và chuyển lỗi cho Express.
export class UserController {
  constructor(service = new UserService()) {
    this.service = service;
  }

  register(router, admin, run) {
    const path = '/admin/accounts';
    router.get(path, admin, run(() => this.service.list()));
    router.post(path, admin, run(req => this.service.create(req.user, req.body)));
    router.put(path + '/:id', admin, run(req => this.service.update(req.user, req.params.id, req.body)));
    router.delete(path + '/:id', admin, run(req => this.service.remove(req.user, req.params.id)));
  }
}
