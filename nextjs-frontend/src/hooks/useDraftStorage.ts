/**
 * Low-level localStorage operations for form draft persistence
 */

const DRAFT_PREFIX = 'nexterp_draft_';
const DRAFT_INDEX_KEY = 'nexterp_draft_index';

export interface DraftEntry<T = Record<string, unknown>> {
  key: string;
  data: T;
  savedAt: number;
  expiresAt: number;
}

function getKey(formKey: string): string {
  return `${DRAFT_PREFIX}${formKey}`;
}

function getIndex(): Record<string, number> {
  if (typeof window === 'undefined') return {};
  try {
    const raw = localStorage.getItem(DRAFT_INDEX_KEY);
    return raw ? JSON.parse(raw) : {};
  } catch {
    return {};
  }
}

function updateIndex(index: Record<string, number>): void {
  if (typeof window === 'undefined') return;
  try {
    localStorage.setItem(DRAFT_INDEX_KEY, JSON.stringify(index));
  } catch {
    // Storage full or unavailable
  }
}

export const draftStorage = {
  /**
   * Save form data to localStorage
   */
  save<T>(formKey: string, data: T, ttlMs = 7 * 24 * 60 * 60 * 1000): void {
    if (typeof window === 'undefined') return;
    const key = getKey(formKey);
    const now = Date.now();
    const entry: DraftEntry<T> = {
      key: formKey,
      data,
      savedAt: now,
      expiresAt: now + ttlMs,
    };
    try {
      localStorage.setItem(key, JSON.stringify(entry));
      const index = getIndex();
      index[formKey] = now;
      updateIndex(index);
    } catch {
      // Storage full - try to clean up old drafts
      draftStorage.cleanup();
      try {
        localStorage.setItem(key, JSON.stringify(entry));
      } catch {
        // Still full, give up
      }
    }
  },

  /**
   * Load form data from localStorage
   */
  load<T>(formKey: string): DraftEntry<T> | null {
    if (typeof window === 'undefined') return null;
    try {
      const raw = localStorage.getItem(getKey(formKey));
      if (!raw) return null;
      const entry = JSON.parse(raw) as DraftEntry<T>;
      if (Date.now() > entry.expiresAt) {
        draftStorage.remove(formKey);
        return null;
      }
      return entry;
    } catch {
      return null;
    }
  },

  /**
   * Remove draft from localStorage
   */
  remove(formKey: string): void {
    if (typeof window === 'undefined') return;
    try {
      localStorage.removeItem(getKey(formKey));
      const index = getIndex();
      delete index[formKey];
      updateIndex(index);
    } catch {
      // Ignore
    }
  },

  /**
   * Check if a draft exists
   */
  exists(formKey: string): boolean {
    return draftStorage.load(formKey) !== null;
  },

  /**
   * Get all draft keys
   */
  getAllKeys(): string[] {
    return Object.keys(getIndex());
  },

  /**
   * Get all drafts with metadata (without data payload)
   */
  getAllDrafts(): Array<{ key: string; savedAt: number; expiresAt: number }> {
    const index = getIndex();
    const drafts: Array<{ key: string; savedAt: number; expiresAt: number }> = [];
    for (const key of Object.keys(index)) {
      const entry = draftStorage.load(key);
      if (entry) {
        drafts.push({ key, savedAt: entry.savedAt, expiresAt: entry.expiresAt });
      }
    }
    return drafts.sort((a, b) => b.savedAt - a.savedAt);
  },

  /**
   * Remove all expired drafts
   */
  cleanup(): void {
    if (typeof window === 'undefined') return;
    const index = getIndex();
    const now = Date.now();
    const keysToRemove: string[] = [];

    for (const [key, savedAt] of Object.entries(index)) {
      const raw = localStorage.getItem(getKey(key));
      if (!raw) {
        keysToRemove.push(key);
        continue;
      }
      try {
        const entry = JSON.parse(raw) as DraftEntry;
        if (now > entry.expiresAt) {
          keysToRemove.push(key);
        }
      } catch {
        keysToRemove.push(key);
      }
    }

    for (const key of keysToRemove) {
      localStorage.removeItem(getKey(key));
      delete index[key];
    }
    updateIndex(index);
  },

  /**
   * Clear all drafts
   */
  clearAll(): void {
    if (typeof window === 'undefined') return;
    const index = getIndex();
    for (const key of Object.keys(index)) {
      localStorage.removeItem(getKey(key));
    }
    localStorage.removeItem(DRAFT_INDEX_KEY);
  },
};
