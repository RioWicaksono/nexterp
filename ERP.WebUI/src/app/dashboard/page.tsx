"use client";

import { useState } from "react";
import { AppShell } from "../components/AppShell";
import { ErrorBoundary } from "../components/ErrorBoundary";
import { DashboardSkeleton } from "../components/skeletons";
import { useI18n } from "../providers/I18nProvider";
import { useToast } from "../providers/ToastProvider";
import {
  SalesLineChart,
  RevenuePieChart,
  WeeklyComparisonChart,
  TrendLineChart,
} from "../components/charts";
import {
  TrendingUp,
  TrendingDown,
  Package,
  Users,
  ShoppingCart,
  DollarSign,
  ArrowUpRight,
  ArrowDownRight,
  Bell,
  Globe,
  Download,
  Filter,
} from "lucide-react";

// Mock data
const stats = [
  {
    title: "Revenue",
    value: "$124,563",
    change: "+12.5%",
    trend: "up",
    icon: DollarSign,
    color: "emerald",
    bgColor: "bg-emerald-100 dark:bg-emerald-900/30",
    iconColor: "text-emerald-600 dark:text-emerald-400",
  },
  {
    title: "Orders",
    value: "2,847",
    change: "+8.2%",
    trend: "up",
    icon: ShoppingCart,
    color: "blue",
    bgColor: "bg-blue-100 dark:bg-blue-900/30",
    iconColor: "text-blue-600 dark:text-blue-400",
  },
  {
    title: "Products",
    value: "1,284",
    change: "-2.1%",
    trend: "down",
    icon: Package,
    color: "purple",
    bgColor: "bg-purple-100 dark:bg-purple-900/30",
    iconColor: "text-purple-600 dark:text-purple-400",
  },
  {
    title: "Customers",
    value: "5,621",
    change: "+15.3%",
    trend: "up",
    icon: Users,
    color: "amber",
    bgColor: "bg-amber-100 dark:bg-amber-900/30",
    iconColor: "text-amber-600 dark:text-amber-400",
  },
];

const recentOrders = [
  { id: "ORD-001", customer: "PT Maju Bersama", total: "$12,450", status: "Completed", date: "2026-07-08" },
  { id: "ORD-002", customer: "CV Teknologi Indonesia", total: "$8,320", status: "Processing", date: "2026-07-08" },
  { id: "ORD-003", customer: "Toko Elektronik Jaya", total: "$5,890", status: "Pending", date: "2026-07-07" },
  { id: "ORD-004", customer: "PT Karya Digital", total: "$23,100", status: "Completed", date: "2026-07-07" },
  { id: "ORD-005", customer: "CV Nusantara Tech", total: "$7,650", status: "Cancelled", date: "2026-07-06" },
];

const topProducts = [
  { name: "Laptop ASUS ROG", sold: 145, revenue: "$86,500" },
  { name: "Monitor LG 27\"", sold: 89, revenue: "$24,500" },
  { name: "Keyboard Mechanical", sold: 234, revenue: "$18,720" },
  { name: "Mouse Wireless", sold: 456, revenue: "$13,680" },
  { name: "Headset Gaming", sold: 178, revenue: "$15,520" },
];

