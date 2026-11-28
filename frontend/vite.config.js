import { defineConfig } from 'vite';
import { fileURLToPath } from 'node:url';

export default defineConfig({
  server: {
    host: process.env.HOST || '127.0.0.1',
    port: Number(process.env.PORT || 5173),
    strictPort: true,
    proxy: {
      '/api': {
        target: 'http://127.0.0.1:' + (process.env.API_PORT || 5174),
        changeOrigin: false,
      },
    },
    fs: {
      allow: [fileURLToPath(new URL('.',import.meta.url))],
      // Ảnh bằng chứng và thư OTP chỉ được truy cập qua các luồng xác thực.
      deny: ['.env','.env.*','*.{crt,pem}','**/.git/**','**/.local/**'],
    },
  },
  build: { outDir: 'dist' },
});
