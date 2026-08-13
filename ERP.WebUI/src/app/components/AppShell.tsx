"use client";

import { useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  LayoutDashboard,
  Package,
  Users,
  ShoppingCart,
  FileText,
  Settings,
  Menu,
  X,
  Bell,
  Search,
  TrendingUp,
  Building,
  CheckCircle,
  BarChart3,
  Briefcase,
  DollarSign,
} from "lucide-react";

const navItems = [
  { href: "/dashboard", icon: LayoutDashboard, label: "Dashboard" },
  { href: "/inventory", icon: Package, label: "Inventory" },
  { href: "/sales", icon: ShoppingCart, label: "Sales" },
  { href: "/purchasing", icon: FileText, label: "Purchasing" },
  { href: "/hr", icon: Users, label: "HR" },
  { href: "/settings", icon: Settings, label: "Settings" },
];

interface AppShellProps {
  children: React.ReactNode;
}

export function AppShell({ children }: AppShellProps) {
  const pathname = usePathname();
  const [sidebarOpen, setSidebarOpen] = useState(false);

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-900 flex">
      {/* Desktop Sidebar */}
      <aside className="hidden lg:flex lg:flex-col lg:w-64 bg-white dark:bg-slate-800 border-r border-slate-200 dark:border-slate-700 fixed h-full">
        <SidebarContent pathname={pathname} />
      </aside>

      {/* Mobile Sidebar Overlay */}
      {sidebarOpen && (
        <div
          className="lg:hidden fixed inset-0 bg-black/50 z-40 backdrop-blur-sm"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Mobile Sidebar */}
      <aside
        className={`
          lg:hidden fixed inset-y-0 left-0 z-50 w-72 bg-white dark:bg-slate-800 border-r border-slate-200 dark:border-slate-700
          transform transition-transform duration-300 ease-in-out
          ${sidebarOpen ? "translate-x-0" : "-translate-x-full"}
        `}
      >
        <SidebarContent pathname={pathname} onNavigate={() => setSidebarOpen(false)} />
      </aside>

      {/* Main Content */}
      <div className="flex-1 lg:ml-64">
        {/* Top Bar */}
        <header className="sticky top-0 z-30 bg-white/80 dark:bg-slate-800/80 backdrop-blur-xl border-b border-slate-200 dark:border-slate-700">
          <div className="flex items-center justify-between px-4 h-16">
            {/* Mobile Menu Button */}
            <button
              onClick={() => setSidebarOpen(true)}
              className="lg:hidden p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors"
              aria-label="Open menu"
            >
              <Menu className="w-6 h-6 text-slate-600 dark:text-slate-300" />
            </button>

            {/* Search */}
            <div className="hidden sm:flex flex-1 max-w-md mx-4">
              <div className="relative w-full">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-400" />
                <input
                  type="search"
                  placeholder="Search anything..."
                  className="w-full pl-10 pr-4 py-2.5 rounded-xl bg-slate-100 dark:bg-slate-700 border-0 text-slate-800 dark:text-white placeholder:text-slate-400 focus:ring-2 focus:ring-blue-500 transition-all"
                />
                <kbd className="absolute right-3 top-1/2 -translate-y-1/2 hidden md:inline-flex px-2 py-1 text-xs font-medium text-slate-400 bg-slate-200 dark:bg-slate-600 dark:text-slate-300 rounded">
                  /
                </kbd>
              </div>
            </div>

            {/* Right Actions */}
            <div className="flex items-center gap-2">
              {/* Notifications */}
              <button className="relative p-2.5 rounded-xl hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors">
                <Bell className="w-5 h-5 text-slate-600 dark:text-slate-300" />
                <span className="absolute top-1.5 right-1.5 w-2.5 h-2.5 bg-red-500 rounded-full border-2 border-white dark:border-slate-800" />
              </button>

              {/* User Avatar */}
              <Link
                href="/settings"
                className="flex items-center gap-3 p-1.5 pr-3 rounded-xl hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors"
              >
                <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-blue-500 to-emerald-500 flex items-center justify-center">
                  <span className="text-white font-semibold text-sm">AD</span>
                </div>
                <span className="hidden sm:block text-sm font-medium text-slate-700 dark:text-slate-200">
                  Admin
                </span>
              </Link>
            </div>
          </div>
        </header>

        {/* Page Content */}
        <main className="p-4 md:p-6 pb-24 lg:pb-6">{children}</main>
      </div>

      {/* Mobile Bottom Navigation */}
      <nav className="lg:hidden fixed bottom-0 left-0 right-0 z-40 bg-white/90 dark:bg-slate-800/90 backdrop-blur-xl border-t border-slate-200 dark:border-slate-700 safe-area-pb">
        <div className="flex items-center justify-around h-16">
          {navItems.map(item => {
            const isActive = pathname === item.href || pathname.startsWith(item.href + "/");
            return (
              <Link
                key={item.href}
                href={item.href}
                className={`
                  flex flex-col items-center justify-center gap-1 flex-1 h-full transition-colors
                  ${isActive
                    ? "text-blue-600 dark:text-blue-400"
                    : "text-slate-400 dark:text-slate-500"
                  }
                `}
              >
                <item.icon className={`w-5 h-5 ${isActive ? "stroke-[2.5]" : ""}`} />
                <span className="text-[10px] font-medium">{item.label}</span>
              </Link>
            );
          })}
        </div>
      </nav>
    </div>
  );
}

