import 'dotenv/config';
import {spawn, spawnSync} from 'node:child_process';
import {resolve} from 'node:path';

const production = process.argv.includes('--production');
const integration = process.argv.includes('--test-integration');
const command = process.argv.find(arg => ['--verify-mail', '--db-setup', '--db-seed', '--db-migrate'].includes(arg));
const project = 'backend/ShopGiay.Api';
const configuration = production ? 'Release' : 'Debug';
const build = spawnSync('dotnet', ['build', project, '-c', configuration], {stdio:'inherit'});
if (build.error) console.error(build.error.message);
if (build.status !== 0) process.exit(build.status ?? 1);

const dll = project + '/bin/' + configuration + '/net10.0/ShopGiay.Api.dll';
const startApi = (args, env, signal) => spawn('dotnet', [dll, ...args], {stdio:'inherit', env, signal});
const startVite = (env, signal) => spawn(process.execPath, ['node_modules/vite/bin/vite.js', 'frontend'], {
  stdio:'inherit', env, signal,
});

if (integration) {
  const {runIntegration} = await import('./test-integration.js');
  process.exitCode = await runIntegration(startApi, startVite);
} else {
  launch();
}

function launch() {
  const port = Number(process.env.PORT || 5173);
  const apiPort = Number(process.env.API_PORT || (port === 5174 ? 5175 : 5174));
  if (!production && !command && port === apiPort) throw Error('PORT và API_PORT phải khác nhau.');
  const host = production ? process.env.HOST || '127.0.0.1' : '127.0.0.1';
  const args = command ? [command] : ['--urls', `http://${host}:${production ? port : apiPort}`];
  const api = startApi(args, {...process.env, ...(production ? {FRONTEND_DIST:resolve('frontend/dist')} : {})});
  const children = [api];
  if (!production && !command) {
    children.push(startVite({...process.env, PORT:String(port), API_PORT:String(apiPort)}));
  }
  let stopping = false;
  function stop(code) {
    if (stopping) return;
    stopping = true;
    for (const child of children) child.kill();
    process.exitCode = code;
  }
  for (const child of children) {
    child.on('error', error => {console.error(error.message); stop(1);});
    child.on('exit', code => stop(code ?? 1));
  }
  process.on('SIGINT', () => stop(0));
  process.on('SIGTERM', () => stop(0));
}
