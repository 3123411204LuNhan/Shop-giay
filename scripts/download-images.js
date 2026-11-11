import {mkdir,writeFile} from 'node:fs/promises';
const images={red:'1542291026-7eec264c27ff',white:'1600269452121-4f2416e55c28',cream:'1549298916-b41d501d3772',green:'1600185365926-3a2ce3cdb9eb',gray:'1539185441755-769473a23570',black:'1543508282-6319a3e2621f'};
await mkdir('public/images',{recursive:true});
for(const[name,id]of Object.entries(images)){
 const response=await fetch(`https://images.unsplash.com/photo-${id}?auto=format&fit=crop&w=1000&q=85`,{signal:AbortSignal.timeout(30000)});
 if(!response.ok)throw new Error(`${name}: ${response.status}`);await writeFile(`public/images/shoe-${name}.jpg`,Buffer.from(await response.arrayBuffer()));console.log('Saved',name);
}
