'use client';

import { useEffect } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import Link from 'next/link';
import { useAuthStore } from '@/lib/store';
import {
  LayoutDashboard,
  Users,
  Package,
  ShoppingCart,
  DollarSign,
  Briefcase,
  Settings,
  Shield,
  Key,
  Building2,
  LogOut,
  ChevronLeft,
  Menu,
} from 'lucide-react';
import { useState } from 'react';

const navigation = [
  { name: 'Dashboard', href: '/dashboard', icon: LayoutDashboard },
  { name: 'HRM', href: '/dashboard/hrm', icon: Users },
  { name: 'Inventory', href: '/dashboard/inventory', icon: Package },
  { name: 'Purchasing', href: '/dashboard/purchasing', icon: ShoppingCart },
  { name: 'Accounting', href: '/dashboard/accounting', icon: DollarSign },
  { name: 'Projects', href: '/dashboard/projects', icon: Briefcase },
  { name: 'Modules', href: '/dashboard/modules', icon: Shield },
  { name: 'Roles', href: '/dashboard/roles', icon: Key },
  { name: 'Organizations', href: '/dashboard/organizations', icon: Building2 },
  { name: 'Settings', href: '/dashboard/settings', icon: Settings },
];

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const { user, isAuthenticated, logout } = useAuthStore();
  const [sidebarOpen, setSidebarOpen] = useState(true);

  useEffect(() => {
    if (!isAuthenticated) {
      router.push('/login');
    }
  }, [isAuthenticated, router]);

  const handleLogout = () => {
    logout();
    router.push('/login');
  };

  if (!isAuthenticated) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-100 dark:bg-slate-900">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-100 dark:bg-slate-900">
      {/* Sidebar */}
      <aside
        className={`fixed top-0 left-0 z-40 h-screen bg-slate-800 transition-all duration-300 ${
          sidebarOpen ? 'w-64' : 'w-20'
        }`}
      >
        <div className="flex flex-col h-full">
          {/* Logo */}
          <div className="flex items-center h-16 px-4 bg-slate-900">
            <div className="flex items-center gap-3">
              <div className="w-8 h-8 rounded-lg bg-blue-600 flex items-center justify-center">
                <span className="text-white font-bold text-sm">N</span>
              </div>
              {sidebarOpen && <span className="text-white font-bold">NEXTERP</span>}
            </div>
            <button
              onClick={() => setSidebarOpen(!sidebarOpen)}
              className="ml-auto text-slate-400 hover:text-white"
            >
              {sidebarOpen ? <ChevronLeft className="w-5 h-5" /> : <Menu className="w-5 h-5" />}
            </button>
          </div>

          {/* Navigation */}
          <nav className="flex-1 py-4 px-3 space-y-1 overflow-y-auto">
            {navigation.map((item) => {
              const isActive = pathname === item.href;
              return (
                <Link
                  key={item.name}
                  href={item.href}
                  className={`flex items-center gap-3 px-3 py-2.5 rounded-lg transition ${
                    isActive
                      ? 'bg-blue-600 text-white'
                      : 'text-slate-300 hover:bg-slate-700 hover:text-white'
                  }`}
                >
                  <item.icon className="w-5 h-5 flex-shrink-0" />
                  {sidebarOpen && <span>{item.name}</span>}
                </Link>
              );
            })}
          </nav>

          {/* User */}
          <div className="p-4 border-t border-slate-700">
            <div className={`flex items-center ${sidebarOpen ? 'gap-3' : 'justify-center'}`}>
              <div className="w-10 h-10 rounded-full bg-slate-600 flex items-center justify-center">
                <span className="text-white font-medium">
                  {user?.firstName?.charAt(0) || 'U'}
                </span>
              </div>
              {sidebarOpen && (
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-white truncate">{user?.fullName}</p>
                  <p className="text-xs text-slate-400 truncate">{user?.email}</p>
                </div>
              )}
            </div>
            <button
              onClick={handleLogout}
              className={`flex items-center gap-3 w-full mt-4 px-3 py-2 text-slate-300 hover:bg-slate-700 hover:text-white rounded-lg transition ${
                sidebarOpen ? '' : 'justify-center'
              }`}
            >
              <LogOut className="w-5 h-5" />
              {sidebarOpen && <span>Logout</span>}
            </button>
          </div>
        </div>
      </aside>

      {/* Main content */}
      <div className={`transition-all duration-300 ${sidebarOpen ? 'ml-64' : 'ml-20'}`}>
        {/* Header */}
        <header className="sticky top-0 z-30 h-16 bg-white dark:bg-slate-800 border-b border-slate-200 dark:border-slate-700">
          <div className="flex items-center justify-between h-full px-6">
            <h1 className="text-xl font-semibold text-slate-900 dark:text-white">
              {navigation.find((n) => n.href === pathname)?.name || 'Dashboard'}
            </h1>
            <div className="flex items-center gap-4">
              <span className="text-sm text-slate-500 dark:text-slate-400">
                {new Date().toLocaleDateString('en-US', {
                  weekday: 'long',
                  year: 'numeric',
                  month: 'long',
                  day: 'numeric',
                })}
              </span>
            </div>
          </div>
        </header>

        {/* Page content */}
        <main className="p-6">{children}</main>
      </div>
    </div>
  );
}
