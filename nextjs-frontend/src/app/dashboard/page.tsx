'use client';

import { useState, useCallback } from 'react';
import { DraggableGrid } from '@/components/dashboard/DraggableGrid';
import { useDashboardStore } from '@/stores/dashboardStore';
import { useAuthStore } from '@/lib/store';
import { RefreshCw, Layout, Lock, Unlock, RotateCcw, Settings, Pencil } from 'lucide-react';

export default function DashboardPage() {
  const { user } = useAuthStore();
  const { isLayoutLocked, setLayoutLocked, resetToDefault } = useDashboardStore();
  const [stats, setStats] = useState({
    employees: '-',
    inventory: '-',
    orders: '-',
    suppliers: '-',
    projects: '-',
    accounts: '-',
  });
  const [loading, setLoading] = useState(true);

  const fetchStats = useCallback(async () => {
    setLoading(true);
    try {
      const res = await fetch('/api/dashboard/stats');
      const data = await res.json();
      if (data.success) {
        setStats({
          employees: String(data.data?.totalEmployees ?? 0),
          inventory: String(data.data?.totalInventoryItems ?? 0),
          orders: String(data.data?.totalPurchaseOrders ?? 0),
          suppliers: String(data.data?.totalSuppliers ?? 0),
          projects: String(data.data?.totalProjects ?? 0),
          accounts: String(data.data?.totalAccounts ?? 0),
        });
      }
    } catch {
      // silently fail
    } finally {
      setLoading(false);
    }
  }, []);

  // eslint-disable-next-line react-hooks/exhaustive-deps
  useState(() => { fetchStats(); });

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white">
            Welcome back, {user?.firstName || 'User'}!
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            {new Date().toLocaleDateString('id-ID', {
              weekday: 'long',
              year: 'numeric',
              month: 'long',
              day: 'numeric',
            })}
          </p>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={fetchStats}
            className="flex items-center gap-2 px-3 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 text-sm transition"
            aria-label="Refresh data"
          >
            <RefreshCw className="w-4 h-4" />
            Refresh
          </button>

          <button
            onClick={() => setLayoutLocked(!isLayoutLocked)}
            className={`flex items-center gap-2 px-3 py-2 border rounded-lg text-sm transition ${
              isLayoutLocked
                ? 'bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800 text-blue-600 dark:text-blue-400'
                : 'bg-white dark:bg-slate-800 border-slate-200 dark:border-slate-700 text-slate-600 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-700'
            }`}
            aria-label={isLayoutLocked ? 'Unlock layout' : 'Lock layout'}
            title={isLayoutLocked ? 'Unlock layout' : 'Lock layout'}
          >
            {isLayoutLocked ? (
              <Lock className="w-4 h-4" />
            ) : (
              <Unlock className="w-4 h-4" />
            )}
            {isLayoutLocked ? 'Locked' : 'Lock'}
          </button>

          <button
            onClick={resetToDefault}
            className="flex items-center gap-2 px-3 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 text-sm text-slate-600 dark:text-slate-400 transition"
            aria-label="Reset to default"
            title="Reset layout"
          >
            <RotateCcw className="w-4 h-4" />
          </button>
        </div>
      </div>

      {/* Info Banner */}
      {!isLayoutLocked && (
        <div className="flex items-center gap-2 px-4 py-2 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg text-sm text-blue-600 dark:text-blue-400">
          <Layout className="w-4 h-4" />
          <span>Drag widgets by their handle to reorder. Click the icons to resize, hide, or customize each widget.</span>
        </div>
      )}

      {/* Draggable Grid */}
      <DraggableGrid stats={stats} isLoading={loading} />
    </div>
  );
}
