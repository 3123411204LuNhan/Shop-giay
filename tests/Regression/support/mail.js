import {fileURLToPath} from 'node:url';
export const localMailbox=process.env.QA_MAILBOX||fileURLToPath(new URL('../../../src/ShopGiay.Web/.local/mail/',import.meta.url));