export default function DashboardPage() {
  const { t, locale, setLocale } = useI18n();
  const { success, warning, info } = useToast();
  const [isLoading, setIsLoading] = useState(false);
  const [selectedPeriod, setSelectedPeriod] = useState("7d");

  const toggleLanguage = () => {
    const newLocale = locale === "en" ? "id" : "en";
    setLocale(newLocale);
    success("Language Changed", `Switched to ${newLocale === "en" ? "English" : "Bahasa Indonesia"}`);
  };

  const showNotification = () => {
    info("New Order", "You have 3 new orders pending approval");
  };

  const handleExport = () => {
    success("Export Started", "Your data will be downloaded shortly.");
  };

  if (isLoading) {
    return (
      <AppShell>
        <DashboardSkeleton />
      </AppShell>
    );
  }

  return (
    <ErrorBoundary>
      <AppShell>
        <div className="space-y-6">
          {/* Header */}
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
              <h1 className="text-2xl font-bold text-slate-900 dark:text-white">
                {t("dashboard.welcome")}, Admin
              </h1>
              <p className="text-slate-500 dark:text-slate-400 text-sm mt-1">
                Here&apos;s what&apos;s happening with your business today.
              </p>
            </div>
            <div className="flex items-center gap-2 flex-wrap">
              {/* Period Selector */}
              <select
                value={selectedPeriod}
                onChange={(e) => setSelectedPeriod(e.target.value)}
                className="px-3 py-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-slate-600 dark:text-slate-300 text-sm"
              >
                <option value="7d">Last 7 days</option>
                <option value="30d">Last 30 days</option>
                <option value="90d">Last 90 days</option>
                <option value="1y">Last year</option>
              </select>

              <button
                onClick={toggleLanguage}
                className="flex items-center gap-2 px-4 py-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
              >
                <Globe className="w-4 h-4 text-slate-500" />
                <span className="text-sm font-medium text-slate-600 dark:text-slate-300 uppercase">
                  {locale}
                </span>
              </button>

              <button
                onClick={handleExport}
                className="flex items-center gap-2 px-4 py-2 rounded-xl bg-blue-600 hover:bg-blue-700 text-white transition-colors"
              >
                <Download className="w-4 h-4" />
                <span className="text-sm font-medium">Export</span>
              </button>

              <button
                onClick={showNotification}
                className="relative p-2.5 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
              >
                <Bell className="w-5 h-5 text-slate-600 dark:text-slate-300" />
                <span className="absolute top-2 right-2 w-2 h-2 bg-red-500 rounded-full" />
              </button>
            </div>
          </div>

          {/* Stats Grid */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {stats.map((stat) => (
              <div
                key={stat.title}
                className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6 hover:shadow-lg transition-shadow"
              >
                <div className="flex items-center justify-between mb-4">
                  <div className={`w-14 h-14 rounded-xl ${stat.bgColor} flex items-center justify-center`}>
                    <stat.icon className={`w-7 h-7 ${stat.iconColor}`} />
                  </div>
                  <span
                    className={`flex items-center gap-1 text-sm font-semibold ${
                      stat.trend === "up"
                        ? "text-emerald-600 dark:text-emerald-400"
                        : "text-red-600 dark:text-red-400"
                    }`}
                  >
                    {stat.trend === "up" ? (
                      <ArrowUpRight className="w-4 h-4" />
                    ) : (
                      <ArrowDownRight className="w-4 h-4" />
                    )}
                    {stat.change}
                  </span>
                </div>
                <p className="text-sm text-slate-500 dark:text-slate-400 mb-1">{stat.title}</p>
                <p className="text-2xl font-bold text-slate-900 dark:text-white">{stat.value}</p>
              </div>
            ))}
          </div>

          {/* Charts Row */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <SalesLineChart />
            <RevenuePieChart />
          </div>

          {/* Second Charts Row */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <WeeklyComparisonChart />
            <TrendLineChart />
          </div>

          {/* Tables Row */}
          <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
            {/* Recent Orders */}
            <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 overflow-hidden">
              <div className="px-6 py-4 border-b border-slate-200 dark:border-slate-700 flex items-center justify-between">
                <h2 className="text-lg font-semibold text-slate-900 dark:text-white">
                  {t("dashboard.recentOrders")}
                </h2>
                <button className="text-sm text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 font-medium">
                  View all →
                </button>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full min-w-[500px]">
                  <thead>
                    <tr className="bg-slate-50 dark:bg-slate-800/50">
                      <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                        Order
                      </th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                        Customer
                      </th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                        Status
                      </th>
                      <th className="px-4 py-3 text-right text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                        Total
                      </th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100 dark:divide-slate-700/50">
                    {recentOrders.map(order => (
                      <tr key={order.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/50 transition-colors">
                        <td className="px-4 py-3.5 whitespace-nowrap">
                          <span className="text-sm font-semibold text-slate-900 dark:text-white">{order.id}</span>
                        </td>
                        <td className="px-4 py-3.5 whitespace-nowrap">
                          <span className="text-sm text-slate-600 dark:text-slate-400">{order.customer}</span>
                        </td>
                        <td className="px-4 py-3.5 whitespace-nowrap">
                          <span
                            className={`badge ${
                              order.status === "Completed"
                                ? "badge-success"
                                : order.status === "Processing"
                                ? "badge-info"
                                : order.status === "Pending"
                                ? "badge-warning"
                                : "badge-danger"
                            }`}
                          >
                            {order.status}
                          </span>
                        </td>
                        <td className="px-4 py-3.5 whitespace-nowrap text-right">
                          <span className="text-sm font-semibold text-slate-900 dark:text-white">{order.total}</span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Top Products */}
            <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 overflow-hidden">
              <div className="px-6 py-4 border-b border-slate-200 dark:border-slate-700 flex items-center justify-between">
                <h2 className="text-lg font-semibold text-slate-900 dark:text-white">
                  {t("dashboard.topProducts")}
                </h2>
                <button className="text-sm text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 font-medium">
                  View all →
                </button>
              </div>
              <div className="divide-y divide-slate-100 dark:divide-slate-700/50">
                {topProducts.map((product, index) => (
                  <div
                    key={product.name}
                    className="px-6 py-3.5 flex items-center justify-between hover:bg-slate-50 dark:hover:bg-slate-800/50 transition-colors"
                  >
                    <div className="flex items-center gap-4">
                      <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-blue-500 to-blue-600 dark:from-blue-600 dark:to-blue-700 flex items-center justify-center">
                        <span className="text-sm font-bold text-white">{index + 1}</span>
                      </div>
                      <div>
                        <p className="text-sm font-semibold text-slate-900 dark:text-white">{product.name}</p>
                        <p className="text-xs text-slate-500 dark:text-slate-400">{product.sold} units sold</p>
                      </div>
                    </div>
                    <span className="text-base font-bold text-emerald-600 dark:text-emerald-400">{product.revenue}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </AppShell>
    </ErrorBoundary>
  );
}
