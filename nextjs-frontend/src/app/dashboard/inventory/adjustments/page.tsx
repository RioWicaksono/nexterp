'use client';

import { useState } from 'react';
import { Plus, Minus, ArrowRight, Trash2 } from 'lucide-react';
import { PageHeader } from '@/components/PageHeader';
import { cn } from '@/lib/utils';

interface Adjustment {
  id: string;
  itemId: string;
  itemName: string;
  quantity: number;
  reason: 'damaged' | 'found' | 'correction';
  date: string;
  notes?: string;
}

const mockAdjustments: Adjustment[] = [
  { id: '1', itemId: '1', itemName: 'Laptop Dell XPS 15', quantity: -2, reason: 'damaged', date: '2024-08-15' },
  { id: '2', itemId: '2', itemName: 'Monitor LG 27"', quantity: 5, reason: 'found', date: '2024-08-14' },
];

export default function AdjustmentsPage() {
  const [adjustments] = useState(mockAdjustments);
  return (
    <div className="space-y-6">
      <PageHeader title="Stock Adjustments" subtitle="Record inventory count corrections" breadcrumbs={[{ label: 'Dashboard', href: '/dashboard' }, { label: 'Inventory' }, { label: 'Adjustments' }]} />
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 overflow-hidden">
        {adjustments.map(a => (
          <div key={a.id} className="flex items-center gap-4 p-4 border-b border-slate-100 dark:border-slate-700 last:border-0">
            <div className={cn('p-2 rounded-full', a.quantity < 0 ? 'bg-red-100 text-red-600' : 'bg-green-100 text-green-600')}>
              {a.quantity > 0 ? <Plus className="w-4 h-4" /> : <Minus className="w-4 h-4" />}
            </div>
            <div className="flex-1">
              <p className="font-medium text-slate-900 dark:text-white">{a.itemName}</p>
              <p className="text-sm text-slate-500">{a.reason} · {a.date}</p>
            </div>
            <span className={cn('font-mono font-medium', a.quantity > 0 ? 'text-green-600' : 'text-red-600')}>
              {a.quantity > 0 ? '+' : ''}{a.quantity}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
