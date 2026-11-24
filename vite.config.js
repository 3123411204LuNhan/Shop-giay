import { defineConfig } from 'vite';
import { fileURLToPath } from 'node:url';

export default defineConfig({
  server: {
    host: '127.0.0.1',
    fs: {
      allow: [fileURLToPath(new URL('.',import.meta.url))],
      // Ảnh bằng chứng và thư OTP chỉ được truy cập qua các luồng xác thực.
      deny: ['.env','.env.*','*.{crt,pem}','**/.git/**','**/.local/**'],
    },
  },
  build: { outDir: 'dist' },
});
