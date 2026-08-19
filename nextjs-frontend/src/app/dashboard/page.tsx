'use client';

import { useEffect, useState } from 'react';
import { useAuthStore } from '@/lib/store';
import { dashboardApi } from '@/lib/api';
import {
  Users, Package, ShoppingCart, DollarSign, Building2,
  ArrowUpRight, ArrowDownRight, Loader2, RefreshCw, Activity
} from 'lucide-react';

interface DashboardStats {
  totalEmployees?: number;
  totalInventoryItems?: number;
  totalPurchaseOrders?: number;
  totalSuppliers?: number;
  totalProjects?: number;
  totalAccounts?: number;
  recentActivities?: { description: string; module: string; timestamp: string }[];
}

export default function DashboardPage() {
  const { user } = useAuthStore();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [refreshing, setRefreshing] = useState(false);

  const fetchStats = async () => {
    try {
      const result = await dashboardApi.getStats();
      if (result?.success && result.data) {
        setStats(result.data);
      } else {
        // Fallback to defaults if API returns empty
        setStats({
          totalEmployees: 0,
          totalInventoryItems: 0,
          totalPurchaseOrders: 0,
          totalSuppliers: 0,
          totalProjects: 0,
          totalAccounts: 0,
          recentActivities: [],
        });
      }
    } catch (err: any) {
      console.error('Failed to fetch stats:', err);
      setError('Failed to load dashboard data');
      // Set defaults on error
      setStats({
        totalEmployees: 0,
        totalInventoryItems: 0,
        totalPurchaseOrders: 0,
        totalSuppliers: 0,
        totalProjects: 0,
        totalAccounts: 0,
        recentActivities: [],
      });
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    fetchStats();
  }, []);

  const handleRefresh = () => {
    setRefreshing(true);
    fetchStats();
  };

  const statCards = [
    { name: 'Total Employees', value: stats?.totalEmployees ?? '-', icon: Users, color: 'blue', href: '/dashboard/hrm' },
    { name: 'Inventory Items', value: stats?.totalInventoryItems ?? '-', icon: Package, color: 'green', href: '/dashboard/inventory' },
    { name: 'Purchase Orders', value: stats?.totalPurchaseOrders ?? '-', icon: ShoppingCart, color: 'orange', href: '/dashboard/purchasing' },
    { name: 'Total Suppliers', value: stats?.totalSuppliers ?? '-', icon: Building2, color: 'purple', href: '/dashboard/purchasing' },
    { name: 'Active Projects', value: stats?.totalProjects ?? '-', icon: Activity, color: 'cyan', href: '/dashboard/projects' },
    { name: 'Chart of Accounts', value: stats?.totalAccounts ?? '-', icon: DollarSign, color: 'emerald', href: '/dashboard/accounting' },
  ];

  const colorClasses: Record<string, string> = {
    blue: 'bg-blue-500',
    green: 'bg-green-500',
    orange: 'bg-orange-500',
    purple: 'bg-purple-500',
    cyan: 'bg-cyan-500',
    emerald: 'bg-emerald-500',
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <Loader2 className="w-8 h-8 animate-spin text-blue-600" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold text-slate-900 dark:text-white">
            Welcome back, {user?.firstName || user?.fullName || 'User'}!
          </h2>
          <p className="text-slate-500 dark:text-slate-400 mt-1">
            {new Date().toLocaleDateString('id-ID', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}
          </p>
        </div>
        <button
          onClick={handleRefresh}
          disabled={refreshing}
          className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 transition disabled:opacity-50"
        >
          <RefreshCw className={`w-4 h-4 ${refreshing ? 'animate-spin' : ''}`} />
          Refresh
        </button>
      </div>

      {/* Error Banner */}
      {error && (
        <div className="p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg text-red-600 dark:text-red-400 text-sm">
          {error}
        </div>
      )}

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {statCards.map((stat) => (
          <a
            key={stat.name}
            href={stat.href}
            className="bg-white dark:bg-slate-800 rounded-xl p-5 shadow-sm border border-slate-200 dark:border-slate-700 hover:shadow-md hover:border-blue-300 dark:hover:border-blue-600 transition group"
          >
            <div className="flex items-center justify-between mb-3">
              <div className={`p-2.5 rounded-lg ${colorClasses[stat.color]}`}>
                <stat.icon className="w-5 h-5 text-white" />
              </div>
              <ArrowUpRight className="w-4 h-4 text-slate-400 group-hover:text-blue-500 transition" />
            </div>
            <div>
              <p className="text-2xl font-bold text-slate-900 dark:text-white">{stat.value}</p>
              <p className="text-sm text-slate-500 dark:text-slate-400 mt-0.5">{stat.name}</p>
            </div>
          </a>
        ))}
      </div>

      {/* Quick Actions */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white dark:bg-slate-800 rounded-xl p-6 shadow-sm border border-slate-200 dark:border-slate-700">
          <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-4">Quick Actions</h3>
          <div className="grid grid-cols-2 gap-3">
            <a href="/dashboard/hrm" className="flex items-center gap-2 px-4 py-3 bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400 rounded-lg hover:bg-blue-100 dark:hover:bg-blue-900/30 transition">
              <Users className="w-5 h-5" /> Add Employee
            </a>
            <a href="/dashboard/inventory" className="flex items-center gap-2 px-4 py-3 bg-green-50 dark:bg-green-900/20 text-green-600 dark:text-green-400 rounded-lg hover:bg-green-100 dark:hover:bg-green-900/30 transition">
              <Package className="w-5 h-5" /> Add Item
            </a>
            <a href="/dashboard/purchasing" className="flex items-center gap-2 px-4 py-3 bg-orange-50 dark:bg-orange-900/20 text-orange-600 dark:text-orange-400 rounded-lg hover:bg-orange-100 dark:hover:bg-orange-900/30 transition">
              <ShoppingCart className="w-5 h-5" /> New PO
            </a>
            <a href="/dashboard/accounting" className="flex items-center gap-2 px-4 py-3 bg-purple-50 dark:bg-purple-900/20 text-purple-600 dark:text-purple-400 rounded-lg hover:bg-purple-100 dark:hover:bg-purple-900/30 transition">
              <DollarSign className="w-5 h-5" /> Accounts
            </a>
          </div>
        </div>

        {/* Recent Activity */}
        <div className="bg-white dark:bg-slate-800 rounded-xl p-6 shadow-sm border border-slate-200 dark:border-slate-700">
          <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-4">Module Overview</h3>
          <div className="space-y-3">
            {[
              { label: 'HRM', count: stats?.totalEmployees ?? '-', color: 'bg-blue-500' },
              { label: 'Inventory', count: stats?.totalInventoryItems ?? '-', color: 'bg-green-500' },
              { label: 'Purchasing', count: stats?.totalPurchaseOrders ?? '-', color: 'bg-orange-500' },
              { label: 'Projects', count: stats?.totalProjects ?? '-', color: 'bg-cyan-500' },
              { label: 'Accounting', count: stats?.totalAccounts ?? '-', color: 'bg-purple-500' },
            ].map((item) => (
              <div key={item.label} className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <div className={`w-2 h-2 rounded-full ${item.color}`} />
                  <span className="text-sm text-slate-600 dark:text-slate-300">{item.label}</span>
                </div>
                <span className="text-sm font-semibold text-slate-900 dark:text-white">{item.count}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
