'use client';

import { useEffect, useState } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import Link from 'next/link';
import { useAuthStore } from '@/lib/store';
import {
  LayoutDashboard, Users, Package, ShoppingCart, DollarSign, Briefcase,
  Settings, Shield, Key, Building2, LogOut, ChevronLeft, Menu, Layers,
} from 'lucide-react';

const mainNav = [
  { name: 'Dashboard', href: '/dashboard', icon: LayoutDashboard },
];

const modulesNav = [
  { name: 'HRM', href: '/dashboard/hrm', icon: Users },
  { name: 'Inventory', href: '/dashboard/inventory', icon: Package },
  { name: 'Purchasing', href: '/dashboard/purchasing', icon: ShoppingCart },
  { name: 'Accounting', href: '/dashboard/accounting', icon: DollarSign },
  { name: 'Projects', href: '/dashboard/projects', icon: Briefcase },
];

const systemNav = [
  { name: 'Modules', href: '/dashboard/modules', icon: Layers },
  { name: 'Roles', href: '/dashboard/roles', icon: Shield },
  { name: 'Organizations', href: '/dashboard/organizations', icon: Building2 },
  { name: 'Settings', href: '/dashboard/settings', icon: Settings },
];

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const { user, isAuthenticated, logout } = useAuthStore();
  const [collapsed, setCollapsed] = useState(false);

  useEffect(() => {
    if (!isAuthenticated) router.push('/login');
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

  const NavSection = ({ items, label }: { items: typeof mainNav; label?: string }) => (
    <div className="space-y-0.5">
      {label && !collapsed && (
        <div className="px-3 py-2 text-xs font-semibold text-slate-500 uppercase tracking-wider">{label}</div>
      )}
      {items.map((item) => {
        const isActive = pathname === item.href;
        return (
          <Link
            key={item.href}
            href={item.href}
            title={collapsed ? item.name : undefined}
            className={`flex items-center gap-2.5 px-2.5 py-2 rounded-md transition text-sm ${
              isActive
                ? 'bg-blue-600 text-white font-medium'
                : 'text-slate-400 hover:text-white hover:bg-slate-700/50'
            }`}
          >
            <item.icon className="w-4 h-4 flex-shrink-0" />
            {!collapsed && <span>{item.name}</span>}
          </Link>
        );
      })}
    </div>
  );

  return (
    <div className="min-h-screen bg-slate-100 dark:bg-slate-900 flex">
      {/* Compact Sidebar */}
      <aside className={`fixed top-0 left-0 z-40 h-screen bg-slate-800 transition-all duration-200 flex flex-col ${collapsed ? 'w-16' : 'w-56'}`}>
        {/* Logo */}
        <div className="flex items-center h-12 px-2 bg-slate-900/50 border-b border-slate-700">
          <div className="w-7 h-7 rounded-md bg-blue-600 flex items-center justify-center flex-shrink-0">
            <span className="text-white font-bold text-xs">N</span>
          </div>
          {!collapsed && <span className="ml-2 text-white font-bold text-sm tracking-wide">NEXTERP</span>}
          <button onClick={() => setCollapsed(!collapsed)} className="ml-auto text-slate-400 hover:text-white p-1">
            {collapsed ? <Menu className="w-4 h-4" /> : <ChevronLeft className="w-4 h-4" />}
          </button>
        </div>

        {/* Nav */}
        <nav className="flex-1 py-2 px-1.5 space-y-4 overflow-y-auto">
          <NavSection items={mainNav} />
          <div className="border-t border-slate-700/50 pt-2">
            <NavSection items={modulesNav} />
          </div>
          <div className="border-t border-slate-700/50 pt-2">
            <NavSection items={systemNav} />
          </div>
        </nav>

        {/* User */}
        <div className="p-2 border-t border-slate-700">
          <div className={`flex items-center ${collapsed ? 'justify-center' : 'gap-2'}`}>
            <div className="w-8 h-8 rounded-full bg-slate-600 flex items-center justify-center flex-shrink-0">
              <span className="text-white text-xs font-medium">{user?.firstName?.charAt(0) || 'U'}</span>
            </div>
            {!collapsed && (
              <div className="flex-1 min-w-0">
                <p className="text-xs font-medium text-white truncate">{user?.fullName}</p>
                <p className="text-xs text-slate-400 truncate">{user?.roles?.[0]}</p>
              </div>
            )}
          </div>
          <button onClick={handleLogout} title="Logout" className={`flex items-center gap-2 w-full mt-1 px-2 py-1.5 text-slate-400 hover:text-white hover:bg-slate-700/50 rounded-md transition text-xs ${collapsed ? 'justify-center' : ''}`}>
            <LogOut className="w-3.5 h-3.5 flex-shrink-0" />
            {!collapsed && <span>Logout</span>}
          </button>
        </div>
      </aside>

      {/* Main Content */}
      <div className={`flex-1 transition-all duration-200 ${collapsed ? 'ml-16' : 'ml-56'}`}>
        <header className="sticky top-0 z-30 h-12 bg-white/80 dark:bg-slate-800/80 backdrop-blur border-b border-slate-200 dark:border-slate-700">
          <div className="flex items-center justify-between h-full px-4">
            <h1 className="text-sm font-medium text-slate-900 dark:text-white">
              {[...mainNav, ...modulesNav, ...systemNav].find(n => n.href === pathname)?.name || 'Dashboard'}
            </h1>
            <div className="flex items-center gap-4 text-xs text-slate-500">
              <span>{new Date().toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}</span>
            </div>
          </div>
        </header>
        <main className="p-4">{children}</main>
      </div>
    </div>
  );
}
