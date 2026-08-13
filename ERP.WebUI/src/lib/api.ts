const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'https://api-production-ab1b.up.railway.app';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
  organizationName?: string;
}

export interface RegisterResponse {
  userId: string;
  organizationId: string;
  message: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: {
    id: string;
    organizationId: string;
    username: string;
    email: string;
    firstName: string;
    lastName: string;
    fullName: string;
    isActive: boolean;
    isSuperAdmin: boolean;
  };
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
}

const COOKIE_NAME = 'nexterp_auth';
const TOKEN_COOKIE = 'nexterp_token';

class ApiClient {
  private baseUrl = API_BASE_URL;

  private getCookie(name: string): string | null {
    if (typeof document === 'undefined') return null;
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop()?.split(';').shift() || null;
    return null;
  }

  private setCookie(name: string, value: string, days: number = 7): void {
    if (typeof document === 'undefined') return;
    const expires = new Date(Date.now() + days * 864e5).toUTCString();
    document.cookie = `${name}=${encodeURIComponent(value)}; expires=${expires}; path=/; SameSite=Strict; Secure`;
  }

  private deleteCookie(name: string): void {
    if (typeof document === 'undefined') return;
    document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; SameSite=Strict; Secure`;
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<ApiResponse<T>> {
    const url = `${this.baseUrl}${endpoint}`;

    const config: RequestInit = {
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
      credentials: 'include', // Include cookies in requests
      ...options,
    };

    try {
      const response = await fetch(url, config);
      const data = await response.json();

      if (!response.ok) {
        // Handle 401 by clearing auth
        if (response.status === 401) {
          this.clearAuth();
        }
        return {
          success: false,
          error: data.error || data.message || 'Request failed',
        };
      }

      return {
        success: true,
        data: data.data || data,
      };
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Network error',
      };
    }
  }

  async login(credentials: LoginRequest): Promise<ApiResponse<LoginResponse>> {
    const response = await this.request<LoginResponse>('/api/v1/auth/login', {
      method: 'POST',
      body: JSON.stringify(credentials),
    });

    if (response.success && response.data) {
      // Store tokens in httpOnly cookies (set by server, but also secure store)
      // The server sets httpOnly cookies, we store user info in localStorage for UI
      this.setCookie(TOKEN_COOKIE, response.data.accessToken, 1); // Short-lived cookie
      localStorage.setItem('user', JSON.stringify(response.data.user));
      // Store a flag that auth is complete (without exposing token)
      localStorage.setItem('auth_complete', 'true');
    }

    return response;
  }

  async register(data: RegisterRequest): Promise<ApiResponse<RegisterResponse>> {
    return this.request<RegisterResponse>('/api/v1/auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  logout(): void {
    // Clear cookies (httpOnly cookies are cleared by server on logout endpoint)
    this.deleteCookie(TOKEN_COOKIE);
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    localStorage.removeItem('auth_complete');
  }

  clearAuth(): void {
    this.logout();
  }

  getStoredToken(): string | null {
    // Tokens are now in httpOnly cookies, read via SSR API route or cookie header
    // For client-side, we rely on credentials: 'include' to send cookies
    return this.getCookie(TOKEN_COOKIE);
  }

  getStoredUser(): LoginResponse['user'] | null {
    if (typeof window === 'undefined') return null;
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }

  isAuthenticated(): boolean {
    // Check both cookie and localStorage flag
    const hasToken = !!this.getStoredToken();
    const hasAuthFlag = localStorage.getItem('auth_complete') === 'true';
    return hasToken || hasAuthFlag;
  }

  async get<T>(endpoint: string): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, {
      method: 'GET',
    });
  }

  async post<T>(endpoint: string, body: unknown): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, {
      method: 'POST',
      body: JSON.stringify(body),
    });
  }
}

export const api = new ApiClient();
