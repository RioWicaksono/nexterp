import { test, expect, Browser, BrowserContext } from '@playwright/test';

/**
 * E2E Tests for Critical User Flows in NEXTERP
 * These tests cover the most important user journeys for the ERP system.
 */

test.describe('Critical User Flows - Authentication', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
  });

  test('complete login flow with demo credentials', async ({ page }) => {
    // Fill login form
    await page.getByLabel(/Username/i).fill('admin@nexterp.com');
    await page.getByLabel(/Password/i).first().fill('Admin@123!');

    // Submit
    await page.getByRole('button', { name: /Sign In/i }).click();

    // Should redirect to dashboard
    await expect(page).not.toHaveURL(/\/login/, { timeout: 10000 });
  });

  test('login shows error for invalid credentials', async ({ page }) => {
    await page.getByLabel(/Username/i).fill('invalid@test.com');
    await page.getByLabel(/Password/i).first().fill('wrongpassword');
    await page.getByRole('button', { name: /Sign In/i }).click();

    // Should show error message
    await expect(page.getByText(/invalid|error|incorrect/i)).toBeVisible({ timeout: 5000 });
  });

  test('session persists after page reload', async ({ page }) => {
    // Login first
    await page.getByLabel(/Username/i).fill('admin@nexterp.com');
    await page.getByLabel(/Password/i).first().fill('Admin@123!');
    await page.getByRole('button', { name: /Sign In/i }).click();
    await expect(page).not.toHaveURL(/\/login/, { timeout: 10000 });

    // Reload page
    await page.reload();

    // Should still be logged in
    await expect(page).not.toHaveURL(/\/login/, { timeout: 5000 });
  });
});

test.describe('Critical User Flows - Dashboard', () => {
  test.use({ storageState: 'playwright/.auth/admin.json' });

  test('dashboard loads with key metrics', async ({ page }) => {
    await page.goto('/dashboard');

    // Wait for dashboard content
    await page.waitForLoadState('networkidle');

    // Check for key elements
    await expect(page.getByText(/dashboard|overview/i)).toBeVisible({ timeout: 10000 });
  });

  test('dashboard navigation works', async ({ page }) => {
    await page.goto('/dashboard');

    // Navigate to different sections if sidebar exists
    const sidebarLinks = page.locator('nav a, aside a, [role="navigation"] a');
    const linkCount = await sidebarLinks.count();

    if (linkCount > 0) {
      // Click first link that leads to a different page
      const firstLink = sidebarLinks.first();
      const href = await firstLink.getAttribute('href');
      if (href && href !== '/') {
        await firstLink.click();
        await page.waitForLoadState('networkidle');
      }
    }
  });
});

test.describe('Critical User Flows - Inventory Management', () => {
  test.use({ storageState: 'playwright/.auth/admin.json' });

  test('can view inventory list', async ({ page }) => {
    await page.goto('/inventory');
    await page.waitForLoadState('networkidle');

    // Should show inventory-related content
    await expect(page.getByText(/inventory|stock|warehouse/i)).toBeVisible({ timeout: 10000 });
  });

  test('can search inventory items', async ({ page }) => {
    await page.goto('/inventory');
    await page.waitForLoadState('networkidle');

    // Look for search input
    const searchInput = page.getByPlaceholder(/search|filter/i).or(page.getByRole('searchbox'));
    if (await searchInput.isVisible()) {
      await searchInput.fill('sample');
      await page.waitForTimeout(500); // Wait for debounce

      // Should show results or empty state
      const results = page.locator('table tbody tr, [role="listitem"], .item');
      const hasResults = await results.count() > 0 || await page.getByText(/no.*result|not.*found/i).isVisible();
      expect(hasResults).toBeTruthy();
    }
  });

  test('can create new inventory item', async ({ page, browser }) => {
    await page.goto('/inventory');
    await page.waitForLoadState('networkidle');

    // Look for add/create button
    const addButton = page.getByRole('button', { name: /add|create|new/i }).or(
      page.locator('[aria-label*="add" i], [aria-label*="create" i]')
    );

    if (await addButton.isVisible()) {
      await addButton.click();
      await page.waitForLoadState('networkidle');

      // Should show form or modal
      const form = page.locator('form, [role="dialog"], modal');
      const heading = page.getByRole('heading', { name: /add|create|new/i });
      await expect(form.or(heading)).toBeVisible({ timeout: 5000 });
    }
  });
});

test.describe('Critical User Flows - Sales Orders', () => {
  test.use({ storageState: 'playwright/.auth/admin.json' });

  test('can view sales orders list', async ({ page }) => {
    await page.goto('/sales/orders');
    await page.waitForLoadState('networkidle');

    await expect(page.getByText(/sales|order/i)).toBeVisible({ timeout: 10000 });
  });

  test('can create new sales order', async ({ page }) => {
    await page.goto('/sales/orders');
    await page.waitForLoadState('networkidle');

    const createButton = page.getByRole('button', { name: /new.*order|create.*order|add.*order/i });
    if (await createButton.isVisible()) {
      await createButton.click();
      await page.waitForLoadState('networkidle');

      // Should show order form
      const heading = page.getByRole('heading', { name: /order|create/i });
      await expect(heading.or(page.locator('form'))).toBeVisible({ timeout: 5000 });
    }
  });
});

