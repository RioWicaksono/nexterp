'use client';

import { DollarSign, Plus } from 'lucide-react';

export default function AccountingPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Accounting</h1>
        <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg">
          <Plus className="w-4 h-4" /> New Journal Entry
        </button>
      </div>
      <div className="bg-white dark:bg-slate-800 rounded-xl p-6 shadow">
        <p className="text-slate-500">Accounting module - journal entries, ledgers, reports coming soon</p>
      </div>
    </div>
  );
}
