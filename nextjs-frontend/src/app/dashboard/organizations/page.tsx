'use client';

import { Building2, Plus, Search } from 'lucide-react';

export default function OrganizationsPage() {
  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Organizations</h1>
        <button type="button" className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg">
          <Plus className="w-4 h-4" /> New Organization
        </button>
      </div>
      <div className="bg-white dark:bg-slate-800 rounded-xl shadow overflow-hidden">
        <div className="p-4 border-b border-slate-200 dark:border-slate-700">
          <div className="relative max-w-md">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input type="text" placeholder="Search organizations..." className="w-full pl-10 pr-4 py-2 border border-slate-200 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-900" />
          </div>
        </div>
        <div className="p-8 text-center">
          <Building2 className="w-12 h-12 mx-auto text-slate-300 dark:text-slate-600 mb-4" />
          <p className="text-slate-500 dark:text-slate-400">Nexterp Demo Corp - Professional tier</p>
        </div>
      </div>
    </div>
  );
}
