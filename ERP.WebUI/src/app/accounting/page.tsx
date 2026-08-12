"use client";

import { Wallet, FileText, DollarSign, PieChart } from "lucide-react";
import { AppShell } from "../components/AppShell";

export default function AccountingPage() {
  return (
    <AppShell>
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-bold text-slate-800 dark:text-white">Accounting</h1>
          <p className="text-slate-500 dark:text-slate-400">Financial management and bookkeeping</p>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard icon={Wallet} label="Cash Balance" value="$0" color="blue" />
          <StatCard icon={FileText} label="Journal Entries" value="0" color="emerald" />
          <StatCard icon={DollarSign} label="Revenue" value="$0" color="purple" />
          <StatCard icon={PieChart} label="Expenses" value="$0" color="amber" />
        </div>

        <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
          <h3 className="text-lg font-semibold text-slate-800 dark:text-white mb-4">Chart of Accounts</h3>
          <p className="text-slate-500 text-center py-8">No accounts configured yet</p>
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
        <div><p className="text-sm text-slate-500">{label}</p><p className="text-2xl font-bold text-slate-800 dark:text-white">{value}</p></div>
      </div>
    </div>
  );
}
