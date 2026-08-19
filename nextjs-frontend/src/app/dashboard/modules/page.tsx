'use client';

import { useState } from 'react';
import { useEffect } from 'react';
import { modulesApi } from '@/lib/api';
import { Breadcrumbs } from '@/components/Breadcrumbs';
import { PageHeader } from '@/components/PageHeader';
import { SkeletonLoader } from '@/components/SkeletonLoader';
import { useToast } from '@/hooks/useToast';
import { Shield, Check, X, Loader2 } from 'lucide-react';

interface Module {
  id: string;
  name: string;
  code: string;
  description: string;
  isEnabled: boolean;
}

export default function ModulesPage() {
  const toast = useToast();
  const [modules, setModules] = useState<Module[]>([]);
  const [loading, setLoading] = useState(true);
  const [enabling, setEnabling] = useState<string | null>(null);

  const defaultModules: Module[] = [
    { id: '1', name: 'Human Resource Management', code: 'HRM', description: 'Employee, attendance, leave, payroll', isEnabled: true },
    { id: '2', name: 'Inventory Management', code: 'INV', description: 'Stock items, warehouses, stock opname', isEnabled: true },
    { id: '3', name: 'Purchasing', code: 'PUR', description: 'Suppliers, purchase orders, GRN', isEnabled: true },
    { id: '4', name: 'Accounting', code: 'ACC', description: 'Chart of accounts, journals, reports', isEnabled: true },
    { id: '5', name: 'Projects', code: 'PRJ', description: 'Project management, tasks, milestones', isEnabled: false },
    { id: '6', name: 'Sales & Distribution', code: 'SAL', description: 'Customers, sales orders, invoices', isEnabled: false },
    { id: '7', name: 'Fixed Assets', code: 'AST', description: 'Asset registration, depreciation', isEnabled: false },
    { id: '8', name: 'Quality Control', code: 'QC', description: 'Inspections, non-conformance', isEnabled: false },
  ];

  useEffect(() => {
    setModules(defaultModules);
    setLoading(false);
  }, []);

  const handleToggle = async (id: string, current: boolean) => {
    setEnabling(id);
    await new Promise(r => setTimeout(r, 500));
    setModules(prev => prev.map(m => m.id === id ? { ...m, isEnabled: !current } : m));
    toast('success', current ? 'Disabled' : 'Enabled', `Module ${current ? 'disabled' : 'enabled'}`);
    setEnabling(null);
  };

  return (
    <div className="space-y-4">
      <Breadcrumbs items={[{ label: 'Dashboard', href: '/dashboard' }, { label: 'Modules' }]} />
      <PageHeader title="Modules" subtitle="Enable or disable system modules" />

      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 overflow-hidden">
        <div className="flex items-center justify-between px-4 py-3 border-b border-slate-200 dark:border-slate-700">
          <p className="text-sm text-slate-500">{modules.filter(m => m.isEnabled).length} of {modules.length} modules enabled</p>
        </div>
        {loading ? (
          <SkeletonLoader rows={5} />
        ) : (
          <div className="divide-y divide-slate-100 dark:divide-slate-700">
            {modules.map(mod => (
              <div key={mod.id} className="flex items-center justify-between px-4 py-3 hover:bg-slate-50 dark:hover:bg-slate-700/30 transition">
                <div className="flex items-center gap-4">
                  <div className="w-10 h-10 rounded-lg bg-slate-100 dark:bg-slate-700 flex items-center justify-center">
                    <Shield className="w-5 h-5 text-slate-500" />
                  </div>
                  <div>
                    <p className="font-medium text-slate-900 dark:text-white">{mod.name}</p>
                    <p className="text-sm text-slate-500">{mod.description}</p>
                  </div>
                </div>
                <div className="flex items-center gap-3">
                  <span className={`px-2 py-1 text-xs font-medium rounded-full ${mod.isEnabled ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400' : 'bg-slate-100 text-slate-500 dark:bg-slate-700'}`}>
                    {mod.isEnabled ? 'Enabled' : 'Disabled'}
                  </span>
                  <button
                    onClick={() => handleToggle(mod.id, mod.isEnabled)}
                    disabled={enabling === mod.id}
                    className={`p-2 rounded-lg transition ${mod.isEnabled ? 'text-green-600 hover:bg-green-50' : 'text-slate-400 hover:bg-slate-100'}`}
                  >
                    {enabling === mod.id ? <Loader2 className="w-4 h-4 animate-spin" /> : mod.isEnabled ? <X className="w-4 h-4" /> : <Check className="w-4 h-4" />}
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
