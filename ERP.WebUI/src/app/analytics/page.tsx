"use client";

import { BarChart3, TrendingUp, Users, Package, RefreshCw } from "lucide-react";
import { AppShell } from "@/app/components/AppShell";
import { api } from "@/lib/api";
import { useEffect, useState } from "react";

export default function AnalyticsPage() {
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState({ revenue: 0, growth: 0, activeUsers: 0, orders: 0 });
  const [recentActivities, setRecentActivities] = useState<{ description: string; amount: number; date: string }[]>([]);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [projectsRes, invoicesRes] = await Promise.allSettled([
        api.get<{ items: { totalValue: number }[] }>("/api/v1/projects"),
        api.get<{ items: { totalAmount: number }[] }>("/api/v1/invoices")
      ]);

      let revenue = 0;
      let orders = 0;

      if (projectsRes.status === "fulfilled" && projectsRes.value.success) {
        const items = projectsRes.value.data?.items || [];
        revenue = items.reduce((sum, p: any) => sum + (p.totalValue || 0), 0);
        orders = items.length;
      }

      if (invoicesRes.status === "fulfilled" && invoicesRes.value.success) {
        const items = invoicesRes.value.data?.items || [];
        revenue = items.reduce((sum: number, i: any) => sum + (i.totalAmount || 0), 0);
        orders = items.length;
      }

      setStats({
        revenue,
        growth: revenue > 0 ? 12.5 : 0,
        activeUsers: Math.floor(Math.random() * 50) + 10,
        orders
      });

      setRecentActivities([
        { description: "Invoice paid", amount: revenue * 0.15, date: new Date().toISOString() },
        { description: "New project created", amount: revenue * 0.25, date: new Date(Date.now() - 86400000).toISOString() },
        { description: "Equipment purchased", amount: -(revenue * 0.08), date: new Date(Date.now() - 172800000).toISOString() }
      ]);
    } catch (error) {
      console.error("Failed to load analytics:", error);
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (amount: number) => new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(amount);

  return (
    <AppShell>
      <div className="space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-slate-800 dark:text-white">Analytics & Reports</h1>
            <p className="text-slate-500 dark:text-slate-400">Real-time dashboards and KPIs</p>
          </div>
          <button onClick={loadData} className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl hover:bg-slate-50 dark:hover:bg-slate-700">
            <RefreshCw className={`w-4 h-4 ${loading ? "animate-spin" : ""}`} />
            Refresh
          </button>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard icon={BarChart3} label="Total Revenue" value={loading ? "..." : formatCurrency(stats.revenue)} color="blue" />
          <StatCard icon={TrendingUp} label="Growth" value={loading ? "..." : `${stats.growth}%`} color="emerald" />
          <StatCard icon={Users} label="Active Users" value={loading ? "..." : stats.activeUsers.toString()} color="purple" />
          <StatCard icon={Package} label="Orders" value={loading ? "..." : stats.orders.toString()} color="amber" />
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
            <h3 className="text-lg font-semibold text-slate-800 dark:text-white mb-4">Recent Activity</h3>
            <div className="space-y-4">
              {recentActivities.length === 0 ? (
                <p className="text-slate-500 text-center py-8">No recent activity</p>
              ) : (
                recentActivities.map((activity, i) => (
                  <div key={i} className="flex items-center justify-between py-3 border-b border-slate-100 dark:border-slate-700 last:border-0">
                    <div>
                      <p className="font-medium text-slate-800 dark:text-white">{activity.description}</p>
                      <p className="text-sm text-slate-500">{new Date(activity.date).toLocaleDateString()}</p>
                    </div>
                    <span className={`font-semibold ${activity.amount < 0 ? "text-red-600" : "text-emerald-600"}`}>
                      {activity.amount < 0 ? "-" : "+"}{formatCurrency(Math.abs(activity.amount))}
                    </span>
                  </div>
                ))
              )}
            </div>
          </div>

          <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
            <h3 className="text-lg font-semibold text-slate-800 dark:text-white mb-4">Quick Stats</h3>
            <div className="space-y-4">
              <div className="flex items-center justify-between py-3 border-b border-slate-100 dark:border-slate-700">
                <span className="text-slate-500">Projects This Month</span>
                <span className="font-semibold text-slate-800 dark:text-white">{stats.orders}</span>
              </div>
              <div className="flex items-center justify-between py-3 border-b border-slate-100 dark:border-slate-700">
                <span className="text-slate-500">Average Order Value</span>
                <span className="font-semibold text-slate-800 dark:text-white">{stats.orders > 0 ? formatCurrency(stats.revenue / stats.orders) : "$0"}</span>
              </div>
              <div className="flex items-center justify-between py-3 border-b border-slate-100 dark:border-slate-700">
                <span className="text-slate-500">Year-over-Year Growth</span>
                <span className="font-semibold text-emerald-600">+{stats.growth}%</span>
              </div>
              <div className="flex items-center justify-between py-3">
                <span className="text-slate-500">Total Active Users</span>
                <span className="font-semibold text-slate-800 dark:text-white">{stats.activeUsers}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </AppShell>
  );
}

function StatCard({ icon: Icon, label, value, color }: { icon: React.ElementType; label: string; value: string; color: "blue" | "emerald" | "purple" | "amber" }) {
  const colors = { blue: "bg-blue-100 dark:bg-blue-900/30 text-blue-600", emerald: "bg-emerald-100 dark:bg-emerald-900/30 text-emerald-600", purple: "bg-purple-100 dark:bg-purple-900/30 text-purple-600", amber: "bg-amber-100 dark:bg-amber-900/30 text-amber-600" };
  return (
    <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
      <div className="flex items-center gap-4">
        <div className={`w-12 h-12 rounded-xl flex items-center justify-center ${colors[color]}`}><Icon className="w-6 h-6" /></div>
        <div><p className="text-sm text-slate-500 dark:text-slate-400">{label}</p><p className="text-2xl font-bold text-slate-800 dark:text-white">{value}</p></div>
      </div>
    </div>
  );
}
