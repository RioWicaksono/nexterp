'use client';

import { useState } from 'react';
import { Shield, Check, X } from 'lucide-react';
import { useRouter } from 'next/navigation';

const MODULES = [
  { id: '1', name: 'Human Resource Management', code: 'HRM', description: 'Employee, attendance, payroll', enabled: true },
  { id: '2', name: 'Inventory Management', code: 'INV', description: 'Stock items, warehouses, opname', enabled: true },
  { id: '3', name: 'Purchasing', code: 'PUR', description: 'Suppliers, purchase orders, GRN', enabled: true },
  { id: '4', name: 'Accounting', code: 'ACC', description: 'Chart of accounts, journals', enabled: true },
  { id: '5', name: 'Projects', code: 'PRJ', description: 'Project management, tasks', enabled: false },
  { id: '6', name: 'Sales & Distribution', code: 'SAL', description: 'Customers, invoices', enabled: false },
  { id: '7', name: 'Fixed Assets', code: 'AST', description: 'Asset management', enabled: false },
];

export default function ModulesPage() {
  const [items, setItems] = useState(MODULES);
  const router = useRouter();

  const toggle = (id: string) => {
    setItems(prev => prev.map(m => m.id === id ? { ...m, enabled: !m.enabled } : m));
    router.refresh();
  };

  return (
    <div className="p-6 space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Modules</h1>
        <p className="text-sm text-slate-500 mt-1">{items.filter(m => m.enabled).length} of {items.length} modules enabled</p>
      </div>

      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 overflow-hidden">
        <table className="w-full">
          <thead className="bg-slate-50 dark:bg-slate-700/50">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Module</th>
              <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Code</th>
              <th className="px-4 py-3 text-center text-xs font-semibold text-slate-500 uppercase">Status</th>
              <th className="px-4 py-3 text-center text-xs font-semibold text-slate-500 uppercase">Toggle</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100 dark:divide-slate-700">
            {items.map(mod => (
              <tr key={mod.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/30">
                <td className="px-4 py-3">
                  <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-slate-100 dark:bg-slate-700 flex items-center justify-center">
                      <Shield className="w-5 h-5 text-slate-500" />
                    </div>
                    <div>
                      <p className="font-medium text-slate-900 dark:text-white">{mod.name}</p>
                      <p className="text-xs text-slate-500">{mod.description}</p>
                    </div>
                  </div>
                </td>
                <td className="px-4 py-3">
                  <span className="px-2 py-0.5 rounded text-xs font-medium bg-slate-100 dark:bg-slate-700 text-slate-500">{mod.code}</span>
                </td>
                <td className="px-4 py-3 text-center">
                  <span className={`px-2 py-1 text-xs rounded-full font-medium ${mod.enabled ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400' : 'bg-slate-100 text-slate-500 dark:bg-slate-700'}`}>
                    {mod.enabled ? 'Enabled' : 'Disabled'}
                  </span>
                </td>
                <td className="px-4 py-3 text-center">
                  <button
                    onClick={() => toggle(mod.id)}
                    aria-label={mod.enabled ? `Disable ${mod.name}` : `Enable ${mod.name}`}
                    className={`p-2 rounded-lg ${mod.enabled ? 'text-green-600 hover:bg-green-50' : 'text-slate-400 hover:bg-slate-100'}`}
                  >
                    {mod.enabled ? <X className="w-4 h-4" /> : <Check className="w-4 h-4" />}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
