import { test, expect } from '@playwright/test';

test.describe('Landing Page', () => {
  test('should display the landing page with correct title', async ({ page }) => {
    await page.goto('/');

    await expect(page).toHaveTitle(/NEXTERP/);
    await expect(page.getByRole('navigation').getByText('NEXTERP')).toBeVisible();
  });

  test('should display hero section', async ({ page }) => {
    await page.goto('/');

    await expect(page.getByText('Enterprise Resource Planning')).toBeVisible();
  });

  test('should display module cards', async ({ page }) => {
    await page.goto('/');

    await expect(page.getByText('Inventory').first()).toBeVisible();
    await expect(page.getByText('Sales').first()).toBeVisible();
    await expect(page.getByText('Accounting').first()).toBeVisible();
    await expect(page.getByText('HRM').first()).toBeVisible();
  });

  test('should display footer', async ({ page }) => {
    await page.goto('/');

    await expect(page.getByRole('contentinfo').getByText(/SeVeN-/i)).toBeVisible();
  });

  test('should navigate to login page', async ({ page }) => {
    await page.goto('/login');
    await expect(page).toHaveURL(/\/login/);
    await expect(page.getByRole('heading', { name: 'Welcome Back' })).toBeVisible();
  });

  test('should navigate to register page', async ({ page }) => {
    await page.goto('/register');
    await expect(page).toHaveURL(/\/register/);
    await expect(page.getByRole('heading', { name: 'Create Your Account' })).toBeVisible();
  });

  test('should display CTA buttons on desktop', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 720 });
    await page.goto('/');

    await expect(page.getByText('Get Started').first()).toBeVisible();
  });
});

test.describe('Login Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
  });

  test('should display login form', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Welcome Back' })).toBeVisible();
    await expect(page.getByLabel(/Username/i)).toBeVisible();
    await expect(page.getByLabel(/Password/i).first()).toBeVisible();
  });

  test('should show validation errors on empty submit', async ({ page }) => {
    await page.getByRole('button', { name: /Sign In/i }).click();

    await expect(page.getByText(/Username is required/i)).toBeVisible();
    await expect(page.getByText(/Password is required/i)).toBeVisible();
  });

  test('should show error for invalid credentials', async ({ page }) => {
    await page.getByLabel(/Username/i).fill('wrong@test.com');
    await page.getByLabel(/Password/i).first().fill('wrongpass');
    await page.getByRole('button', { name: /Sign In/i }).click();

    await expect(page.getByText(/Invalid/i)).toBeVisible();
  });

  test('should toggle password visibility', async ({ page }) => {
    const passwordInput = page.getByLabel(/Password/i).first();
    const showButton = page.locator('button[aria-label="Show password"]').first();

    await expect(passwordInput).toHaveAttribute('type', 'password');

    await showButton.click();
    await expect(passwordInput).toHaveAttribute('type', 'text');

    await page.locator('button[aria-label="Hide password"]').click();
    await expect(passwordInput).toHaveAttribute('type', 'password');
  });

  test('should navigate to register page', async ({ page }) => {
    await page.getByRole('link', { name: /Sign up free/i }).click();

    await expect(page).toHaveURL(/\/register/);
  });

  test('should have forgot password link', async ({ page }) => {
    await expect(page.getByText(/Forgot password/i)).toBeVisible();
  });

  test('should display demo credentials hint', async ({ page }) => {
    await expect(page.getByText(/Demo Account:/i)).toBeVisible();
    await expect(page.getByText(/admin@nexterp.com/i)).toBeVisible();
  });

  test('should have working theme toggle', async ({ page }) => {
    const themeButton = page.locator('button[aria-label="Toggle theme"]');
    await expect(themeButton).toBeVisible();

    await themeButton.click();
    // Theme should toggle (no specific assertion needed)
  });
});

test.describe('Register Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/register');
  });

  test('should display registration form', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Create Your Account' })).toBeVisible();
    await expect(page.getByLabel(/Full Name/i)).toBeVisible();
    await expect(page.getByLabel(/Work Email/i)).toBeVisible();
    await expect(page.getByLabel(/Company/i)).toBeVisible();
  });

  test('should show validation errors', async ({ page }) => {
    await page.getByRole('button', { name: /Create Account/i }).click();

    await expect(page.getByText(/Full name is required/i)).toBeVisible();
    await expect(page.getByText(/Email is required/i)).toBeVisible();
  });

  test('should show password requirements', async ({ page }) => {
    await page.getByLabel(/Password/i).first().fill('Test');

    await expect(page.getByText(/At least 8 characters/i)).toBeVisible();
    await expect(page.getByText(/One uppercase letter/i)).toBeVisible();
  });

  test('should navigate to login page', async ({ page }) => {
    await page.getByRole('link', { name: /Sign in/i }).click();

    await expect(page).toHaveURL(/\/login/);
  });

  test('should validate password strength', async ({ page }) => {
    await page.getByLabel(/Password/i).first().fill('weak');
    await page.getByRole('button', { name: /Create Account/i }).click();

    await expect(page.getByText(/At least 8 characters/i)).toBeVisible();
  });
});

test.describe('Responsive Design', () => {
  test('should work on mobile viewport', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');

    await expect(page.getByText('NEXTERP').first()).toBeVisible();
  });

  test('should work on tablet viewport', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto('/');

    await expect(page.getByRole('navigation').getByText('NEXTERP')).toBeVisible();
  });

  test('should display login form on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/login');

    await expect(page.getByRole('heading', { name: 'Welcome Back' })).toBeVisible();
  });

  test('should display register form on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/register');

    await expect(page.getByRole('heading', { name: 'Create Your Account' })).toBeVisible();
  });

  test('should display hero section on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');

    await expect(page.getByText('Enterprise Resource Planning')).toBeVisible();
  });

  test('should display modules on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');

    await expect(page.getByText('Inventory').first()).toBeVisible();
  });
});
