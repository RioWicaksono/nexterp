'use client';

import { useAuthStore } from '@/lib/store';
import {
  Users,
  Package,
  ShoppingCart,
  DollarSign,
  TrendingUp,
  ArrowUpRight,
  ArrowDownRight,
} from 'lucide-react';

const stats = [
  {
    name: 'Total Employees',
    value: '156',
    change: '+12%',
    changeType: 'increase',
    icon: Users,
    color: 'blue',
  },
  {
    name: 'Inventory Items',
    value: '2,847',
    change: '+8%',
    changeType: 'increase',
    icon: Package,
    color: 'green',
  },
  {
    name: 'Purchase Orders',
    value: '342',
    change: '-3%',
    changeType: 'decrease',
    icon: ShoppingCart,
    color: 'orange',
  },
  {
    name: 'Revenue',
    value: '$124,500',
    change: '+24%',
    changeType: 'increase',
    icon: DollarSign,
    color: 'purple',
  },
];

const colorClasses = {
  blue: 'bg-blue-500',
  green: 'bg-green-500',
  orange: 'bg-orange-500',
  purple: 'bg-purple-500',
};

export default function DashboardPage() {
  const { user } = useAuthStore();

  return (
    <div className="space-y-6">
      {/* Welcome */}
      <div>
        <h2 className="text-2xl font-bold text-slate-900 dark:text-white">
          Welcome back, {user?.firstName}!
        </h2>
        <p className="text-slate-500 dark:text-slate-400">
          Here&apos;s what&apos;s happening with your enterprise today.
        </p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {stats.map((stat) => (
          <div
            key={stat.name}
            className="bg-white dark:bg-slate-800 rounded-xl p-6 shadow-sm border border-slate-200 dark:border-slate-700"
          >
            <div className="flex items-center justify-between">
              <div className={`p-3 rounded-lg ${colorClasses[stat.color as keyof typeof colorClasses]}`}>
                <stat.icon className="w-6 h-6 text-white" />
              </div>
              <div
                className={`flex items-center gap-1 text-sm font-medium ${
                  stat.changeType === 'increase' ? 'text-green-600' : 'text-red-600'
                }`}
              >
                {stat.changeType === 'increase' ? (
                  <ArrowUpRight className="w-4 h-4" />
                ) : (
                  <ArrowDownRight className="w-4 h-4" />
                )}
                {stat.change}
              </div>
            </div>
            <div className="mt-4">
              <p className="text-3xl font-bold text-slate-900 dark:text-white">{stat.value}</p>
              <p className="text-sm text-slate-500 dark:text-slate-400">{stat.name}</p>
            </div>
          </div>
        ))}
      </div>

      {/* Quick Actions */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white dark:bg-slate-800 rounded-xl p-6 shadow-sm border border-slate-200 dark:border-slate-700">
          <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-4">
            Quick Actions
          </h3>
          <div className="grid grid-cols-2 gap-3">
            <button className="flex items-center gap-2 px-4 py-3 bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400 rounded-lg hover:bg-blue-100 dark:hover:bg-blue-900/30 transition">
              <Users className="w-5 h-5" />
              Add Employee
            </button>
            <button className="flex items-center gap-2 px-4 py-3 bg-green-50 dark:bg-green-900/20 text-green-600 dark:text-green-400 rounded-lg hover:bg-green-100 dark:hover:bg-green-900/30 transition">
              <Package className="w-5 h-5" />
              Add Inventory
            </button>
            <button className="flex items-center gap-2 px-4 py-3 bg-orange-50 dark:bg-orange-900/20 text-orange-600 dark:text-orange-400 rounded-lg hover:bg-orange-100 dark:hover:bg-orange-900/30 transition">
              <ShoppingCart className="w-5 h-5" />
              New PO
            </button>
            <button className="flex items-center gap-2 px-4 py-3 bg-purple-50 dark:bg-purple-900/20 text-purple-600 dark:text-purple-400 rounded-lg hover:bg-purple-100 dark:hover:bg-purple-900/30 transition">
              <DollarSign className="w-5 h-5" />
              Create Invoice
            </button>
          </div>
        </div>

        <div className="bg-white dark:bg-slate-800 rounded-xl p-6 shadow-sm border border-slate-200 dark:border-slate-700">
          <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-4">
            Recent Activity
          </h3>
          <div className="space-y-4">
            {[
              { text: 'New employee added: John Doe', time: '5 minutes ago' },
              { text: 'Purchase order #PO-2024-001 approved', time: '1 hour ago' },
              { text: 'Invoice #INV-2024-015 created', time: '3 hours ago' },
              { text: 'Inventory stock updated for SKU-001', time: '5 hours ago' },
            ].map((activity, i) => (
              <div key={i} className="flex items-start gap-3">
                <div className="w-2 h-2 mt-2 rounded-full bg-blue-500"></div>
                <div>
                  <p className="text-sm text-slate-900 dark:text-white">{activity.text}</p>
                  <p className="text-xs text-slate-500 dark:text-slate-400">{activity.time}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
