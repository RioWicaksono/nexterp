import axios from 'axios';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'https://api-production-ab1b.up.railway.app';

const api = axios.create({
  baseURL: `${API_BASE_URL}/api/v1`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token to requests
api.interceptors.request.use((config) => {
  if (typeof window !== 'undefined') {
    const token = localStorage.getItem('nexterp_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
  }
  return config;
});

// Handle auth errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      if (typeof window !== 'undefined') {
        localStorage.removeItem('nexterp_token');
        localStorage.removeItem('nexterp_user');
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  data?: {
    accessToken: string;
    refreshToken: string;
    expiresAt: string;
    user: {
      id: string;
      username: string;
      email: string;
      firstName: string;
      lastName: string;
      fullName: string;
      organizationId: string;
      isActive: boolean;
      isSuperAdmin: boolean;
      roles: string[];
    };
  };
  error?: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
}

export const authApi = {
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('/auth/login', data);
    return response.data;
  },

  logout: async (): Promise<void> => {
    try {
      await api.post('/auth/logout');
    } finally {
      if (typeof window !== 'undefined') {
        localStorage.removeItem('nexterp_token');
        localStorage.removeItem('nexterp_user');
      }
    }
  },

  refresh: async (refreshToken: string): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('/auth/refresh', { refreshToken });
    return response.data;
  },
};

export const modulesApi = {
  getAll: async () => {
    const response = await api.get<ApiResponse<any[]>>('/modules');
    return response.data;
  },
  getEnabled: async (orgId: string) => {
    const response = await api.get<ApiResponse<any[]>>(`/modules/enabled/${orgId}`);
    return response.data;
  },
};

export const dashboardApi = {
  getStats: async () => {
    const response = await api.get<ApiResponse<any>>('/dashboard/stats');
    return response.data;
  },
  getHrmStats: async () => {
    const response = await api.get<ApiResponse<any>>('/hrm/dashboard/stats');
    return response.data;
  },
};

export default api;
