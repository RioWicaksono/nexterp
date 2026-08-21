'use client';

import { useState, useMemo, useCallback } from 'react';
import { Search, Clock, X, FileText, Package, Users, ShoppingCart, DollarSign, TrendingUp, Command } from 'lucide-react';
import { useRouter } from 'next/navigation';
import { cn } from '@/lib/utils';

interface SearchResult {
  id: string;
  type: 'employee' | 'inventory' | 'order' | 'account' | 'project' | 'page';
  title: string;
  subtitle?: string;
  href: string;
  icon?: string;
}

const searchData: SearchResult[] = [
  { id: '1', type: 'page', title: 'Dashboard', href: '/dashboard' },
  { id: '2', type: 'page', title: 'Human Resource', href: '/dashboard/hrm' },
  { id: '3', type: 'page', title: 'Inventory', href: '/dashboard/inventory' },
  { id: '4', type: 'page', title: 'Purchase Orders', href: '/dashboard/purchasing' },
  { id: '5', type: 'page', title: 'Accounting', href: '/dashboard/accounting' },
  { id: '6', type: 'page', title: 'Projects', href: '/dashboard/projects' },
];

const typeConfig: Record<string, { icon: typeof Users; color: string }> = {
  employee: { icon: Users, color: 'bg-blue-100 text-blue-600' },
  inventory: { icon: Package, color: 'bg-green-100 text-green-600' },
  order: { icon: ShoppingCart, color: 'bg-orange-100 text-orange-600' },
  account: { icon: DollarSign, color: 'bg-emerald-100 text-emerald-600' },
  project: { icon: TrendingUp, color: 'bg-purple-100 text-purple-600' },
  page: { icon: FileText, color: 'bg-slate-100 text-slate-600' },
};

export function SearchModal() {
  const [query, setQuery] = useState('');
  const [isOpen, setIsOpen] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState(0);
  const router = useRouter();

  const results = useMemo(() => {
    if (!query.trim()) return searchData.slice(0, 5);
    const q = query.toLowerCase();
    return searchData.filter(
      (r) =>
        r.title.toLowerCase().includes(q) || r.subtitle?.toLowerCase().includes(q)
    ).slice(0, 10);
  }, [query]);

  const handleKeyDown = useCallback((e: React.KeyboardEvent) => {
    if (e.key === 'k' && (e.metaKey || e.ctrlKey)) {
      e.preventDefault();
      setIsOpen(true);
    }
    if (!isOpen) return;
    if (e.key === 'Escape') setIsOpen(false);
    if (e.key === 'ArrowDown') setSelectedIndex((i) => Math.min(i + 1, results.length - 1));
    if (e.key === 'ArrowUp') setSelectedIndex((i) => Math.max(i - 1, 0));
    if (e.key === 'Enter' && results[selectedIndex]) {
      router.push(results[selectedIndex].href);
      setIsOpen(false);
    }
  }, [isOpen, results, selectedIndex, router]);

  if (!isOpen) {
    return (
      <button
        onClick={() => setIsOpen(true)}
        className="flex items-center gap-2 px-3 py-1.5 bg-slate-100 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg text-sm text-slate-400 hover:border-slate-300 dark:hover:border-slate-600 transition"
      >
        <Search className="w-4 h-4" />
        <span className="hidden sm:inline">Search...</span>
        <kbd className="hidden sm:inline px-1.5 py-0.5 text-xs bg-white dark:bg-slate-700 border border-slate-200 dark:border-slate-600 rounded">
          ⌘K
        </kbd>
      </button>
    );
  }

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center pt-[20vh]" onKeyDown={handleKeyDown}>
      <div className="fixed inset-0 bg-black/40" onClick={() => setIsOpen(false)} />
      <div className="relative w-full max-w-lg bg-white dark:bg-slate-800 rounded-xl shadow-2xl border border-slate-200 dark:border-slate-700 overflow-hidden">
        <div className="flex items-center gap-3 px-4 py-3 border-b border-slate-200 dark:border-slate-700">
          <Search className="w-5 h-5 text-slate-400 shrink-0" />
          <input
            autoFocus
            value={query}
            onChange={(e) => { setQuery(e.target.value); setSelectedIndex(0); }}
            placeholder="Search pages, employees, orders..."
            className="flex-1 bg-transparent outline-none text-slate-900 dark:text-white placeholder-slate-400"
          />
          <button onClick={() => setIsOpen(false)} className="shrink-0 p-1 hover:bg-slate-100 dark:hover:bg-slate-700 rounded">
            <X className="w-4 h-4 text-slate-400" />
          </button>
        </div>
        <div className="max-h-80 overflow-y-auto p-2">
          {results.length === 0 ? (
            <p className="px-4 py-8 text-center text-slate-400">No results found</p>
          ) : (
            results.map((result, i) => {
              const config = typeConfig[result.type] || typeConfig.page;
              const Icon = config.icon;
              return (
                <button
                  key={result.id}
                  onClick={() => { router.push(result.href); setIsOpen(false); }}
                  className={cn(
                    'w-full flex items-center gap-3 px-3 py-2 rounded-lg text-left transition',
                    i === selectedIndex ? 'bg-blue-50 dark:bg-blue-900/20' : 'hover:bg-slate-50 dark:hover:bg-slate-700/50'
                  )}
                >
                  <div className={cn('p-1.5 rounded', config.color}>
                    <Icon className="w-4 h-4" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="font-medium text-slate-900 dark:text-white truncate">{result.title}</p>
                    {result.subtitle && <p className="text-sm text-slate-500 truncate">{result.subtitle}</p>}
                  </div>
                  {i === selectedIndex && (
                    <kbd className="shrink-0 px-1.5 py-0.5 text-xs bg-slate-100 dark:bg-slate-700 text-slate-500 rounded">↵</kbd>
                  )}
                </button>
              );
            })
          )}
        </div>
        <div className="px-4 py-2 border-t border-slate-100 dark:border-slate-700 text-xs text-slate-400 flex gap-4">
          <span>↑↓ Navigate</span>
          <span>↵ Open</span>
          <span>Esc Close</span>
        </div>
      </div>
    </div>
  );
}
