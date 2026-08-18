import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export interface User {
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
}

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (user: User, token: string) => void;
  logout: () => void;
  setLoading: (loading: boolean) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      isAuthenticated: false,
      isLoading: true,

      login: (user, token) => {
        if (typeof window !== 'undefined') {
          localStorage.setItem('nexterp_token', token);
          localStorage.setItem('nexterp_user', JSON.stringify(user));
        }
        set({ user, token, isAuthenticated: true, isLoading: false });
      },

      logout: () => {
        if (typeof window !== 'undefined') {
          localStorage.removeItem('nexterp_token');
          localStorage.removeItem('nexterp_user');
        }
        set({ user: null, token: null, isAuthenticated: false, isLoading: false });
      },

      setLoading: (isLoading) => set({ isLoading }),
    }),
    {
      name: 'nexterp-auth',
      partialize: (state) => ({
        user: state.user,
        token: state.token,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);
