import { test, expect } from '@playwright/test';

/**
 * Login E2E Tests
 */
test.describe('Authentication', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
  });

  test('should display login form', async ({ page }) => {
    await expect(page.locator('h1, h2, [class*="title"], [class*="heading"]')).toContainText(/login|sign in/i);
    await expect(page.locator('input[name="username"], input[type="text"]')).toBeVisible();
    await expect(page.locator('input[name="password"], input[type="password"]')).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeVisible();
  });

  test('should login with valid credentials', async ({ page }) => {
    await page.fill('input[name="username"], input[type="text"]', 'admin');
    await page.fill('input[name="password"], input[type="password"]', 'DevPassword2024!');
    await page.click('button[type="submit"]');

    // Wait for redirect to dashboard
    await page.waitForURL(/dashboard/, { timeout: 10000 });

    // Verify dashboard is loaded
    await expect(page.locator('body')).toContainText(/dashboard|home/i);
  });

  test('should show error with invalid credentials', async ({ page }) => {
    await page.fill('input[name="username"], input[type="text"]', 'admin');
    await page.fill('input[name="password"], input[type="password"]', 'wrongpassword');
    await page.click('button[type="submit"]');

    // Wait for error message
    await expect(page.locator('[class*="error"], [class*="alert"], text=/invalid|failed|error/i')).toBeVisible({ timeout: 5000 });
  });

  test('should validate required fields', async ({ page }) => {
    // Click submit without filling
    await page.click('button[type="submit"]');

    // Should show validation messages
    await expect(page.locator('[class*="error"], [class*="required"], text=/required|empty/i')).toBeVisible();
  });
});
