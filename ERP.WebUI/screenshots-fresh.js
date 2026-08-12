const { chromium } = require('playwright');

async function takeScreenshots() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1280, height: 900 } });
  const page = await context.newPage();

  const pages = [
    { name: 'login-fresh', url: 'http://localhost:3001/login' },
    { name: 'homepage-fresh', url: 'http://localhost:3001/' },
    { name: 'dashboard-fresh', url: 'http://localhost:3001/dashboard' }
  ];

  for (const p of pages) {
    try {
      console.log(`Loading ${p.name}...`);
      await page.goto(p.url, { waitUntil: 'networkidle', timeout: 30000 });
      await page.waitForTimeout(2000);
      await page.screenshot({ path: `screenshots/${p.name}.png`, fullPage: false });
      console.log(`  Saved: screenshots/${p.name}.png`);
    } catch (err) {
      console.log(`  Error: ${err.message}`);
    }
  }

  await browser.close();
  console.log('Done!');
}

takeScreenshots();
