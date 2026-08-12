# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: app.spec.ts >> Register Page >> should show validation errors
- Location: e2e\app.spec.ts:127:7

# Error details

```
Error: expect(locator).toBeVisible() failed

Locator: getByText(/Full name is required/i)
Expected: visible
Timeout: 5000ms
Error: element(s) not found

Call log:
  - Expect "toBeVisible" with timeout 5000ms
  - waiting for getByText(/Full name is required/i)

```

```yaml
- button "Install app":
  - img
- banner:
  - link "N NEXTERP":
    - /url: /
  - button "Toggle theme":
    - img
- main:
  - heading "Create Your Account" [level=1]
  - paragraph: Start your 14-day free trial. No credit card required.
  - text: Full Name
  - textbox "Full Name":
    - /placeholder: John Doe
  - text: Work Email
  - textbox "Work Email":
    - /placeholder: john@company.com
  - text: Company Name
  - textbox "Company Name":
    - /placeholder: Acme Corporation
  - text: Password
  - textbox "Password":
    - /placeholder: Create a strong password
  - button "Show password":
    - img
  - text: Confirm Password
  - textbox "Confirm Password":
    - /placeholder: Confirm your password
  - checkbox "I agree to the Terms of Service and Privacy Policy"
  - text: I agree to the
  - link "Terms of Service":
    - /url: /terms/
  - text: and
  - link "Privacy Policy":
    - /url: /privacy/
  - button "Create Account"
  - paragraph:
    - text: Already have an account?
    - link "Sign in":
      - /url: /login/
- contentinfo:
  - paragraph: © 2026 NEXTERP by SeVeN-. All rights reserved.
```

# Test source

