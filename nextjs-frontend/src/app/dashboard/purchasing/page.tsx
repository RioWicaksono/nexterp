'use client';

import { useState } from 'react';
import { ShoppingCart, Plus, Search, FileText, Clock, CheckCircle, XCircle } from 'lucide-react';

interface PurchaseOrder {
  id: string;
  orderNumber: string;
  supplier: string;
  date: string;
  total: string;
  status: 'pending' | 'approved' | 'rejected';
}

const mockOrders: PurchaseOrder[] = [
  { id: '1', orderNumber: 'PO-2024-001', supplier: 'PT Supplier A', date: '2024-01-15', total: 'Rp 15.000.000', status: 'approved' },
  { id: '2', orderNumber: 'PO-2024-002', supplier: 'CV Supplier B', date: '2024-01-14', total: 'Rp 8.500.000', status: 'pending' },
  { id: '3', orderNumber: 'PO-2024-003', supplier: 'Toko Elektronik C', date: '2024-01-13', total: 'Rp 22.000.000', status: 'rejected' },
];

const statusBadge = (status: string) => {
  const styles: Record<string, string> = {
    pending: 'bg-yellow-100 text-yellow-700',
    approved: 'bg-green-100 text-green-700',
    rejected: 'bg-red-100 text-red-700',
  };
  const icons: Record<string, any> = { pending: Clock, approved: CheckCircle, rejected: XCircle };
  const Icon = icons[status] || Clock;
  return (
    <span className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium ${styles[status] || ''}`}>
      <Icon className="w-3 h-3" />
      {status}
    </span>
  );
};

export default function PurchasingPage() {
  const [orders] = useState<PurchaseOrder[]>(mockOrders);
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Purchasing</h1>
        <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg">
          <Plus className="w-4 h-4" /> New Order
        </button>
      </div>
      <div className="bg-white dark:bg-slate-800 rounded-xl shadow overflow-hidden">
        <div className="p-4 border-b border-slate-200 dark:border-slate-700">
          <div className="relative max-w-md">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input type="text" placeholder="Search orders..." className="w-full pl-10 pr-4 py-2 border border-slate-200 dark:border-slate-700 rounded-lg" />
          </div>
        </div>
        <table className="w-full text-left">
          <thead className="bg-slate-50 dark:bg-slate-900 text-sm text-slate-500">
            <tr>
              <th className="px-4 py-3 font-medium">Order #</th>
              <th className="px-4 py-3 font-medium">Supplier</th>
              <th className="px-4 py-3 font-medium">Date</th>
              <th className="px-4 py-3 font-medium text-right">Total</th>
              <th className="px-4 py-3 font-medium">Status</th>
              <th className="px-4 py-3 text-right">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y dark:divide-slate-800">
            {orders.map(order => (
              <tr key={order.id} className="hover:bg-slate-50 dark:hover:bg-slate-800">
                <td className="px-4 py-3 font-mono text-sm">{order.orderNumber}</td>
                <td className="px-4 py-3">{order.supplier}</td>
                <td className="px-4 py-3">{order.date}</td>
                <td className="px-4 py-3 text-right font-medium">{order.total}</td>
                <td className="px-4 py-3">{statusBadge(order.status)}</td>
                <td className="px-4 py-3 text-right">
                  <button className="p-1.5 hover:bg-slate-100 rounded dark:hover:bg-slate-700">
                    <FileText className="w-4 h-4" />
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
