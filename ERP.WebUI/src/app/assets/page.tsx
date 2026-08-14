"use client";

import { HardDrive, DollarSign, Wrench, AlertCircle, Plus, RefreshCw } from "lucide-react";
import { AppShell } from "@/app/components/AppShell";
import { api } from "@/lib/api";
import { useEffect, useState } from "react";

interface Asset {
  id: string;
  assetName: string;
  assetCode: string;
  assetType: string;
  purchaseDate: string;
  purchaseValue: number;
  currentValue: number;
  status: string;
}

export default function AssetsPage() {
  const [loading, setLoading] = useState(true);
  const [assets, setAssets] = useState<Asset[]>([]);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const res = await api.get<{ items: Asset[] }>("/api/v1/assets");
      if (res.success && res.data) {
        setAssets(res.data.items || []);
      }
    } catch (error) {
      console.error("Failed to load assets:", error);
    } finally {
      setLoading(false);
    }
  };

  const totalValue = assets.reduce((sum, a) => sum + a.purchaseValue, 0);
  const currentValue = assets.reduce((sum, a) => sum + a.currentValue, 0);
  const maintenanceDue = assets.filter(a => a.status === "Maintenance").length;
  const needsAttention = assets.filter(a => a.status === "Needs Repair").length;

  const formatCurrency = (amount: number) => new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(amount);

  return (
    <AppShell>
      <div className="space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-slate-800 dark:text-white">Fixed Assets</h1>
            <p className="text-slate-500 dark:text-slate-400">Manage and track fixed assets</p>
          </div>
          <div className="flex gap-2">
            <button onClick={loadData} className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl hover:bg-slate-50 dark:hover:bg-slate-700">
              <RefreshCw className="w-4 h-4" />
              Refresh
            </button>
            <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl">
              <Plus className="w-4 h-4" />
              Add Asset
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard icon={HardDrive} label="Total Assets" value={assets.length.toString()} color="blue" />
          <StatCard icon={DollarSign} label="Total Value" value={formatCurrency(totalValue)} color="emerald" />
          <StatCard icon={Wrench} label="Maintenance Due" value={maintenanceDue.toString()} color="purple" />
          <StatCard icon={AlertCircle} label="Need Attention" value={needsAttention.toString()} color="amber" />
        </div>

        <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-slate-50 dark:bg-slate-700/50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Asset</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Code</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Type</th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-slate-500 uppercase">Purchase Value</th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-slate-500 uppercase">Current Value</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                {loading ? (
                  <tr><td colSpan={6} className="px-6 py-12 text-center"><div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin mx-auto" /></td></tr>
                ) : assets.length === 0 ? (
                  <tr><td colSpan={6} className="px-6 py-12 text-center text-slate-500">No assets found</td></tr>
                ) : (
                  assets.map(asset => (
                    <tr key={asset.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/50">
                      <td className="px-6 py-4 font-medium text-slate-800 dark:text-white">{asset.assetName}</td>
                      <td className="px-6 py-4 text-slate-500">{asset.assetCode}</td>
                      <td className="px-6 py-4 text-slate-500">{asset.assetType}</td>
                      <td className="px-6 py-4 text-right text-slate-800 dark:text-white">{formatCurrency(asset.purchaseValue)}</td>
                      <td className="px-6 py-4 text-right font-semibold text-slate-800 dark:text-white">{formatCurrency(asset.currentValue)}</td>
                      <td className="px-6 py-4">
                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${asset.status === "Active" ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400" : "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400"}`}>{asset.status}</span>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
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
        <div><p className="text-sm text-slate-500 dark:text-slate-400">{label}</p><p className="text-xl font-bold text-slate-800 dark:text-white">{value}</p></div>
      </div>
    </div>
  );
}
