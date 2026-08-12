const { chromium } = require('playwright');

async function takeSectionScreenshots() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1280, height: 900 } });
  const page = await context.newPage();

  // Homepage sections
  const sections = [
    { name: 'homepage-hero', url: 'http://localhost:3000/', selector: 'section:first-of-type' },
    { name: 'homepage-features', url: 'http://localhost:3000/', selector: '#features' },
    { name: 'homepage-modules', url: 'http://localhost:3000/', selector: '#modules' },
    { name: 'homepage-pricing', url: 'http://localhost:3000/', selector: '#pricing' },
  ];

  for (const s of sections) {
    try {
      console.log(`Capturing ${s.name}...`);
      await page.goto(s.url, { waitUntil: 'networkidle', timeout: 30000 });
      await page.waitForTimeout(1000);

      if (s.selector) {
        const element = await page.$(s.selector);
        if (element) {
          await element.scrollIntoViewIfNeeded();
          await page.waitForTimeout(500);
          const box = await element.boundingBox();
          if (box) {
            await page.screenshot({
              path: `screenshots/${s.name}.png`,
              clip: { x: 0, y: 0, width: 1280, height: Math.min(box.height + 100, 900) }
            });
          }
        } else {
          console.log(`  Selector ${s.selector} not found`);
        }
      }
      console.log(`  Saved: screenshots/${s.name}.png`);
    } catch (err) {
      console.log(`  Error: ${err.message}`);
    }
  }

  await browser.close();
  console.log('Done!');
}

takeSectionScreenshots();
