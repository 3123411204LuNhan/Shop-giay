import { defineConfig } from 'vite';
// Vite phục vụ giao diện khi sửa React; mọi /api được chuyển tới backend C#.
export default defineConfig({server:{host:'127.0.0.1',port:5174,proxy:{'/api':'http://localhost:5173'}},build:{outDir:'../src/ShopGiay.Web/wwwroot',emptyOutDir:true}});
