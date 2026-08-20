'use client';

import { useEffect, useRef, useState, useCallback } from 'react';
import { draftStorage, type DraftEntry } from './useDraftStorage';

export type AutoSaveStatus = 'idle' | 'saving' | 'saved' | 'restored' | 'error';

export interface UseAutoSaveOptions<T> {
  /** Unique key to identify this form */
  formKey: string;
  /** Current form data to auto-save */
  data: T;
  /** Debounce delay in ms (default: 1000) */
  debounceMs?: number;
  /** TTL for draft in ms (default: 7 days) */
  ttlMs?: number;
  /** Called when draft is restored */
  onRestore?: (data: T) => void;
  /** Called when draft is saved */
  onSave?: () => void;
  /** Called on save error */
  onError?: (error: Error) => void;
  /** Enable/disable auto-save (default: true when modal open) */
  enabled?: boolean;
}

export interface UseAutoSaveReturn<T> {
  /** Current save status */
  status: AutoSaveStatus;
  /** Timestamp of last save */
  lastSavedAt: number | null;
  /** Whether a draft exists */
  hasDraft: boolean;
  /** Restore the draft and return its data */
  restoreDraft: () => T | null;
  /** Delete the current draft */
  clearDraft: () => void;
  /** Force save immediately */
  saveNow: () => void;
}

export function useAutoSave<T extends Record<string, unknown>>({
  formKey,
  data,
  debounceMs = 1000,
  ttlMs = 7 * 24 * 60 * 60 * 1000,
  onRestore,
  onSave,
  onError,
  enabled = true,
}: UseAutoSaveOptions<T>): UseAutoSaveReturn<T> {
  const [status, setStatus] = useState<AutoSaveStatus>('idle');
  const [lastSavedAt, setLastSavedAt] = useState<number | null>(null);
  const [hasDraft, setHasDraft] = useState(false);
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const prevDataRef = useRef<string>('');
  const formKeyRef = useRef(formKey);

  // Update formKey ref when it changes
  useEffect(() => {
    formKeyRef.current = formKey;
  }, [formKey]);

  // Check for existing draft on mount
  useEffect(() => {
    if (!enabled) return;
    const draft = draftStorage.load<T>(formKey);
    setHasDraft(draft !== null);
    if (draft) {
      setLastSavedAt(draft.savedAt);
    }
  }, [formKey, enabled]);

  // Save function
  const save = useCallback(() => {
    if (!formKeyRef.current) return;
    setStatus('saving');
    try {
      draftStorage.save(formKeyRef.current, data, ttlMs);
      const now = Date.now();
      setLastSavedAt(now);
      setHasDraft(true);
      setStatus('saved');
      onSave?.();
      // Reset to idle after 2s
      setTimeout(() => setStatus('idle'), 2000);
    } catch (err) {
      setStatus('error');
      onError?.(err instanceof Error ? err : new Error('Failed to save draft'));
    }
  }, [data, ttlMs, onSave, onError]);

  // Debounced save on data change
  useEffect(() => {
    if (!enabled) return;

    const dataStr = JSON.stringify(data);
    if (dataStr === prevDataRef.current) return;
    prevDataRef.current = dataStr;

    // Clear existing timeout
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }

    // Don't auto-save empty data
    const isEmpty = Object.values(data).every((v) => v === '' || v === null || v === undefined);
    if (isEmpty) return;

    timeoutRef.current = setTimeout(save, debounceMs);

    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, [data, debounceMs, save, enabled]);

  // Cleanup timeout on unmount
  useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, []);

  const restoreDraft = useCallback((): T | null => {
    const entry = draftStorage.load<T>(formKey);
    if (!entry) return null;
    setStatus('restored');
    onRestore?.(entry.data);
    setTimeout(() => setStatus('idle'), 2000);
    return entry.data;
  }, [formKey, onRestore]);

  const clearDraft = useCallback(() => {
    draftStorage.remove(formKey);
    setHasDraft(false);
    setLastSavedAt(null);
    setStatus('idle');
    prevDataRef.current = '';
  }, [formKey]);

  const saveNow = useCallback(() => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }
    save();
  }, [save]);

  return {
    status,
    lastSavedAt,
    hasDraft,
    restoreDraft,
    clearDraft,
    saveNow,
  };
}
