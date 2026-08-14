"use client";

import { ShoppingCart, Users, DollarSign, TrendingUp, RefreshCw, Plus } from "lucide-react";
import { AppShell } from "@/app/components/AppShell";
import { api } from "@/lib/api";
import { useEffect, useState } from "react";

interface Customer {
  id: string;
  customerName: string;
  customerCode: string;
  email: string;
  phone: string;
  isActive: boolean;
}

export default function SalesPage() {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<"overview" | "customers" | "orders">("overview");

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const res = await api.get<{ items: Customer[] }>("/api/v1/customers");
      if (res.success && res.data) {
        setCustomers(res.data.items || []);
      }
    } catch (error) {
      console.error("Failed to load sales data:", error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <AppShell>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-slate-800 dark:text-white">
              Sales Management
            </h1>
            <p className="text-slate-500 dark:text-slate-400">
              Manage customers, sales orders, and invoices
            </p>
          </div>
          <div className="flex gap-2">
            <button onClick={loadData} className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl hover:bg-slate-50 dark:hover:bg-slate-700">
              <RefreshCw className={`w-4 h-4 ${loading ? "animate-spin" : ""}`} />
              Refresh
            </button>
            <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl">
              <Plus className="w-4 h-4" />
              New Sales Order
            </button>
          </div>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard icon={Users} label="Customers" value={customers.length.toString()} color="blue" />
          <StatCard icon={ShoppingCart} label="Pending Orders" value="0" color="emerald" />
          <StatCard icon={DollarSign} label="Revenue" value="$0" color="purple" />
          <StatCard icon={TrendingUp} label="Growth" value="0%" color="amber" />
        </div>

        {/* Tabs */}
        <div className="border-b border-slate-200 dark:border-slate-700">
          <nav className="flex gap-6">
            {["overview", "customers", "orders"].map(tab => (
              <button
                key={tab}
                onClick={() => setActiveTab(tab as typeof activeTab)}
                className={`pb-3 text-sm font-medium border-b-2 transition-colors ${
                  activeTab === tab
                    ? "border-blue-600 text-blue-600"
                    : "border-transparent text-slate-500 hover:text-slate-700"
                }`}
              >
                {tab.charAt(0).toUpperCase() + tab.slice(1)}
              </button>
            ))}
          </nav>
        </div>

        {/* Content */}
        {loading ? (
          <div className="flex items-center justify-center h-64">
            <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
          </div>
        ) : activeTab === "overview" ? (
          <div className="grid lg:grid-cols-2 gap-6">
            <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
              <h3 className="text-lg font-semibold text-slate-800 dark:text-white mb-4">Customers</h3>
              {customers.length === 0 ? (
                <p className="text-slate-500 text-center py-8">No customers found</p>
              ) : (
                <div className="space-y-3">
                  {customers.slice(0, 5).map(c => (
                    <div key={c.id} className="flex items-center gap-3 p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl">
                      <div className="w-10 h-10 rounded-lg bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center">
                        <Users className="w-5 h-5 text-blue-600" />
                      </div>
                      <div>
                        <p className="font-medium text-slate-800 dark:text-white">{c.customerName}</p>
                        <p className="text-sm text-slate-500">{c.customerCode}</p>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
            <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
              <h3 className="text-lg font-semibold text-slate-800 dark:text-white mb-4">Recent Orders</h3>
              <p className="text-slate-500 text-center py-8">No recent orders</p>
            </div>
          </div>
        ) : activeTab === "customers" ? (
          <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 overflow-hidden">
            <table className="w-full">
              <thead className="bg-slate-50 dark:bg-slate-700/50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Customer</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Code</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Email</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                {customers.length === 0 ? (
                  <tr><td colSpan={4} className="px-6 py-12 text-center text-slate-500">No customers found</td></tr>
                ) : (
                  customers.map(c => (
                    <tr key={c.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/50">
                      <td className="px-6 py-4 font-medium text-slate-800 dark:text-white">{c.customerName}</td>
                      <td className="px-6 py-4 text-slate-500">{c.customerCode}</td>
                      <td className="px-6 py-4 text-slate-500">{c.email}</td>
                      <td className="px-6 py-4">
                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${c.isActive ? "bg-green-100 text-green-700" : "bg-slate-100 text-slate-500"}`}>
                          {c.isActive ? "Active" : "Inactive"}
                        </span>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
            <p className="text-slate-500 text-center py-12">No sales orders found</p>
          </div>
        )}
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
