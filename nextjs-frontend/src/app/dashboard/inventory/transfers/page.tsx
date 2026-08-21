'use client';

import { useState } from 'react';
import { ArrowRight } from 'lucide-react';
import { PageHeader } from '@/components/PageHeader';
import { cn } from '@/lib/utils';

interface Transfer {
  id: string;
  itemName: string;
  fromWarehouse: string;
  toWarehouse: string;
  quantity: number;
  status: 'pending' | 'approved' | 'completed';
  date: string;
}

const mock: Transfer[] = [
  { id: '1', itemName: 'Laptop Dell XPS 15', fromWarehouse: 'Main Warehouse', toWarehouse: 'Distribution Center', quantity: 5, status: 'completed', date: '2024-08-15' },
  { id: '2', itemName: 'Monitor LG 27"', fromWarehouse: 'Backup Storage', toWarehouse: 'Main Warehouse', quantity: 3, status: 'pending', date: '2024-08-14' },
];

const statusColor = { pending: 'bg-yellow-100 text-yellow-700', approved: 'bg-blue-100 text-blue-700', completed: 'bg-green-100 text-green-700' };

export default function TransfersPage() {
  const [transfers] = useState(mock);
  return (
    <div className="space-y-6">
      <PageHeader title="Warehouse Transfers" subtitle="Move inventory between locations" breadcrumbs={[{ label: 'Dashboard', href: '/dashboard' }, { label: 'Inventory' }, { label: 'Transfers' }]} />
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 overflow-hidden">
        {transfers.map(t => (
          <div key={t.id} className="flex items-center gap-4 p-4 border-b border-slate-100 dark:border-slate-700 last:border-0">
            <div className="flex-1">
              <p className="font-medium text-slate-900 dark:text-white">{t.itemName}</p>
              <p className="text-sm text-slate-500">{t.date}</p>
            </div>
            <div className="flex items-center gap-2 text-sm">
              <span className="text-slate-500">{t.fromWarehouse}</span>
              <ArrowRight className="w-4 h-4 text-slate-400" />
              <span className="text-slate-700 dark:text-slate-300">{t.toWarehouse}</span>
            </div>
            <span className={cn('px-2 py-1 text-xs rounded-full capitalize', statusColor[t.status])>{t.status}</span>
            <span className="font-mono text-slate-700 dark:text-slate-300">×{t.quantity}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
