import 'dotenv/config';

function integer(name, fallback, min, max) {
  const value = Number(process.env[name] || fallback);
  if (!Number.isInteger(value) || value < min || value > max) throw new Error(`${name} không hợp lệ.`);
  return value;
}
export const storeConfig = Object.freeze({
  environment: process.env.APP_ENV || 'development',
  holdMinutes: integer('PAYMENT_HOLD_MINUTES', 30, 1, 10080),
  returnDays: integer('RETURN_WINDOW_DAYS', 7, 1, 365),
  bank: {
    name: process.env.BANK_NAME || '', account: process.env.BANK_ACCOUNT || '',
    holder: process.env.BANK_HOLDER || '',
  },
});
export function publicStoreConfig() {
  return {...storeConfig, manualPayments: true, exchangeMode: 'RETURN_AND_REORDER'};
}
