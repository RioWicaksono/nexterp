"use client";

import { Wallet, FileText, DollarSign, PieChart, RefreshCw, Plus } from "lucide-react";
import { AppShell } from "@/app/components/AppShell";
import { api } from "@/lib/api";
import { useEffect, useState } from "react";

interface JournalEntry {
  id: string;
  entryNumber: string;
  entryDate: string;
  description: string;
  totalDebit: number;
  totalCredit: number;
  status: string;
}

interface Account {
  id: string;
  accountCode: string;
  accountName: string;
  accountType: string;
  balance: number;
  isActive: boolean;
}

export default function AccountingPage() {
  const [loading, setLoading] = useState(true);
  const [journalEntries, setJournalEntries] = useState<JournalEntry[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [activeTab, setActiveTab] = useState<"overview" | "journal" | "accounts">("overview");

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [journalRes, accountsRes] = await Promise.all([
        api.get<{ items: JournalEntry[] }>("/api/v1/journal-entries?pageSize=10"),
        api.get<{ items: Account[] }>("/api/v1/accounts"),
      ]);

      if (journalRes.success && journalRes.data) {
        setJournalEntries(journalRes.data.items || []);
      }
      if (accountsRes.success && accountsRes.data) {
        setAccounts(accountsRes.data.items || []);
      }
    } catch (error) {
      console.error("Failed to load accounting data:", error);
    } finally {
      setLoading(false);
    }
  };

  const totalRevenue = accounts.filter(a => a.accountType === "Revenue").reduce((sum, a) => sum + a.balance, 0);
  const totalExpenses = accounts.filter(a => a.accountType === "Expense").reduce((sum, a) => sum + a.balance, 0);
  const cashBalance = accounts.filter(a => a.accountType === "Asset" && a.accountName.toLowerCase().includes("cash")).reduce((sum, a) => sum + a.balance, 0);

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(amount);
  };

  return (
    <AppShell>
      <div className="space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-slate-800 dark:text-white">Accounting</h1>
            <p className="text-slate-500 dark:text-slate-400">Financial management and bookkeeping</p>
          </div>
          <div className="flex gap-2">
            <button
              onClick={loadData}
              className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
            >
              <RefreshCw className="w-4 h-4" />
              Refresh
            </button>
            <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl transition-colors">
              <Plus className="w-4 h-4" />
              New Entry
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard icon={Wallet} label="Cash Balance" value={formatCurrency(cashBalance)} color="blue" />
          <StatCard icon={FileText} label="Journal Entries" value={journalEntries.length.toString()} color="emerald" />
          <StatCard icon={DollarSign} label="Revenue" value={formatCurrency(totalRevenue)} color="purple" />
          <StatCard icon={PieChart} label="Expenses" value={formatCurrency(totalExpenses)} color="amber" />
        </div>

        <div className="border-b border-slate-200 dark:border-slate-700">
          <nav className="flex gap-6">
            {[
              { id: "overview", label: "Overview" },
              { id: "journal", label: "Journal Entries" },
              { id: "accounts", label: "Chart of Accounts" },
            ].map(tab => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id as typeof activeTab)}
                className={`
                  pb-3 text-sm font-medium border-b-2 transition-colors
                  ${activeTab === tab.id
                    ? "border-blue-600 text-blue-600"
                    : "border-transparent text-slate-500 hover:text-slate-700 dark:hover:text-slate-300"
                  }
                `}
              >
                {tab.label}
              </button>
            ))}
          </nav>
        </div>

        {loading ? (
          <div className="flex items-center justify-center h-64">
            <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
          </div>
        ) : (
          <>
            {activeTab === "overview" && (
              <div className="grid lg:grid-cols-2 gap-6">
                <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
                  <h3 className="text-lg font-semibold text-slate-800 dark:text-white mb-4">Recent Journal Entries</h3>
                  {journalEntries.length === 0 ? (
                    <p className="text-slate-500 text-center py-8">No journal entries yet</p>
                  ) : (
                    <div className="space-y-3">
                      {journalEntries.slice(0, 5).map(entry => (
                        <div key={entry.id} className="flex items-center justify-between p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl">
                          <div>
                            <p className="font-medium text-slate-800 dark:text-white">{entry.entryNumber}</p>
                            <p className="text-sm text-slate-500">{entry.description}</p>
                          </div>
                          <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                            entry.status === "Posted"
                              ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400"
                              : "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400"
                          }`}>
                            {entry.status}
                          </span>
                        </div>
                      ))}
                    </div>
                  )}
                </div>

                <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
                  <h3 className="text-lg font-semibold text-slate-800 dark:text-white mb-4">Top Accounts</h3>
                  {accounts.length === 0 ? (
                    <p className="text-slate-500 text-center py-8">No accounts configured</p>
                  ) : (
                    <div className="space-y-3">
                      {accounts.slice(0, 5).map(account => (
                        <div key={account.id} className="flex items-center justify-between p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl">
                          <div>
                            <p className="font-medium text-slate-800 dark:text-white">{account.accountName}</p>
                            <p className="text-sm text-slate-500">{account.accountCode}</p>
                          </div>
                          <span className="font-semibold text-slate-800 dark:text-white">
                            {formatCurrency(account.balance)}
                          </span>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            )}

            {activeTab === "journal" && (
              <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 overflow-hidden">
                <table className="w-full">
                  <thead className="bg-slate-50 dark:bg-slate-700/50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Entry #</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Date</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Description</th>
                      <th className="px-6 py-3 text-right text-xs font-medium text-slate-500 uppercase">Debit</th>
                      <th className="px-6 py-3 text-right text-xs font-medium text-slate-500 uppercase">Credit</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Status</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                    {journalEntries.length === 0 ? (
                      <tr>
                        <td colSpan={6} className="px-6 py-12 text-center text-slate-500">No journal entries found</td>
                      </tr>
                    ) : (
                      journalEntries.map(entry => (
                        <tr key={entry.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/50">
                          <td className="px-6 py-4 font-medium text-slate-800 dark:text-white">{entry.entryNumber}</td>
                          <td className="px-6 py-4 text-slate-500">{new Date(entry.entryDate).toLocaleDateString()}</td>
                          <td className="px-6 py-4 text-slate-500">{entry.description}</td>
                          <td className="px-6 py-4 text-right text-slate-800 dark:text-white">{formatCurrency(entry.totalDebit)}</td>
                          <td className="px-6 py-4 text-right text-slate-800 dark:text-white">{formatCurrency(entry.totalCredit)}</td>
                          <td className="px-6 py-4">
                            <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                              entry.status === "Posted"
                                ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400"
                                : "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400"
                            }`}>
                              {entry.status}
                            </span>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            )}

            {activeTab === "accounts" && (
              <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 overflow-hidden">
                <table className="w-full">
                  <thead className="bg-slate-50 dark:bg-slate-700/50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Code</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Account Name</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Type</th>
                      <th className="px-6 py-3 text-right text-xs font-medium text-slate-500 uppercase">Balance</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Status</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                    {accounts.length === 0 ? (
                      <tr>
                        <td colSpan={5} className="px-6 py-12 text-center text-slate-500">No accounts configured</td>
                      </tr>
                    ) : (
                      accounts.map(account => (
                        <tr key={account.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/50">
                          <td className="px-6 py-4 font-medium text-slate-800 dark:text-white">{account.accountCode}</td>
                          <td className="px-6 py-4 text-slate-500">{account.accountName}</td>
                          <td className="px-6 py-4 text-slate-500">{account.accountType}</td>
                          <td className="px-6 py-4 text-right font-semibold text-slate-800 dark:text-white">{formatCurrency(account.balance)}</td>
                          <td className="px-6 py-4">
                            <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                              account.isActive
                                ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400"
                                : "bg-slate-100 text-slate-500"
                            }`}>
                              {account.isActive ? "Active" : "Inactive"}
                            </span>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            )}
          </>
        )}
      </div>
    </AppShell>
  );
}

function StatCard({ icon: Icon, label, value, color }: { icon: React.ElementType; label: string; value: string; color: "blue" | "emerald" | "purple" | "amber" }) {
  const colors = {
    blue: "bg-blue-100 dark:bg-blue-900/30 text-blue-600",
    emerald: "bg-emerald-100 dark:bg-emerald-900/30 text-emerald-600",
    purple: "bg-purple-100 dark:bg-purple-900/30 text-purple-600",
    amber: "bg-amber-100 dark:bg-amber-900/30 text-amber-600",
  };
  return (
    <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
      <div className="flex items-center gap-4">
        <div className={`w-12 h-12 rounded-xl flex items-center justify-center ${colors[color]}`}><Icon className="w-6 h-6" /></div>
        <div><p className="text-sm text-slate-500 dark:text-slate-400">{label}</p><p className="text-xl font-bold text-slate-800 dark:text-white">{value}</p></div>
      </div>
    </div>
  );
}
