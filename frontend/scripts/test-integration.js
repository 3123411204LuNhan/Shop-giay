import {spawn} from 'node:child_process';
import {randomUUID} from 'node:crypto';
import {mkdtemp, rm} from 'node:fs/promises';
import {createServer} from 'node:net';
import {tmpdir} from 'node:os';
import {join} from 'node:path';
import {setTimeout as delay} from 'node:timers/promises';
import mysql from 'mysql2/promise';
import {config} from '../tests/db.js';

const host = '127.0.0.1';
const url = port => `http://${host}:${port}`;

async function freePort() {
  const server = createServer();
  await new Promise((resolve, reject) => server.once('error', reject).listen(0, host, resolve));
  const {port} = server.address();
  await new Promise(resolve => server.close(resolve));
  return port;
}

async function waitForHealth(port, signal) {
  const endpoint = url(port) + '/api/health';
  const deadline = Date.now() + 45000;
  let last = 'chưa kết nối được';
  while (Date.now() < deadline) {
    signal.throwIfAborted();
    try {
      const response = await fetch(endpoint, {signal:AbortSignal.any([signal, AbortSignal.timeout(2000)])});
      last = 'HTTP ' + response.status;
      if (response.ok) {
        const health = await response.json();
        if (health.status === 'ok' && health.database === 'connected') return;
        last = 'API chưa xác nhận kết nối MySQL';
      } else {
        await response.body?.cancel();
      }
    } catch (error) {
      signal.throwIfAborted();
      last = error.cause?.code ?? error.message;
    }
    await delay(250, undefined, {signal});
  }
  throw new Error(`Hết thời gian chờ ${endpoint} (${last}).`);
}

export async function runIntegration(startApi, startVite) {
  const database = 'sole_studio_nhom19_it_' + randomUUID().replaceAll('-', '');
  const controller = new AbortController();
  const {signal} = controller;
  const children = [];
  let interrupted;
  let admin, workspace;
  let ownsDatabase = false;
  let exitCode = 1;
  let phase = 'Chuẩn bị môi trường kiểm thử';

  function interrupt(code) {
    interrupted = code;
    controller.abort(new Error('Kiểm thử đã bị dừng.'));
  }
  const sigint = () => interrupt(130);
  const sigterm = () => interrupt(143);
  process.on('SIGINT', sigint);
  process.on('SIGTERM', sigterm);

  function watch(child, name, server = false) {
    const finished = new Promise(resolve => {
      let error;
      child.on('error', cause => { error = cause; });
      child.once('close', (code, reason) => {
        const failure = error ?? new Error(`${name} đã dừng (${reason ?? `mã thoát ${code}`}).`);
        if (server && !signal.aborted) controller.abort(failure);
        resolve({code:code ?? 1, error, failure});
      });
    });
    children.push(finished);
    return finished;
  }

  try {
    const port = await freePort();
    let apiPort = await freePort();
    while (apiPort === port) apiPort = await freePort();
    workspace = await mkdtemp(join(tmpdir(), 'shopgiay-integration-'));
    const env = {
      ...process.env, DB_NAME:database, HOST:host, PORT:String(port), API_PORT:String(apiPort),
      APP_ORIGIN:url(port), MAIL_FALLBACK_LOCAL:'true', SMTP_HOST:'',
      MAIL_LOCAL_DIR:join(workspace, 'mail'), IMAGE_UPLOAD_DIR:join(workspace, 'uploads'),
      Logging__LogLevel__Default:'Warning',
    };

    phase = 'Tạo database kiểm thử';
    const {host:dbHost, port:dbPort, user, password, connectTimeout} = config;
    admin = await mysql.createConnection({host:dbHost, port:dbPort, user, password, connectTimeout});
    signal.throwIfAborted();
    await admin.query('CREATE DATABASE ?? CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci', [database]);
    ownsDatabase = true;
    for (const command of ['--db-setup', '--db-seed']) {
      phase = command.slice(2);
      signal.throwIfAborted();
      const result = await watch(startApi([command], env, signal), command);
      if (result.code !== 0) throw result.failure;
    }

    phase = 'Khởi động ASP.NET Core';
    signal.throwIfAborted();
    watch(startApi(['--urls', url(apiPort)], env, signal), 'ASP.NET Core', true);
    await waitForHealth(apiPort, signal);
    phase = 'Khởi động Vite';
    watch(startVite(env, signal), 'Vite', true);
    await waitForHealth(port, signal);
    signal.throwIfAborted();

    phase = 'Chạy bộ kiểm thử';
    console.log(`API đã sẵn sàng qua ${url(port)}. Cổng ASP.NET Core là ${apiPort}.`);
    const result = await watch(spawn(process.execPath, ['--test', '--test-concurrency=1', 'tests/*.test.js'], {
      stdio:'inherit', env, signal,
    }), 'Bộ kiểm thử');
    signal.throwIfAborted();
    if (result.error) throw result.error;
    exitCode = result.code;
  } catch (error) {
    console.error(`${phase} thất bại. ${signal.reason?.message ?? error.message}`);
  } finally {
    controller.abort();
    await Promise.all(children);
    try {
      if (ownsDatabase) await admin.query('DROP DATABASE ??', [database]);
    } catch (error) {
      console.error(`Không dọn được database kiểm thử ${database}. ${error.message}`);
      exitCode = 1;
    } finally {
      admin?.destroy();
    }
    try {
      if (workspace) await rm(workspace, {recursive:true, force:true, maxRetries:2});
    } catch (error) {
      console.error(`Không dọn được thư mục kiểm thử ${workspace}. ${error.message}`);
      exitCode = 1;
    }
    process.off('SIGINT', sigint);
    process.off('SIGTERM', sigterm);
  }
  return interrupted ?? exitCode;
}