```ts
  30  |   });
  31  | 
  32  |   test('should navigate to login page', async ({ page }) => {
  33  |     await page.goto('/login');
  34  |     await expect(page).toHaveURL(/\/login/);
  35  |     await expect(page.getByRole('heading', { name: 'Welcome Back' })).toBeVisible();
  36  |   });
  37  | 
  38  |   test('should navigate to register page', async ({ page }) => {
  39  |     await page.goto('/register');
  40  |     await expect(page).toHaveURL(/\/register/);
  41  |     await expect(page.getByRole('heading', { name: 'Create Your Account' })).toBeVisible();
  42  |   });
  43  | 
  44  |   test('should display CTA buttons on desktop', async ({ page }) => {
  45  |     await page.setViewportSize({ width: 1280, height: 720 });
  46  |     await page.goto('/');
  47  | 
  48  |     await expect(page.getByText('Get Started').first()).toBeVisible();
  49  |   });
  50  | });
  51  | 
  52  | test.describe('Login Page', () => {
  53  |   test.beforeEach(async ({ page }) => {
  54  |     await page.goto('/login');
  55  |   });
  56  | 
  57  |   test('should display login form', async ({ page }) => {
  58  |     await expect(page.getByRole('heading', { name: 'Welcome Back' })).toBeVisible();
  59  |     await expect(page.getByLabel(/Username/i)).toBeVisible();
  60  |     await expect(page.getByLabel(/Password/i).first()).toBeVisible();
  61  |   });
  62  | 
  63  |   test('should show validation errors on empty submit', async ({ page }) => {
  64  |     await page.getByRole('button', { name: /Sign In/i }).click();
  65  | 
  66  |     await expect(page.getByText(/Username is required/i)).toBeVisible();
  67  |     await expect(page.getByText(/Password is required/i)).toBeVisible();
  68  |   });
  69  | 
  70  |   test('should show error for invalid credentials', async ({ page }) => {
  71  |     await page.getByLabel(/Username/i).fill('wrong@test.com');
  72  |     await page.getByLabel(/Password/i).first().fill('wrongpass');
  73  |     await page.getByRole('button', { name: /Sign In/i }).click();
  74  | 
  75  |     await expect(page.getByText(/Invalid/i)).toBeVisible();
  76  |   });
  77  | 
  78  |   test('should toggle password visibility', async ({ page }) => {
  79  |     const passwordInput = page.getByLabel(/Password/i).first();
  80  |     const showButton = page.locator('button[aria-label="Show password"]').first();
  81  | 
  82  |     await expect(passwordInput).toHaveAttribute('type', 'password');
  83  | 
  84  |     await showButton.click();
  85  |     await expect(passwordInput).toHaveAttribute('type', 'text');
  86  | 
  87  |     await page.locator('button[aria-label="Hide password"]').click();
  88  |     await expect(passwordInput).toHaveAttribute('type', 'password');
  89  |   });
  90  | 
  91  |   test('should navigate to register page', async ({ page }) => {
  92  |     await page.getByRole('link', { name: /Sign up free/i }).click();
  93  | 
  94  |     await expect(page).toHaveURL(/\/register/);
  95  |   });
  96  | 
  97  |   test('should have forgot password link', async ({ page }) => {
  98  |     await expect(page.getByText(/Forgot password/i)).toBeVisible();
  99  |   });
  100 | 
  101 |   test('should display demo credentials hint', async ({ page }) => {
  102 |     await expect(page.getByText(/Demo Account:/i)).toBeVisible();
  103 |     await expect(page.getByText(/admin@nexterp.com/i)).toBeVisible();
  104 |   });
  105 | 
  106 |   test('should have working theme toggle', async ({ page }) => {
  107 |     const themeButton = page.locator('button[aria-label="Toggle theme"]');
  108 |     await expect(themeButton).toBeVisible();
  109 | 
  110 |     await themeButton.click();
  111 |     // Theme should toggle (no specific assertion needed)
  112 |   });
  113 | });
  114 | 
  115 | test.describe('Register Page', () => {
  116 |   test.beforeEach(async ({ page }) => {
  117 |     await page.goto('/register');
  118 |   });
  119 | 
  120 |   test('should display registration form', async ({ page }) => {
  121 |     await expect(page.getByRole('heading', { name: 'Create Your Account' })).toBeVisible();
  122 |     await expect(page.getByLabel(/Full Name/i)).toBeVisible();
  123 |     await expect(page.getByLabel(/Work Email/i)).toBeVisible();
  124 |     await expect(page.getByLabel(/Company/i)).toBeVisible();
  125 |   });
  126 | 
  127 |   test('should show validation errors', async ({ page }) => {
  128 |     await page.getByRole('button', { name: /Create Account/i }).click();
  129 | 
> 130 |     await expect(page.getByText(/Full name is required/i)).toBeVisible();
      |                                                            ^ Error: expect(locator).toBeVisible() failed
  131 |     await expect(page.getByText(/Email is required/i)).toBeVisible();
  132 |   });
  133 | 
  134 |   test('should show password requirements', async ({ page }) => {
  135 |     await page.getByLabel(/Password/i).first().fill('Test');
  136 | 
  137 |     await expect(page.getByText(/At least 8 characters/i)).toBeVisible();
  138 |     await expect(page.getByText(/One uppercase letter/i)).toBeVisible();
  139 |   });
  140 | 
  141 |   test('should navigate to login page', async ({ page }) => {
  142 |     await page.getByRole('link', { name: /Sign in/i }).click();
  143 | 
  144 |     await expect(page).toHaveURL(/\/login/);
  145 |   });
  146 | 
  147 |   test('should validate password strength', async ({ page }) => {
  148 |     await page.getByLabel(/Password/i).first().fill('weak');
  149 |     await page.getByRole('button', { name: /Create Account/i }).click();
  150 | 
  151 |     await expect(page.getByText(/At least 8 characters/i)).toBeVisible();
  152 |   });
  153 | });
  154 | 
  155 | test.describe('Responsive Design', () => {
  156 |   test('should work on mobile viewport', async ({ page }) => {
  157 |     await page.setViewportSize({ width: 375, height: 667 });
  158 |     await page.goto('/');
  159 | 
  160 |     await expect(page.getByText('NEXTERP').first()).toBeVisible();
  161 |   });
  162 | 
  163 |   test('should work on tablet viewport', async ({ page }) => {
  164 |     await page.setViewportSize({ width: 768, height: 1024 });
  165 |     await page.goto('/');
  166 | 
  167 |     await expect(page.getByRole('navigation').getByText('NEXTERP')).toBeVisible();
  168 |   });
  169 | 
  170 |   test('should display login form on mobile', async ({ page }) => {
  171 |     await page.setViewportSize({ width: 375, height: 667 });
  172 |     await page.goto('/login');
  173 | 
  174 |     await expect(page.getByRole('heading', { name: 'Welcome Back' })).toBeVisible();
  175 |   });
  176 | 
  177 |   test('should display register form on mobile', async ({ page }) => {
  178 |     await page.setViewportSize({ width: 375, height: 667 });
  179 |     await page.goto('/register');
  180 | 
  181 |     await expect(page.getByRole('heading', { name: 'Create Your Account' })).toBeVisible();
  182 |   });
  183 | 
  184 |   test('should display hero section on mobile', async ({ page }) => {
  185 |     await page.setViewportSize({ width: 375, height: 667 });
  186 |     await page.goto('/');
  187 | 
  188 |     await expect(page.getByText('Enterprise Resource Planning')).toBeVisible();
  189 |   });
  190 | 
  191 |   test('should display modules on mobile', async ({ page }) => {
  192 |     await page.setViewportSize({ width: 375, height: 667 });
  193 |     await page.goto('/');
  194 | 
  195 |     await expect(page.getByText('Inventory').first()).toBeVisible();
  196 |   });
  197 | });
  198 | 
```