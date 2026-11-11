import 'dotenv/config';
import express from 'express';
import helmet from 'helmet';
import cookieParser from 'cookie-parser';
import { rateLimit } from 'express-rate-limit';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { pool, query } from './db.js';
import { router } from './routes.js';
const app=express(); const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
app.disable('x-powered-by');
app.use(helmet({contentSecurityPolicy:false,crossOriginEmbedderPolicy:false}));
app.use(express.json({limit:'1mb'}));app.use(cookieParser());
app.get('/api/health',async(req,res)=>{try{await pool.query('SELECT 1');res.json({status:'ok',database:'connected'});}catch{res.status(503).json({status:'setup_required',message:'Chưa kết nối MySQL. Kiểm tra DB_HOST, DB_PORT và DB_PASSWORD trong .env, chạy npm run db:setup rồi npm run db:seed.'});}});
app.use('/api',(req,res,next)=>{
  if(!['GET','HEAD','OPTIONS'].includes(req.method)) {
    const origin=req.get('origin');const host=req.get('host');
    if((origin && new URL(origin).host!==host)||req.get('x-sole-request')!=='1')return res.status(403).json({message:'Yêu cầu không hợp lệ.'});
  } next();
});
app.use('/api/auth',rateLimit({windowMs:15*60*1000,limit:100,standardHeaders:true,legacyHeaders:false,message:{message:'Quá nhiều yêu cầu xác thực. Vui lòng thử lại sau.'}}));
app.use('/api',router);
app.use('/api',(req,res)=>res.status(404).json({message:'Không tìm thấy API.'}));
app.use((err,req,res,next)=>{if(req.method!=='GET')query("INSERT INTO NhatKyHeThong(NguoiThucHienID,HanhDong,DoiTuong,NoiDung,KetQua) VALUES(?,'YEU_CAU_THAT_BAI','API',?,'THAT_BAI')",[req.user?.id||null,req.method+' '+req.path+' · HTTP '+(err.status||500)]).catch(()=>{});if(err.status)return res.status(err.status).json({message:err.message});if(err.code==='ER_DUP_ENTRY')return res.status(409).json({message:'Dữ liệu đã tồn tại hoặc yêu cầu đã được xử lý. Vui lòng tải lại.'});console.error('[API]',err.code||err.name);res.status(500).json({message:err.code?.includes('CONN')||err.code==='EHOSTUNREACH'?'Không kết nối được MySQL. Kiểm tra cấu hình .env.':'Không thể hoàn thành thao tác. Vui lòng kiểm tra dữ liệu hoặc thử lại.'});});
if(process.argv.includes('--production')){app.use(express.static(path.join(root,'dist')));app.get('/{*path}',(req,res)=>res.sendFile(path.join(root,'dist/index.html')));}
else{const {createServer}=await import('vite');const vite=await createServer({root,server:{middlewareMode:true},appType:'spa'});app.use(vite.middlewares);}
const port=Number(process.env.PORT||5173);const server=app.listen(port,process.env.HOST||'127.0.0.1',()=>console.log(`SOLE STUDIO: http://localhost:${port}`));
async function stop(){server.close();await pool.end();process.exit(0);}process.on('SIGTERM',stop);process.on('SIGINT',stop);
