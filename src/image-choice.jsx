import React,{useEffect,useState} from 'react';

export function ImageChoice({label,original,value,onChange}) {
  const mode=value?.mode ?? 'keep';
  const [filePreview,setFilePreview]=useState('');
  const file=mode==='file'?value?.file:null;
  useEffect(()=>{
    if(!file){setFilePreview('');return;}
    const url=URL.createObjectURL(file);
    setFilePreview(url);
    return ()=>URL.revokeObjectURL(url);
  },[file]);
  const url=value?.url?.trim();
  const preview=mode==='keep'?original:mode==='file'?filePreview:
    mode==='url'&&/^(https?:\/\/|\/images\/)/i.test(url||'')?url:'';

  return <fieldset>
    <legend>{label} <small>(không bắt buộc)</small></legend>
    <label>Cách đính kèm<select value={mode} onChange={e=>onChange({mode:e.target.value})}>
      <option value="keep">{original?'Giữ ảnh hiện tại':'Không đính kèm ảnh'}</option>
      <option value="file">Chọn tệp từ thiết bị</option>
      <option value="url">Nhập URL ảnh</option>
      {original&&<option value="remove">Xóa ảnh hiện tại</option>}
    </select></label>
    {mode==='file'&&<label>Tệp ảnh<input type="file" accept="image/jpeg,image/png,image/webp" onChange={e=>onChange({mode,file:e.target.files[0]})}/><small>JPEG, PNG hoặc WebP, tối đa 5 MB.</small></label>}
    {mode==='url'&&<label>URL ảnh<input type="text" inputMode="url" maxLength={1000} placeholder="https://…" value={value.url||''} onChange={e=>onChange({mode,url:e.target.value})}/></label>}
    {preview&&<img src={preview} alt="Xem trước ảnh" width="160" style={{height:120,objectFit:'contain'}}/>}
  </fieldset>;
}

// Không gửi trường cũ khi sửa các thông tin khác của sản phẩm.
export function imagePayload({photo,image:previousImage,...fields}) {
  if(photo?.mode==='remove') return {...fields,removeImage:true};
  if(photo?.mode==='url'&&photo.url?.trim()) return {...fields,image:photo.url.trim()};
  if(photo?.mode!=='file'||!photo.file) return fields;
  if(photo.file.size>5*1024*1024) throw Error('Ảnh vượt quá 5 MB.');
  const form=new FormData();
  for(const [key,value] of Object.entries(fields)) {
    if(value!=null) form.set(key,String(value));
  }
  form.set('imageFile',photo.file);
  return form;
}
