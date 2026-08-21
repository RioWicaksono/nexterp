/**
 * @jest-environment jsdom
 */
import { renderHook, act, waitFor } from '@testing-library/react';
import { useAutoSave } from './useAutoSave';

// Mock localStorage
const localStorageMock = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
  clear: jest.fn(),
};
Object.defineProperty(global, 'localStorage', { value: localStorageMock });

// Mock timers
beforeEach(() => {
  jest.useFakeTimers();
  localStorageMock.getItem.mockClear();
  localStorageMock.setItem.mockClear();
  localStorageMock.removeItem.mockClear();
});

afterEach(() => {
  jest.useRealTimers();
});

describe('useAutoSave', () => {
  it('should initialize with correct default state', () => {
    const { result } = renderHook(() =>
      useAutoSave({
        formKey: 'test-form',
        data: { name: '' },
      })
    );

    expect(result.current.status).toBe('idle');
    expect(result.current.hasDraft).toBe(false);
    expect(result.current.lastSavedAt).toBeNull();
  });

  it('should update status when saving', async () => {
    const { result } = renderHook(() =>
      useAutoSave({
        formKey: 'test-form',
        data: { name: 'test' },
        debounceMs: 100,
      })
    );

    // Trigger data change
    act(() => {
      jest.advanceTimersByTime(200);
    });

    await waitFor(() => {
      expect(result.current.status).toBe('saved');
    });

    expect(result.current.hasDraft).toBe(true);
  });

  it('should clear draft when clearDraft is called', () => {
    const { result } = renderHook(() =>
      useAutoSave({
        formKey: 'test-form',
        data: { name: 'test' },
      })
    );

    act(() => {
      result.current.clearDraft();
    });

    expect(result.current.hasDraft).toBe(false);
    expect(result.current.lastSavedAt).toBeNull();
  });

  it('should restore draft when restoreDraft is called', async () => {
    const savedData = { name: 'saved draft' };
    localStorageMock.getItem.mockReturnValueOnce(
      JSON.stringify({
        key: 'test-form',
        data: savedData,
        savedAt: Date.now(),
        expiresAt: Date.now() + 7 * 24 * 60 * 60 * 1000,
      })
    );

    const onRestore = jest.fn();
    const { result } = renderHook(() =>
      useAutoSave({
        formKey: 'test-form',
        data: { name: '' },
        onRestore,
      })
    );

    act(() => {
      const restored = result.current.restoreDraft();
      expect(restored).toEqual(savedData);
    });

    expect(onRestore).toHaveBeenCalledWith(savedData);
    expect(result.current.status).toBe('restored');
  });

  it('should not auto-save when enabled is false', () => {
    const { result } = renderHook(() =>
      useAutoSave({
        formKey: 'test-form',
        data: { name: 'test' },
        enabled: false,
      })
    );

    act(() => {
      jest.advanceTimersByTime(1000);
    });

    expect(result.current.status).toBe('idle');
    expect(localStorageMock.setItem).not.toHaveBeenCalled();
  });

  it('should not save empty data', () => {
    const { result } = renderHook(() =>
      useAutoSave({
        formKey: 'test-form',
        data: { name: '', age: null },
      })
    );

    act(() => {
      jest.advanceTimersByTime(2000);
    });

    expect(localStorageMock.setItem).not.toHaveBeenCalled();
  });

  it('should call saveNow to force immediate save', async () => {
    const { result } = renderHook(() =>
      useAutoSave({
        formKey: 'test-form',
        data: { name: 'test' },
      })
    );

    act(() => {
      result.current.saveNow();
    });

    await waitFor(() => {
      expect(result.current.status).toBe('saved');
    });

    expect(localStorageMock.setItem).toHaveBeenCalled();
  });
});
