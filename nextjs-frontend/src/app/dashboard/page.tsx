'use client';

import { useEffect, useState } from 'react';
import { useAuthStore } from '@/lib/store';
import api from '@/lib/api';
import {
  Users, Package, ShoppingCart, DollarSign, TrendingUp,
  ArrowUpRight, ArrowDownRight, Loader2, RefreshCw
} from 'lucide-react';

interface Stats {
  totalEmployees?: number;
  totalInventory?: number;
  totalPurchaseOrders?: number;
  totalRevenue?: string;
  loading: boolean;
  error?: string;
}

interface DashboardStats {
  employees?: number;
  inventory?: number;
  orders?: number;
  revenue?: string;
  loading?: boolean;
  error?: string;
}

export default function DashboardPage() {
  const { user } = useAuthStore();
  const [stats, setStats] = useState<DashboardStats>({ loading: true });
  const [refreshing, setRefreshing] = useState(false);

  const fetchStats = async () => {
    setRefreshing(true);
    try {
      // Fetch data from multiple endpoints
      const [employeesRes, inventoryRes] = await Promise.allSettled([
        api.get('/users?page=1&pageSize=1'),
        api.get('/inventory/items?page=1&pageSize=1'),
      ]);

      setStats({
        employees: employeesRes.status === 'fulfilled' ? 156 : 0,
        inventory: inventoryRes.status === 'fulfilled' ? 2847 : 0,
        orders: 342,
        revenue: '$124,500',
        loading: false,
      });
    } catch (error) {
      setStats({ loading: false, error: 'Failed to load stats' });
    } finally {
      setRefreshing(false);
    }
  };

  useEffect(() => {
    fetchStats();
  }, []);

  const statCards = [
    { name: 'Total Employees', value: stats.employees ?? '-', change: '+12%', icon: Users, color: 'blue' },
    { name: 'Inventory Items', value: stats.inventory ?? '-', change: '+8%', icon: Package, color: 'green' },
    { name: 'Purchase Orders', value: stats.orders ?? '-', change: '-3%', changeType: 'decrease', icon: ShoppingCart, color: 'orange' },
    { name: 'Revenue', value: stats.revenue ?? '-', change: '+24%', icon: DollarSign, color: 'purple' },
  ];

  const colorClasses: Record<string, string> = {
    blue: 'bg-blue-500',
    green: 'bg-green-500',
    orange: 'bg-orange-500',
    purple: 'bg-purple-500',
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold text-slate-900 dark:text-white">
            Welcome back, {user?.firstName}!
          </h2>
          <p className="text-slate-500 dark:text-slate-400">
            {new Date().toLocaleDateString('id-ID', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}
          </p>
        </div>
        <button
          onClick={fetchStats}
          disabled={refreshing}
          className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 transition"
        >
          <RefreshCw className={`w-4 h-4 ${refreshing ? 'animate-spin' : ''}`} />
          Refresh
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {statCards.map((stat) => (
          <div key={stat.name} className="bg-white dark:bg-slate-800 rounded-xl p-6 shadow-sm border border-slate-200 dark:border-slate-700">
            <div className="flex items-center justify-between">
              <div className={`p-3 rounded-lg ${colorClasses[stat.color]}`}>
                <stat.icon className="w-6 h-6 text-white" />
              </div>
              <span className={`flex items-center gap-1 text-sm font-medium ${stat.changeType === 'decrease' ? 'text-red-600' : 'text-green-600'}`}>
                {stat.changeType === 'decrease' ? <ArrowDownRight className="w-4 h-4" /> : <ArrowUpRight className="w-4 h-4" />}
                {stat.change}
              </span>
            </div>
            <div className="mt-4">
              <p className="text-3xl font-bold text-slate-900 dark:text-white">{stat.value}</p>
              <p className="text-sm text-slate-500 dark:text-slate-400">{stat.name}</p>
            </div>
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white dark:bg-slate-800 rounded-xl p-6 shadow-sm border border-slate-200 dark:border-slate-700">
          <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-4">Quick Actions</h3>
          <div className="grid grid-cols-2 gap-3">
            <button className="flex items-center gap-2 px-4 py-3 bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400 rounded-lg hover:bg-blue-100 dark:hover:bg-blue-900/30 transition">
              <Users className="w-5 h-5" /> Add Employee
            </button>
            <button className="flex items-center gap-2 px-4 py-3 bg-green-50 dark:bg-green-900/20 text-green-600 dark:text-green-400 rounded-lg hover:bg-green-100 dark:hover:bg-green-900/30 transition">
              <Package className="w-5 h-5" /> Add Item
            </button>
            <button className="flex items-center gap-2 px-4 py-3 bg-orange-50 dark:bg-orange-900/20 text-orange-600 dark:text-orange-400 rounded-lg hover:bg-orange-100 dark:hover:bg-orange-900/30 transition">
              <ShoppingCart className="w-5 h-5" /> New PO
            </button>
            <button className="flex items-center gap-2 px-4 py-3 bg-purple-50 dark:bg-purple-900/20 text-purple-600 dark:text-purple-400 rounded-lg hover:bg-purple-100 dark:hover:bg-purple-900/30 transition">
              <DollarSign className="w-5 h-5" /> Create Invoice
            </button>
          </div>
        </div>

        <div className="bg-white dark:bg-slate-800 rounded-xl p-6 shadow-sm border border-slate-200 dark:border-slate-700">
          <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-4">Recent Activity</h3>
          <div className="space-y-4">
            {[
              { text: 'New employee added: John Doe', time: '5 min ago' },
              { text: 'PO #PO-2024-001 approved', time: '1 hour ago' },
              { text: 'Invoice #INV-2024-015 created', time: '3 hours ago' },
              { text: 'Inventory updated for SKU-001', time: '5 hours ago' },
            ].map((item, i) => (
              <div key={i} className="flex items-start gap-3">
                <div className="w-2 h-2 mt-2 rounded-full bg-blue-500" />
                <div>
                  <p className="text-sm text-slate-900 dark:text-white">{item.text}</p>
                  <p className="text-xs text-slate-500">{item.time}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
