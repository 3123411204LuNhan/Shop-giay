import { PromotionRepository } from './PromotionRepository.js';
import { createPromotion, updatePromotion, deleteDraft } from '../promotions.js';

const repository = new PromotionRepository();

// PromotionController chuyển yêu cầu tới các phương thức xử lý công bố, tạo, sửa và xóa khuyến mãi.
export class PromotionService {
  async published() {
    return await repository.published();
  }

  async list() {
    return await repository.list();
  }

  async create(r) {
    return createPromotion(r);
  }

  async update(r) {
    return updatePromotion(r);
  }

  async deleteDraft(r) {
    return deleteDraft(r);
  }
}