function SidebarContent({
  pathname,
  onNavigate,
}: {
  pathname: string;
  onNavigate?: () => void;
}) {
  const [openMenus, setOpenMenus] = useState<string[]>(["modules"]);

  const toggleMenu = (menu: string) => {
    setOpenMenus(prev =>
      prev.includes(menu) ? prev.filter(m => m !== menu) : [...prev, menu]
    );
  };

  return (
    <div className="flex flex-col h-full">
      {/* Logo */}
      <div className="p-6 border-b border-slate-200 dark:border-slate-700">
        <Link href="/" className="flex items-center gap-3" onClick={onNavigate}>
          <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-blue-600 to-emerald-500 flex items-center justify-center shadow-lg shadow-blue-500/25">
            <span className="text-white font-bold text-lg">N</span>
          </div>
          <div>
            <span className="font-bold text-lg text-slate-800 dark:text-white">NEXTERP</span>
            <p className="text-[10px] text-slate-400 -mt-0.5">Enterprise ERP</p>
          </div>
        </Link>
      </div>

      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto p-4 space-y-1">
        <SidebarItem href="/dashboard" icon={LayoutDashboard} label="Dashboard" active={pathname === "/dashboard"} onClick={onNavigate} />

        {/* Modules Menu */}
        <div className="space-y-1">
          <button
            onClick={() => toggleMenu("modules")}
            className="w-full flex items-center justify-between px-3 py-2.5 rounded-xl text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors"
          >
            <span className="text-sm font-medium">Modules</span>
            <svg className={`w-4 h-4 transition-transform ${openMenus.includes("modules") ? "rotate-180" : ""}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
            </svg>
          </button>

          {openMenus.includes("modules") && (
            <div className="pl-3 space-y-0.5">
              <SidebarItem href="/inventory" icon={Package} label="Inventory" active={pathname.startsWith("/inventory")} onClick={onNavigate} indent />
              <SidebarItem href="/accounting" icon={DollarSign} label="Accounting" active={pathname.startsWith("/accounting")} onClick={onNavigate} indent />
              <SidebarItem href="/sales" icon={ShoppingCart} label="Sales" active={pathname.startsWith("/sales")} onClick={onNavigate} indent />
              <SidebarItem href="/purchasing" icon={FileText} label="Purchasing" active={pathname.startsWith("/purchasing")} onClick={onNavigate} indent />
              <SidebarItem href="/hr" icon={Users} label="HRM" active={pathname.startsWith("/hr")} onClick={onNavigate} indent />
              <SidebarItem href="/projects" icon={Briefcase} label="Projects" active={pathname.startsWith("/projects")} onClick={onNavigate} indent />
            </div>
          )}
        </div>

        <SidebarItem href="/analytics" icon={BarChart3} label="Analytics" active={pathname.startsWith("/analytics")} onClick={onNavigate} />
        <SidebarItem href="/assets" icon={Building} label="Assets" active={pathname.startsWith("/assets")} onClick={onNavigate} />
        <SidebarItem href="/quality" icon={CheckCircle} label="Quality" active={pathname.startsWith("/quality")} onClick={onNavigate} />
      </nav>

      {/* Bottom */}
      <div className="p-4 border-t border-slate-200 dark:border-slate-700">
        <SidebarItem href="/settings" icon={Settings} label="Settings" active={pathname.startsWith("/settings")} onClick={onNavigate} />
      </div>
    </div>
  );
}

function SidebarItem({
  href,
  icon: Icon,
  label,
  active,
  onClick,
  indent,
}: {
  href: string;
  icon: React.ElementType;
  label: string;
  active: boolean;
  onClick?: () => void;
  indent?: boolean;
}) {
  return (
    <Link
      href={href}
      onClick={onClick}
      className={`
        flex items-center gap-3 px-3 py-2.5 rounded-xl transition-colors
        ${indent ? "ml-2" : ""}
        ${active
          ? "bg-blue-50 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400"
          : "text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-700 hover:text-slate-900 dark:hover:text-slate-200"
        }
      `}
    >
      <Icon className={`w-5 h-5 ${active ? "stroke-[2.5]" : ""}`} />
      <span className="text-sm font-medium">{label}</span>
    </Link>
  );
}
