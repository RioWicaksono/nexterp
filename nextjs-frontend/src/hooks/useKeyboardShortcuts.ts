'use client';

import { useEffect, useCallback, useState } from 'react';

export interface KeyboardShortcut {
  key: string;
  ctrl?: boolean;
  shift?: boolean;
  alt?: boolean;
  meta?: boolean;
  action: () => void;
  description?: string;
  enabled?: boolean;
}

interface UseKeyboardShortcutsOptions {
  shortcuts: KeyboardShortcut[];
  enabled?: boolean;
}

/**
 * Hook for registering keyboard shortcuts
 * Supports Ctrl/Cmd + key combinations
 */
export function useKeyboardShortcuts({
  shortcuts,
  enabled = true,
}: UseKeyboardShortcutsOptions) {
  const handleKeyDown = useCallback(
    (event: KeyboardEvent) => {
      if (!enabled) return;

      // Don't trigger shortcuts when typing in inputs
      const target = event.target as HTMLElement;
      const isInputField =
        target.tagName === 'INPUT' ||
        target.tagName === 'TEXTAREA' ||
        target.isContentEditable;

      for (const shortcut of shortcuts) {
        if (shortcut.enabled === false) continue;

        const keyMatch = event.key.toLowerCase() === shortcut.key.toLowerCase();
        const ctrlMatch = shortcut.ctrl ? event.ctrlKey : !event.ctrlKey;
        const shiftMatch = shortcut.shift ? event.shiftKey : !event.shiftKey;
        const altMatch = shortcut.alt ? event.altKey : !event.altKey;
        // Meta key = Cmd on Mac, Win on Windows
        const metaMatch = shortcut.meta
          ? event.metaKey
          : !event.metaKey;

        if (keyMatch && ctrlMatch && shiftMatch && altMatch && metaMatch) {
          // Skip if in input field and shortcut is not Escape
          if (isInputField && shortcut.key.toLowerCase() !== 'escape') {
            continue;
          }

          event.preventDefault();
          event.stopPropagation();
          shortcut.action();
          return;
        }
      }
    },
    [shortcuts, enabled]
  );

  useEffect(() => {
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [handleKeyDown]);
}

/**
 * Common keyboard shortcut presets
 */
export const KeyboardShortcuts = {
  // Navigation
  GO_TO_DASHBOARD: { key: '1', ctrl: true, description: 'Go to Dashboard' },
  GO_TO_HRM: { key: '2', ctrl: true, description: 'Go to HRM' },
  GO_TO_INVENTORY: { key: '3', ctrl: true, description: 'Go to Inventory' },
  GO_TO_PURCHASING: { key: '4', ctrl: true, description: 'Go to Purchasing' },
  GO_TO_SALES: { key: '5', ctrl: true, description: 'Go to Sales' },
  GO_TO_ACCOUNTING: { key: '6', ctrl: true, description: 'Go to Accounting' },
  GO_TO_PROJECTS: { key: '7', ctrl: true, description: 'Go to Projects' },

  // Actions
  SEARCH: { key: 'k', ctrl: true, description: 'Open Quick Search' },
  NEW_ITEM: { key: 'n', ctrl: true, description: 'Create New Item' },
  SAVE: { key: 's', ctrl: true, description: 'Save Current Item' },
  DELETE: { key: 'd', ctrl: true, description: 'Delete Selected' },
  REFRESH: { key: 'r', ctrl: true, description: 'Refresh Data' },

  // Navigation
  ESCAPE: { key: 'Escape', description: 'Close Modal/Dialog' },
  HELP: { key: '?', shift: true, description: 'Show Keyboard Shortcuts' },

  // Quick Actions
  TOGGLE_SIDEBAR: { key: 'b', ctrl: true, description: 'Toggle Sidebar' },
  LOGOUT: { key: 'o', ctrl: true, shift: true, description: 'Logout' },
} as const;
