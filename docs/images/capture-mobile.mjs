import { chromium, devices } from '/tmp/node_modules/playwright/index.mjs';
import { mkdirSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const outDir = __dirname;
mkdirSync(outDir, { recursive: true });

const BASE = process.env.PAYFLOW_UI || 'http://localhost:4200';
const EMAIL = 'ava.chen@payflow.demo';
const PASSWORD = 'Password1!';

async function shot(page, name) {
  // Dismiss any toast overlays for a clean capture
  const dismiss = page.locator('button[aria-label*="Dismiss"], button[aria-label*="dismiss"], .toast button').first();
  if (await dismiss.count()) {
    await dismiss.click({ timeout: 1000 }).catch(() => undefined);
  }
  await page.waitForTimeout(4500); // toast auto-dismiss is 4200ms
  const path = join(outDir, name);
  await page.screenshot({ path, fullPage: false, animations: 'disabled' });
  console.log('wrote', path);
}

const browser = await chromium.launch({ headless: true });
const context = await browser.newContext({
  ...devices['iPhone 13'],
  deviceScaleFactor: 2,
});
const page = await context.newPage();

await page.goto(`${BASE}/login`, { waitUntil: 'domcontentloaded', timeout: 60000 });
await page.locator('input[type="email"]').fill(EMAIL);
await page.locator('input[type="password"]').fill(PASSWORD);
await page.getByRole('button', { name: 'Sign in' }).click();
await page.waitForURL((url) => !url.pathname.includes('login'), { timeout: 20000 });
await page.getByText('$2,271.00').waitFor({ timeout: 15000 });
await shot(page, 'home-balance.png');

await page.goto(`${BASE}/transactions`, { waitUntil: 'domcontentloaded' });
await page.getByText(/From Sofia Nguyen|To Marcus Lee|Activity/i).first().waitFor({ timeout: 15000 });
await page.waitForTimeout(800);
await page.screenshot({
  path: join(outDir, 'activity.png'),
  fullPage: false,
  animations: 'disabled',
});
console.log('wrote activity.png');

await page.goto(`${BASE}/beneficiaries`, { waitUntil: 'domcontentloaded' });
await page.getByText('Marcus Lee').first().waitFor({ timeout: 15000 });
await page.waitForTimeout(600);
await page.screenshot({
  path: join(outDir, 'people.png'),
  fullPage: false,
  animations: 'disabled',
});
console.log('wrote people.png');

await page.goto(`${BASE}/notifications`, { waitUntil: 'domcontentloaded' });
await page.getByText(/Transfer received|Alerts/i).first().waitFor({ timeout: 15000 });
await page.waitForTimeout(600);
await page.screenshot({
  path: join(outDir, 'notifications.png'),
  fullPage: false,
  animations: 'disabled',
});
console.log('wrote notifications.png');

await page.goto(`${BASE}/transfer`, { waitUntil: 'domcontentloaded' });
await page.getByText('Marcus Lee').first().waitFor({ timeout: 15000 });
await page.locator('li', { hasText: 'Marcus Lee' }).locator('button').first().click();
await page.getByRole('button', { name: /Continue with Marcus/i }).click();
await page.getByRole('button', { name: 'Review transfer' }).waitFor({ timeout: 15000 });

for (const label of ['4', '2', 'Decimal', '5', '0']) {
  await page.getByRole('button', { name: label, exact: true }).click();
}
await page.locator('input[placeholder*="What"]').fill('Weekend coffee');
await page.getByRole('button', { name: 'Review transfer' }).click();
await page.getByText("You're sending").waitFor({ timeout: 15000 });
await page.waitForTimeout(500);
await page.screenshot({
  path: join(outDir, 'transfer-confirm.png'),
  fullPage: false,
  animations: 'disabled',
});
console.log('wrote transfer-confirm.png');

await browser.close();
console.log('DONE');
