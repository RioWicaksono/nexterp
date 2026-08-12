/**
 * Unit Tests for I18n Provider
 */

import { renderHook, act, waitFor } from '@testing-library/react';
import { I18nProvider, useI18n } from '@/app/providers/I18nProvider';

// Mock localStorage
let mockLocalStorage: Record<string, string> = {};

beforeEach(() => {
  mockLocalStorage = {};
  Object.defineProperty(window, 'localStorage', {
    value: {
      getItem: jest.fn((key: string) => mockLocalStorage[key] || null),
      setItem: jest.fn((key: string, value: string) => { mockLocalStorage[key] = value; }),
      removeItem: jest.fn((key: string) => { delete mockLocalStorage[key]; }),
      clear: jest.fn(() => { mockLocalStorage = {}; }),
    },
    writable: true,
  });
});

describe('I18n Provider', () => {
  it('should provide translations', () => {
    const { result } = renderHook(() => useI18n(), {
      wrapper: I18nProvider,
    });

    expect(result.current.locale).toBe('en');
    expect(typeof result.current.t).toBe('function');
    expect(typeof result.current.setLocale).toBe('function');
  });

  it('should translate dashboard title in English', async () => {
    const { result } = renderHook(() => useI18n(), {
      wrapper: I18nProvider,
    });

    await waitFor(() => {
      expect(result.current.locale).toBe('en');
    });

    expect(result.current.t('dashboard.title')).toBe('Dashboard');
  });

  it('should switch locale to Indonesian', async () => {
    const { result } = renderHook(() => useI18n(), {
      wrapper: I18nProvider,
    });

    await waitFor(() => {
      expect(result.current.locale).toBe('en');
    });

    act(() => {
      result.current.setLocale('id');
    });

    await waitFor(() => {
      expect(result.current.t('dashboard.title')).toBe('Dasbor');
    });
  });

  it('should persist locale to localStorage', async () => {
    const { result } = renderHook(() => useI18n(), {
      wrapper: I18nProvider,
    });

    await waitFor(() => {
      expect(result.current.locale).toBe('en');
    });

    act(() => {
      result.current.setLocale('id');
    });

    await waitFor(() => {
      expect(window.localStorage.setItem).toHaveBeenCalledWith('nexterp-locale', 'id');
    });
  });

  it('should handle translation parameters', async () => {
    const { result } = renderHook(() => useI18n(), {
      wrapper: I18nProvider,
    });

    await waitFor(() => {
      expect(result.current.locale).toBe('en');
    });

    const translated = result.current.t('errors.minLength', { min: 8 });
    expect(translated).toContain('8');
  });

  it('should translate common labels', async () => {
    const { result } = renderHook(() => useI18n(), {
      wrapper: I18nProvider,
    });

    await waitFor(() => {
      expect(result.current.locale).toBe('en');
    });

    expect(result.current.t('common.save')).toBe('Save');
    expect(result.current.t('common.cancel')).toBe('Cancel');
    expect(result.current.t('common.loading')).toBe('Loading...');
  });

  it('should translate navigation labels', async () => {
    const { result } = renderHook(() => useI18n(), {
      wrapper: I18nProvider,
    });

    await waitFor(() => {
      expect(result.current.locale).toBe('en');
    });

    expect(result.current.t('nav.dashboard')).toBe('Dashboard');
    expect(result.current.t('nav.inventory')).toBe('Inventory');
    expect(result.current.t('nav.sales')).toBe('Sales');
  });
});
