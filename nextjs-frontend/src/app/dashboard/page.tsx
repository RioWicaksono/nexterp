'use client';

import { useEffect, useState } from 'react';
import { useAuthStore } from '@/lib/store';
import Link from 'next/link';
import {
  Users, Package, ShoppingCart, DollarSign, Building2,
  ArrowUpRight, RefreshCw, Activity
} from 'lucide-react';

interface Stats { employees: string; inventory: string; orders: string; suppliers: string; projects: string; accounts: string; }

export default function DashboardPage() {
  const { user } = useAuthStore();
  const [stats, setStats] = useState<Stats>({ employees: '-', inventory: '-', orders: '-', suppliers: '-', projects: '-', accounts: '-' });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch('/api/dashboard/stats').then(r => r.json()).then(d => {
      if (d.success) setStats({
        employees: String(d.data?.totalEmployees ?? 0),
        inventory: String(d.data?.totalInventoryItems ?? 0),
        orders: String(d.data?.totalPurchaseOrders ?? 0),
        suppliers: String(d.data?.totalSuppliers ?? 0),
        projects: String(d.data?.totalProjects ?? 0),
        accounts: String(d.data?.totalAccounts ?? 0),
      });
    }).catch(() => {}).finally(() => setLoading(false));
  }, []);

  const cards = [
    { label: 'Total Employees', value: stats.employees, icon: Users, bgClass: 'bg-blue-500', href: '/dashboard/hrm' },
    { label: 'Inventory Items', value: stats.inventory, icon: Package, bgClass: 'bg-green-500', href: '/dashboard/inventory' },
    { label: 'Purchase Orders', value: stats.orders, icon: ShoppingCart, bgClass: 'bg-orange-500', href: '/dashboard/purchasing' },
    { label: 'Total Suppliers', value: stats.suppliers, icon: Building2, bgClass: 'bg-purple-500', href: '/dashboard/purchasing' },
    { label: 'Active Projects', value: stats.projects, icon: Activity, bgClass: 'bg-cyan-500', href: '/dashboard/projects' },
    { label: 'Chart of Accounts', value: stats.accounts, icon: DollarSign, bgClass: 'bg-emerald-500', href: '/dashboard/accounting' },
  ];

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white">
            Welcome back, {user?.firstName || 'User'}!
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            {new Date().toLocaleDateString('id-ID', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}
          </p>
        </div>
        <button
          onClick={() => window.location.reload()}
          className="flex items-center gap-2 px-3 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 text-sm"
        >
          <RefreshCw className="w-4 h-4" /> Refresh
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {cards.map(card => (
          <Link
            key={card.label}
            href={card.href}
            className={`bg-white dark:bg-slate-800 rounded-xl p-5 border border-slate-200 dark:border-slate-700 hover:shadow-md hover:border-blue-300 dark:hover:border-blue-600 transition group cursor-pointer`}
          >
            <div className="flex items-center justify-between mb-3">
              <div className={`p-2.5 rounded-lg ${card.bgClass}`}>
                <card.icon className="w-5 h-5 text-white" />
              </div>
              <ArrowUpRight className="w-4 h-4 text-slate-400 group-hover:text-blue-500" />
            </div>
            <p className="text-2xl font-bold text-slate-900 dark:text-white">{card.value}</p>
            <p className="text-sm text-slate-500 mt-0.5">{card.label}</p>
          </Link>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <div className="bg-white dark:bg-slate-800 rounded-xl p-6 border border-slate-200 dark:border-slate-700">
          <h3 className="font-semibold text-slate-900 dark:text-white mb-4">Quick Actions</h3>
          <div className="grid grid-cols-2 gap-3">
            <Link href="/dashboard/hrm" className="flex items-center gap-2 px-4 py-3 bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400 rounded-lg hover:bg-blue-100 dark:hover:bg-blue-900/30 transition text-sm">
              <Users className="w-5 h-5" /> Add Employee
            </Link>
            <Link href="/dashboard/inventory" className="flex items-center gap-2 px-4 py-3 bg-green-50 dark:bg-green-900/20 text-green-600 dark:text-green-400 rounded-lg hover:bg-green-100 dark:hover:bg-green-900/30 transition text-sm">
              <Package className="w-5 h-5" /> Add Item
            </Link>
            <Link href="/dashboard/purchasing" className="flex items-center gap-2 px-4 py-3 bg-orange-50 dark:bg-orange-900/20 text-orange-600 dark:text-orange-400 rounded-lg hover:bg-orange-100 dark:hover:bg-orange-900/30 transition text-sm">
              <ShoppingCart className="w-5 h-5" /> New PO
            </Link>
            <Link href="/dashboard/accounting" className="flex items-center gap-2 px-4 py-3 bg-purple-50 dark:bg-purple-900/20 text-purple-600 dark:text-purple-400 rounded-lg hover:bg-purple-100 dark:hover:bg-purple-900/30 transition text-sm">
              <DollarSign className="w-5 h-5" /> Create Journal
            </Link>
          </div>
        </div>
        <div className="bg-white dark:bg-slate-800 rounded-xl p-6 border border-slate-200 dark:border-slate-700">
          <h3 className="font-semibold text-slate-900 dark:text-white mb-4">Quick Actions</h3>
          <div className="space-y-3 text-sm text-slate-500">
            <p>Dashboard ready for use</p>
          </div>
        </div>
      </div>
    </div>
  );
}
