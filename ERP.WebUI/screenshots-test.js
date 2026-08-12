const { chromium } = require('playwright');

async function takeScreenshots() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1280, height: 800 } });
  const page = await context.newPage();

  const pages = [
    { name: 'homepage', url: 'http://localhost:3000/' },
    { name: 'login', url: 'http://localhost:3000/login' },
    { name: 'dashboard', url: 'http://localhost:3000/dashboard' },
    { name: 'inventory', url: 'http://localhost:3000/inventory' }
  ];

  for (const p of pages) {
    try {
      console.log(`Loading ${p.name}...`);
      await page.goto(p.url, { waitUntil: 'networkidle', timeout: 30000 });
      await page.waitForTimeout(2000);

      // Check for errors in console
      const errors = [];
      page.on('console', msg => {
        if (msg.type() === 'error') {
          errors.push(msg.text());
        }
      });

      // Take screenshot
      await page.screenshot({
        path: `screenshots/${p.name}.png`,
        fullPage: true
      });

      console.log(`  Screenshot saved: screenshots/${p.name}.png`);

      if (errors.length > 0) {
        console.log(`  Console errors: ${errors.length}`);
        errors.forEach(e => console.log(`    - ${e}`));
      } else {
        console.log(`  No console errors`);
      }
    } catch (err) {
      console.log(`  Error loading ${p.name}: ${err.message}`);
    }
  }

  await browser.close();
  console.log('Done!');
}

takeScreenshots();