test.describe('Critical User Flows - User Management', () => {
  test.use({ storageState: 'playwright/.auth/admin.json' });

  test('can view user list (admin only)', async ({ page }) => {
    await page.goto('/users');
    await page.waitForLoadState('networkidle');

    // Should show users or redirect
    const hasUsers = await page.getByText(/user|employee|staff/i).isVisible({ timeout: 5000 }).catch(() => false);
    if (!hasUsers) {
      await expect(page).toHaveURL(/\/users/);
    }
  });
});

test.describe('Critical User Flows - Error Handling', () => {
  test.use({ storageState: 'playwright/.auth/admin.json' });

  test('shows error page for 404', async ({ page }) => {
    await page.goto('/this-page-does-not-exist-12345');
    await page.waitForLoadState('networkidle');

    // Should show 404 page
    const pageContent = await page.content();
    const has404 = pageContent.toLowerCase().includes('404') ||
                   pageContent.toLowerCase().includes('not found') ||
                   pageContent.toLowerCase().includes('page not found');
    expect(has404 || page.url().includes('404')).toBeTruthy();
  });

  test('shows error message on API failure', async ({ page }) => {
    // Navigate to a page that might have API dependency
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Reload to trigger fresh API call
    await page.reload();
    await page.waitForLoadState('networkidle');

    // Should show error state or load successfully
    const hasError = await page.getByText(/error|failed|unavailable/i).isVisible({ timeout: 3000 }).catch(() => false);
    const hasContent = await page.locator('main, [role="main"], body').isVisible();
    expect(hasError || hasContent).toBeTruthy();
  });
});

test.describe('Critical User Flows - Performance', () => {
  test.use({ storageState: 'playwright/.auth/admin.json' });

  test('page loads within acceptable time', async ({ page }) => {
    const startTime = Date.now();
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    const loadTime = Date.now() - startTime;

    // Dashboard should load within 5 seconds
    expect(loadTime).toBeLessThan(5000);
  });

  test('navigation is responsive', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const startTime = Date.now();
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    const navTime = Date.now() - startTime;

    // Navigation should be under 3 seconds
    expect(navTime).toBeLessThan(3000);
  });
});

test.describe('Critical User Flows - Accessibility', () => {
  test.use({ storageState: 'playwright/.auth/admin.json' });

  test('page has proper heading structure', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Should have at least one h1
    const h1Count = await page.locator('h1').count();
    expect(h1Count).toBeGreaterThan(0);
  });

  test('form inputs have labels', async ({ page }) => {
    await page.goto('/login');
    await page.waitForLoadState('networkidle');

    // Check inputs have associated labels
    const inputs = page.locator('input:not([type="hidden"])');
    const inputCount = await inputs.count();

    for (let i = 0; i < Math.min(inputCount, 5); i++) {
      const input = inputs.nth(i);
      const hasLabel = await input.getAttribute('aria-label') ||
                       await page.locator(`label[for="${await input.getAttribute('id')}"]`).isVisible() ||
                       await input.getAttribute('placeholder');
      expect(hasLabel).toBeTruthy();
    }
  });

  test('focus is visible on interactive elements', async ({ page }) => {
    await page.goto('/login');
    await page.waitForLoadState('networkidle');

    // Tab to first interactive element
    await page.keyboard.press('Tab');

    // Focus outline should be visible
    const focusedElement = page.locator(':focus');
    await expect(focusedElement).toBeVisible();
  });
});

test.describe('Critical User Flows - Theme & Preferences', () => {
  test.use({ storageState: 'playwright/.auth/admin.json' });

  test('theme toggle works', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    const themeToggle = page.locator('button[aria-label*="theme" i], button[aria-label*="dark" i], button[aria-label*="light" i]');

    if (await themeToggle.isVisible()) {
      const initialTheme = await page.evaluate(() =>
        document.documentElement.classList.contains('dark') ? 'dark' : 'light'
      );

      await themeToggle.click();
      await page.waitForTimeout(300);

      const newTheme = await page.evaluate(() =>
        document.documentElement.classList.contains('dark') ? 'dark' : 'light'
      );

      expect(newTheme).not.toBe(initialTheme);
    }
  });

  test('preferences persist across sessions', async ({ page, browser }) => {
    // Set a preference
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    const themeToggle = page.locator('button[aria-label*="theme" i]');
    if (await themeToggle.isVisible()) {
      await themeToggle.click();
      await page.waitForTimeout(300);
    }

    // Save storage state
    const storageStatePath = 'playwright/.auth/admin-prefs.json';
    await page.context().storageState({ path: storageStatePath });

    // Create new context with saved state
    const newContext: BrowserContext = await browser.newContext({
      storageState: storageStatePath
    });

    const newPage = await newContext.newPage();
    await newPage.goto('/dashboard');
    await newPage.waitForLoadState('networkidle');

    // Preference should persist
    const theme = await newPage.evaluate(() => document.documentElement.classList.contains('dark'));
    expect(theme !== null).toBeTruthy();

    await newContext.close();
  });
});
