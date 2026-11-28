import React, {useState} from 'react';
import {ImageOff} from 'lucide-react';

// Dùng chung ở quản lý, cửa hàng và xem trước: giữ nguyên tỷ lệ, không cắt ảnh sản phẩm.
export function ProductImage({src, alt = 'Ảnh sản phẩm', className = '', loading = 'lazy'}) {
  const source = typeof src === 'string' ? src.trim() : '';
  // Ghi nhận chính URL bị lỗi; khi quản lý đổi URL, ảnh mới được tải lại bình thường.
  const [failedSource, setFailedSource] = useState(null);
  const unavailable = !source || failedSource === source;
  return <span className={'product-media ' + className} data-image-state={unavailable ? 'unavailable' : 'available'}>
    {unavailable ? <span className="product-media-placeholder" role="img" aria-label={'Chưa có ảnh: ' + alt}>
      <ImageOff size={24} aria-hidden="true"/><span>{source ? 'Ảnh không tải được' : 'Chưa có ảnh'}</span>
    </span> : <img key={source} src={source} alt={alt} loading={loading} decoding="async"
      onError={() => setFailedSource(source)}
      style={{width: '100%', height: '100%', objectFit: 'contain', objectPosition: 'center', transform: 'none'}}/>}
  </span>;
}
