import mysql from 'mysql2/promise';
// Chỉ chạy test trên schema mới có tiền tố qa; không dùng database thật.
export const config={host:process.env.DB_HOST||'127.0.0.1',port:Number(process.env.DB_PORT||3306),user:process.env.DB_USER||'root',password:process.env.DB_PASSWORD||'',database:process.env.DB_NAME||'',timezone:'Z',decimalNumbers:true};
if(!/^sole_studio_nhom19_csharp_qa/.test(config.database))throw Error('DB_NAME của test phải bắt đầu bằng sole_studio_nhom19_csharp_qa');
export const pool=mysql.createPool(config);
pool.on('connection',c=>c.query("SET time_zone='+00:00'"));
export async function query(sql,args=[]){const[rows]=await pool.execute(sql,args);return rows;}
