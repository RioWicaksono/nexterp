'use client';

import { useEffect, useState, useCallback, useRef } from 'react';
import { useRouter } from 'next/navigation';

interface CommandItem {
  id: string;
  label: string;
  icon?: string;
  shortcut?: string;
  action: () => void;
  category?: string;
}

interface CommandPaletteProps {
  isOpen: boolean;
  onClose: () => void;
  items: CommandItem[];
}

/**
 * Command Palette (Cmd+K style) component
 * Provides quick access to all app features
 */
export function CommandPalette({ isOpen, onClose, items }: CommandPaletteProps) {
  const [query, setQuery] = useState('');
  const [selectedIndex, setSelectedIndex] = useState(0);
  const inputRef = useRef<HTMLInputElement>(null);

  const filteredItems = items.filter((item) =>
    item.label.toLowerCase().includes(query.toLowerCase())
  );

  const executeCommand = useCallback(
    (item: CommandItem) => {
      onClose();
      setQuery('');
      setSelectedIndex(0);
      item.action();
    },
    [onClose]
  );

  useEffect(() => {
    if (isOpen && inputRef.current) {
      inputRef.current.focus();
    }
  }, [isOpen]);

  useEffect(() => {
    setSelectedIndex(0);
  }, [query]);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (!isOpen) return;

      switch (e.key) {
        case 'ArrowDown':
          e.preventDefault();
          setSelectedIndex((prev) =>
            prev < filteredItems.length - 1 ? prev + 1 : 0
          );
          break;
        case 'ArrowUp':
          e.preventDefault();
          setSelectedIndex((prev) =>
            prev > 0 ? prev - 1 : filteredItems.length - 1
          );
          break;
        case 'Enter':
          e.preventDefault();
          if (filteredItems[selectedIndex]) {
            executeCommand(filteredItems[selectedIndex]);
          }
          break;
        case 'Escape':
          e.preventDefault();
          onClose();
          break;
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, filteredItems, selectedIndex, executeCommand, onClose]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-hidden">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-black/50 backdrop-blur-sm transition-opacity"
        onClick={onClose}
      />

      {/* Palette */}
      <div className="absolute left-1/2 top-1/4 w-full max-w-2xl -translate-x-1/2">
        <div className="mx-4 overflow-hidden rounded-xl bg-white shadow-2xl dark:bg-gray-800">
          {/* Search Input */}
          <div className="flex items-center border-b border-gray-200 px-4 dark:border-gray-700">
            <svg
              className="h-5 w-5 text-gray-400"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
              />
            </svg>
            <input
              ref={inputRef}
              type="text"
              placeholder="Type a command or search..."
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              className="flex-1 border-0 bg-transparent py-4 px-3 text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-0 dark:text-gray-100"
            />
            <kbd className="rounded bg-gray-100 px-2 py-1 text-xs font-medium text-gray-500 dark:bg-gray-700 dark:text-gray-400">
              ESC
            </kbd>
          </div>

          {/* Results */}
          <div className="max-h-96 overflow-y-auto py-2">
            {filteredItems.length === 0 ? (
              <div className="px-4 py-8 text-center text-gray-500 dark:text-gray-400">
                No commands found
              </div>
            ) : (
              <ul>
                {filteredItems.map((item, index) => (
                  <li key={item.id}>
                    <button
                      onClick={() => executeCommand(item)}
                      className={`flex w-full items-center justify-between px-4 py-3 text-left transition-colors ${
                        index === selectedIndex
                          ? 'bg-blue-50 text-blue-600 dark:bg-blue-900/30 dark:text-blue-400'
                          : 'text-gray-700 hover:bg-gray-50 dark:text-gray-200 dark:hover:bg-gray-700'
                      }`}
                    >
                      <div className="flex items-center gap-3">
                        {item.icon && (
                          <span className="text-lg">{item.icon}</span>
                        )}
                        <div>
                          <div className="font-medium">{item.label}</div>
                          {item.category && (
                            <div className="text-xs text-gray-500 dark:text-gray-400">
                              {item.category}
                            </div>
                          )}
                        </div>
                      </div>
                      {item.shortcut && (
                        <kbd className="rounded bg-gray-100 px-2 py-1 text-xs font-medium text-gray-500 dark:bg-gray-700 dark:text-gray-400">
                          {item.shortcut}
                        </kbd>
                      )}
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          {/* Footer */}
          <div className="flex items-center justify-between border-t border-gray-200 px-4 py-2 text-xs text-gray-500 dark:border-gray-700 dark:text-gray-400">
            <div className="flex items-center gap-4">
              <span>Navigate</span>
              <span>Select</span>
              <span>Close</span>
            </div>
            <span>NEXTERP Command Palette</span>
          </div>
        </div>
      </div>
    </div>
  );
}

/**
 * Hook to manage command palette
 */
export function useCommandPalette() {
  const [isOpen, setIsOpen] = useState(false);
  const router = useRouter();

  const commands: CommandItem[] = [
    // Navigation
    { id: 'nav-dashboard', label: 'Dashboard', icon: '📊', shortcut: 'Ctrl+1', category: 'Navigation', action: () => router.push('/dashboard') },
    { id: 'nav-hrm', label: 'Human Resources', icon: '👥', shortcut: 'Ctrl+2', category: 'Navigation', action: () => router.push('/dashboard/hrm') },
    { id: 'nav-inventory', label: 'Inventory', icon: '📦', shortcut: 'Ctrl+3', category: 'Navigation', action: () => router.push('/dashboard/inventory') },
    { id: 'nav-purchasing', label: 'Purchasing', icon: '🛒', shortcut: 'Ctrl+4', category: 'Navigation', action: () => router.push('/dashboard/purchasing') },
    { id: 'nav-sales', label: 'Sales', icon: '💰', shortcut: 'Ctrl+5', category: 'Navigation', action: () => router.push('/dashboard/sales') },
    { id: 'nav-accounting', label: 'Accounting', icon: '📒', shortcut: 'Ctrl+6', category: 'Navigation', action: () => router.push('/dashboard/accounting') },
    { id: 'nav-projects', label: 'Projects', icon: '📁', shortcut: 'Ctrl+7', category: 'Navigation', action: () => router.push('/dashboard/projects') },
    { id: 'nav-roles', label: 'Roles & Permissions', icon: '🔐', category: 'Navigation', action: () => router.push('/dashboard/roles') },
    { id: 'nav-modules', label: 'Modules', icon: '🧩', category: 'Navigation', action: () => router.push('/dashboard/modules') },
    { id: 'nav-settings', label: 'Settings', icon: '⚙️', category: 'Navigation', action: () => router.push('/dashboard/settings') },

    // Actions
    { id: 'action-logout', label: 'Logout', icon: '🚪', shortcut: 'Ctrl+Shift+O', category: 'Actions', action: () => {
      document.cookie = 'nexterp_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
      document.cookie = 'nexterp_refresh=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
      router.push('/login');
    }},
  ];

  const open = useCallback(() => setIsOpen(true), []);
  const close = useCallback(() => setIsOpen(false), []);
  const toggle = useCallback(() => setIsOpen((prev) => !prev), []);

  return { isOpen, commands, open, close, toggle };
}
