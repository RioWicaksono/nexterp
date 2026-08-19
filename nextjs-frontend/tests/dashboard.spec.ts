import { test, expect } from '@playwright/test';

/**
 * Dashboard E2E Tests
 */
test.describe('Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    // Login first
    await page.goto('/login');
    await page.fill('input[name="username"], input[type="text"]', 'admin');
    await page.fill('input[name="password"], input[type="password"]', 'DevPassword2024!');
    await page.click('button[type="submit"]');
    await page.waitForURL(/dashboard/, { timeout: 10000 });
  });

  test('should load dashboard page', async ({ page }) => {
    await expect(page).toHaveURL(/dashboard/);
    await expect(page.locator('body')).not.toContainText(/loading|error/i);
  });

  test('should display navigation menu', async ({ page }) => {
    // Check for main navigation elements
    const nav = page.locator('nav, [class*="sidebar"], [class*="menu"]');
    await expect(nav.first()).toBeVisible();
  });

  test('should navigate to HRM page', async ({ page }) => {
    await page.click('a[href*="hrm"], [class*="hrm"]');
    await page.waitForURL(/hrm/, { timeout: 5000 });
  });

  test('should navigate to Inventory page', async ({ page }) => {
    await page.click('a[href*="inventory"], [class*="inventory"]');
    await page.waitForURL(/inventory/, { timeout: 5000 });
  });

  test('should navigate to Purchasing page', async ({ page }) => {
    await page.click('a[href*="purchasing"], [class*="purchasing"]');
    await page.waitForURL(/purchasing/, { timeout: 5000 });
  });

  test('should navigate to Accounting page', async ({ page }) => {
    await page.click('a[href*="accounting"], [class*="accounting"]');
    await page.waitForURL(/accounting/, { timeout: 5000 });
  });

  test('should navigate to Projects page', async ({ page }) => {
    await page.click('a[href*="projects"], [class*="projects"]');
    await page.waitForURL(/projects/, { timeout: 5000 });
  });

  test('should navigate to Settings page', async ({ page }) => {
    await page.click('a[href*="settings"], [class*="settings"]');
    await page.waitForURL(/settings/, { timeout: 5000 });
  });

  test('should logout successfully', async ({ page }) => {
    await page.click('button:has-text("Logout"), a:has-text("Logout"), [class*="logout"]');
    await page.waitForURL(/login/, { timeout: 5000 });
  });
});

/**
 * Navigation E2E Tests
 */
test.describe('Navigation', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.fill('input[name="username"], input[type="text"]', 'admin');
    await page.fill('input[name="password"], input[type="password"]', 'DevPassword2024!');
    await page.click('button[type="submit"]');
    await page.waitForURL(/dashboard/, { timeout: 10000 });
  });

  test('should highlight active navigation item', async ({ page }) => {
    const activeItem = page.locator('a[aria-current="page"], a[class*="active"]');
    await expect(activeItem.first()).toBeVisible();
  });

  test('should collapse sidebar on mobile', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });

    // Toggle sidebar button should be visible
    const toggleButton = page.locator('button[aria-label*="toggle"], button[class*="menu"]');
    await expect(toggleButton.first()).toBeVisible();
  });
});
