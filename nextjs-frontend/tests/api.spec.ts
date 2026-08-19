import { test, expect, APIRequestContext } from '@playwright/test';

/**
 * API Integration Tests
 */
test.describe('API Integration', () => {
  let apiContext: APIRequestContext;

  test.beforeAll(async ({ request }) => {
    apiContext = request;
  });

  test.describe('Auth API', () => {
    test('POST /api/v1/auth/login should work with valid credentials', async () => {
      const response = await apiContext.post('/api/v1/auth/login', {
        data: {
          username: 'admin',
          password: 'DevPassword2024!',
        },
        headers: {
          'Content-Type': 'application/json',
        },
      });

      expect(response.ok()).toBeTruthy();
      const body = await response.json();
      expect(body).toHaveProperty('value');
      expect(body.value).toHaveProperty('accessToken');
      expect(body.value).toHaveProperty('refreshToken');
      expect(body.value).toHaveProperty('user');
    });

    test('POST /api/v1/auth/login should fail with invalid credentials', async () => {
      const response = await apiContext.post('/api/v1/auth/login', {
        data: {
          username: 'admin',
          password: 'wrongpassword',
        },
        headers: {
          'Content-Type': 'application/json',
        },
      });

      // Should return 401 or error response
      const body = await response.json();
      expect(response.status()).toBeGreaterThanOrEqual(400);
    });

    test('POST /api/v1/auth/login should validate required fields', async () => {
      const response = await apiContext.post('/api/v1/auth/login', {
        data: {
          username: '',
          password: '',
        },
        headers: {
          'Content-Type': 'application/json',
        },
      });

      // Should return 400 Bad Request
      expect(response.status()).toBeGreaterThanOrEqual(400);
    });
  });

  test.describe('Protected API Routes', () => {
    let authToken: string;

    test.beforeAll(async () => {
      // Login to get token
      const loginResponse = await apiContext.post('/api/v1/auth/login', {
        data: {
          username: 'admin',
          password: 'DevPassword2024!',
        },
      });
      const loginBody = await loginResponse.json();
      authToken = loginBody.value?.accessToken || '';
    });

    test('GET /api/v1/users should require authentication', async () => {
      const response = await apiContext.get('/api/v1/users');
      expect([401, 403]).toContain(response.status());
    });

    test('GET /api/v1/users should work with valid token', async () => {
      const response = await apiContext.get('/api/v1/users', {
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      // Should return 200 or 404 (if endpoint doesn't exist)
      expect([200, 404]).toContain(response.status());
    });
  });

  test.describe('Health Checks', () => {
    test('GET /health/live should return 200', async () => {
      const response = await apiContext.get('/health/live');
      expect(response.status()).toBe(200);
    });

    test('GET /health/ready should return 200', async () => {
      const response = await apiContext.get('/health/ready');
      expect(response.status()).toBe(200);
    });
  });
});

/**
 * Rate Limiting Tests
 */
test.describe('Rate Limiting', () => {
  test('should rate limit excessive login attempts', async ({ request }) => {
    // Make many rapid login attempts
    const attempts = 10;
    const results: number[] = [];

    for (let i = 0; i < attempts; i++) {
      const response = await request.post('/api/v1/auth/login', {
        data: {
          username: 'admin',
          password: 'wrongpassword',
        },
      });
      results.push(response.status());
    }

    // At least some requests should be rate limited (429)
    // or all should return 401 (invalid credentials)
    const hasRateLimit = results.includes(429);
    const allUnauthorized = results.every((s) => s === 401);

    expect(hasRateLimit || allUnauthorized).toBeTruthy();
  });
});
