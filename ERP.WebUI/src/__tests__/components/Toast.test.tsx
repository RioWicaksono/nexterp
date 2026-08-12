/**
 * Unit Tests for Toast Provider
 */

import { renderHook, act } from '@testing-library/react';
import { ToastProvider, useToast } from '@/app/providers/ToastProvider';

describe('Toast Provider', () => {
  it('should provide toast functions', () => {
    const { result } = renderHook(() => useToast(), {
      wrapper: ToastProvider,
    });

    expect(typeof result.current.success).toBe('function');
    expect(typeof result.current.error).toBe('function');
    expect(typeof result.current.warning).toBe('function');
    expect(typeof result.current.info).toBe('function');
    expect(typeof result.current.removeToast).toBe('function');
    expect(Array.isArray(result.current.toasts)).toBe(true);
  });

  it('should add toast when success is called', () => {
    const { result } = renderHook(() => useToast(), {
      wrapper: ToastProvider,
    });

    expect(result.current.toasts).toHaveLength(0);

    act(() => {
      result.current.success('Test Success', 'Message');
    });

    expect(result.current.toasts).toHaveLength(1);
    expect(result.current.toasts[0].type).toBe('success');
    expect(result.current.toasts[0].title).toBe('Test Success');
  });

  it('should add toast when error is called', () => {
    const { result } = renderHook(() => useToast(), {
      wrapper: ToastProvider,
    });

    act(() => {
      result.current.error('Test Error', 'Error message');
    });

    expect(result.current.toasts).toHaveLength(1);
    expect(result.current.toasts[0].type).toBe('error');
    expect(result.current.toasts[0].title).toBe('Test Error');
  });

  it('should add toast when warning is called', () => {
    const { result } = renderHook(() => useToast(), {
      wrapper: ToastProvider,
    });

    act(() => {
      result.current.warning('Test Warning');
    });

    expect(result.current.toasts).toHaveLength(1);
    expect(result.current.toasts[0].type).toBe('warning');
  });

  it('should add toast when info is called', () => {
    const { result } = renderHook(() => useToast(), {
      wrapper: ToastProvider,
    });

    act(() => {
      result.current.info('Test Info', 'Info message');
    });

    expect(result.current.toasts).toHaveLength(1);
    expect(result.current.toasts[0].type).toBe('info');
    expect(result.current.toasts[0].message).toBe('Info message');
  });

  it('should remove toast by id', () => {
    const { result } = renderHook(() => useToast(), {
      wrapper: ToastProvider,
    });

    act(() => {
      result.current.success('Test');
    });

    expect(result.current.toasts).toHaveLength(1);
    const toastId = result.current.toasts[0].id;

    act(() => {
      result.current.removeToast(toastId);
    });

    expect(result.current.toasts).toHaveLength(0);
  });

  it('should add multiple toasts', () => {
    const { result } = renderHook(() => useToast(), {
      wrapper: ToastProvider,
    });

    act(() => {
      result.current.success('Success 1');
      result.current.error('Error 1');
      result.current.warning('Warning 1');
      result.current.info('Info 1');
    });

    expect(result.current.toasts).toHaveLength(4);
  });

  it('should generate unique IDs for toasts', () => {
    const { result } = renderHook(() => useToast(), {
      wrapper: ToastProvider,
    });

    act(() => {
      result.current.success('Toast 1');
      result.current.success('Toast 2');
    });

    expect(result.current.toasts[0].id).not.toBe(result.current.toasts[1].id);
  });
});
