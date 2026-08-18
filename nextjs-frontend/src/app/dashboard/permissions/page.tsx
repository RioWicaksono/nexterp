'use client';

import { Shield, Plus } from 'lucide-react';

const modules = ['HRM', 'Inventory', 'Purchasing', 'Accounting', 'Projects', 'Assets', 'Quality', 'Analytics', 'Admin'];

export default function PermissionsPage() {
  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Permissions</h1>
      </div>
      <div className="bg-white dark:bg-slate-800 rounded-xl p-6 shadow">
        <p className="text-slate-500 dark:text-slate-400">Permission management by module - organized by: {modules.join(', ')}</p>
      </div>
    </div>
  );
}
