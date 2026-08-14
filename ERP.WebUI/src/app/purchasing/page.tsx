"use client";

import { ShoppingCart, Truck, DollarSign, Package, RefreshCw, Plus } from "lucide-react";
import { AppShell } from "@/app/components/AppShell";
import { api } from "@/lib/api";
import { useEffect, useState } from "react";

interface Supplier {
  id: string;
  supplierName: string;
  supplierCode: string;
  email: string;
  phone: string;
  isActive: boolean;
}

export default function PurchasingPage() {
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<"overview" | "suppliers" | "orders">("overview");

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const res = await api.get<{ items: Supplier[] }>("/api/v1/suppliers");
      if (res.success && res.data) {
        setSuppliers(res.data.items || []);
      }
    } catch (error) {
      console.error("Failed to load purchasing data:", error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <AppShell>
      <div className="space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-slate-800 dark:text-white">Purchasing</h1>
            <p className="text-slate-500 dark:text-slate-400">Manage suppliers and purchase orders</p>
          </div>
          <div className="flex gap-2">
            <button onClick={loadData} className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl hover:bg-slate-50 dark:hover:bg-slate-700">
              <RefreshCw className={`w-4 h-4 ${loading ? "animate-spin" : ""}`} />
              Refresh
            </button>
            <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl">
              <Plus className="w-4 h-4" />
              New Purchase Order
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard icon={Truck} label="Suppliers" value={suppliers.length.toString()} color="blue" />
          <StatCard icon={Package} label="Pending PO" value="0" color="emerald" />
          <StatCard icon={DollarSign} label="Total Spend" value="$0" color="purple" />
          <StatCard icon={ShoppingCart} label="This Month" value="0" color="amber" />
        </div>

        <div className="border-b border-slate-200 dark:border-slate-700">
          <nav className="flex gap-6">
            {["overview", "suppliers", "orders"].map(tab => (
              <button key={tab} onClick={() => setActiveTab(tab as typeof activeTab)} className={`pb-3 text-sm font-medium border-b-2 ${activeTab === tab ? "border-blue-600 text-blue-600" : "border-transparent text-slate-500"}`}>
                {tab.charAt(0).toUpperCase() + tab.slice(1)}
              </button>
            ))}
          </nav>
        </div>

        {loading ? (
          <div className="flex items-center justify-center h-64"><div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" /></div>
        ) : (
          <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
            <h3 className="text-lg font-semibold text-slate-800 dark:text-white mb-4">Suppliers</h3>
            {suppliers.length === 0 ? (
              <p className="text-slate-500 text-center py-8">No suppliers found</p>
            ) : (
              <div className="space-y-3">
                {suppliers.map(s => (
                  <div key={s.id} className="flex items-center justify-between p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 rounded-lg bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center">
                        <Truck className="w-5 h-5 text-emerald-600" />
                      </div>
                      <div>
                        <p className="font-medium text-slate-800 dark:text-white">{s.supplierName}</p>
                        <p className="text-sm text-slate-500">{s.supplierCode}</p>
                      </div>
                    </div>
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${s.isActive ? "bg-green-100 text-green-700" : "bg-slate-100 text-slate-500"}`}>{s.isActive ? "Active" : "Inactive"}</span>
                  </div>
                ))}
              </div>
            )}
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
        <div>
          <p className="text-sm text-slate-500 dark:text-slate-400">{label}</p>
          <p className="text-2xl font-bold text-slate-800 dark:text-white">{value}</p>
        </div>
      </div>
    </div>
  );
}
