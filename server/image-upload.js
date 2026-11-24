import express from 'express';
import { randomUUID } from 'node:crypto';
import { mkdir, unlink, writeFile } from 'node:fs/promises';
import { isAbsolute, join, relative, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
import { fail } from './db.js';

const maxBytes = 5 * 1024 * 1024;
const projectRoot = fileURLToPath(new URL('../', import.meta.url));
const root = resolve(process.env.IMAGE_UPLOAD_DIR || join(projectRoot,'.local','uploads'));
const relativeRoot = relative(projectRoot,root);
const insideProject = !isAbsolute(relativeRoot) && relativeRoot.split(sep)[0] !== '..';
if (insideProject && !relativeRoot.startsWith('.local'+sep))
  throw Error('IMAGE_UPLOAD_DIR phải nằm trong .local hoặc ngoài thư mục dự án.');
const filename = /^[\da-f-]{36}\.(png|jpg|webp)$/;
const mediaTypes = { png:'image/png', jpg:'image/jpeg', webp:'image/webp' };

function imageExtension(bytes) {
  if (bytes.length >= 33 && bytes.subarray(0,8).equals(Buffer.from('89504e470d0a1a0a','hex')) &&
      bytes.readUInt32BE(8) === 13 && bytes.toString('ascii',12,16) === 'IHDR' &&
      bytes.toString('ascii',bytes.length-8,bytes.length-4) === 'IEND') return 'png';
  if (bytes.length >= 12 && bytes[0] === 255 && bytes[1] === 216 && bytes[2] === 255 &&
      bytes.at(-2) === 255 && bytes.at(-1) === 217) return 'jpg';
  if (bytes.length >= 20 && bytes.toString('ascii',0,4) === 'RIFF' && bytes.readUInt32LE(4) === bytes.length-8 &&
      bytes.toString('ascii',8,12) === 'WEBP' && ['VP8 ','VP8L','VP8X'].includes(bytes.toString('ascii',12,16))) return 'webp';
  fail('Tệp phải là ảnh JPEG, PNG hoặc WebP.', 415);
}

const rawImageForm = express.raw({ type:'multipart/form-data', limit:maxBytes+64*1024, inflate:false });

// Dùng sau xác thực. JSON vẫn đi qua cùng API khi khách chọn liên kết hoặc để trống.
export const imageForm = [rawImageForm, async (req, res, next) => {
  if (!req.is('multipart/form-data')) return next();
  let form;
  try {
    form = await new Response(req.body,{headers:{'Content-Type':req.get('content-type')}}).formData();
  } catch {
    return next(Object.assign(new Error('Dữ liệu tải ảnh không hợp lệ.'),{status:400}));
  }
  try {
    const fields = Object.create(null);
    for (const [key,value] of form) {
      if (typeof value === 'string' && key !== 'imageFile' && !Object.hasOwn(fields,key)) fields[key]=value;
      else if (key === 'imageFile' && typeof value !== 'string' && !req.imageFile) {
        if (value.size > maxBytes) fail('Ảnh vượt quá 5 MB.',413);
        const bytes = Buffer.from(await value.arrayBuffer());
        req.imageFile = {bytes,extension:imageExtension(bytes)};
      } else fail('Mỗi yêu cầu chỉ được có một ảnh và một giá trị cho mỗi trường.');
    }
    if (req.imageFile && (String(fields.image ?? '').trim() || fields.removeImage === 'true'))
      fail('Hãy chọn một tệp ảnh hoặc một URL.');
    req.body = fields;
    next();
  } catch (error) { next(error); }
}];

async function removeStored(scope,name) {
  try { await unlink(join(root,scope,name)); }
  catch (error) { if (error.code !== 'ENOENT') console.error('[Ảnh tải lên]',error.code); }
}

// Chỉ tạo tệp khi service chuẩn bị lưu tham chiếu. Retry bỏ qua bước này khi đã có yêu cầu.
export async function withImageUpload(file,scope,work) {
  let name;
  try {
    return await work(async () => {
      if (!file) return undefined;
      await mkdir(join(root,scope),{recursive:true});
      name = `${randomUUID()}.${file.extension}`;
      await writeFile(join(root,scope,name),file.bytes,{flag:'wx'});
      return name;
    });
  } catch (error) {
    if (name && error.code !== 'EEXIST') await removeStored(scope,name);
    throw error;
  }
}

export async function removeProductUpload(url) {
  const prefix='/api/product-images/';
  const name=url?.startsWith(prefix)?url.slice(prefix.length):'';
  if (filename.test(name)) await removeStored('products',name);
}

export function sendUploadedImage(scope,name,res,next) {
  if (!filename.test(name)) fail('Không tìm thấy ảnh.',404);
  res.type(mediaTypes[name.split('.').at(-1)]);
  res.sendFile(name,{root:join(root,scope),cacheControl:false},error=>{
    if (error && !res.headersSent && (error.status===404 || error.code==='ENOENT')) {
      res.removeHeader('Content-Type');
      next(Object.assign(new Error('Không tìm thấy ảnh.'),{status:404}));
    } else if (error) next(error);
  });
}
