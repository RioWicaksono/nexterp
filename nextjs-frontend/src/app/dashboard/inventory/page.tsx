'use client';

import { useEffect, useState } from 'react';
import api from '@/lib/api';
import { useAuthStore } from '@/lib/store';
import { Package, Plus, Search, Loader2, Edit, Trash2, Filter } from 'lucide-react';

interface InventoryItem {
  id: string;
  sku: string;
  name: string;
  quantity?: number;
  unitPrice?: number;
  category?: string;
  isActive?: boolean;
}

export default function InventoryPage() {
  const { token } = useAuthStore(state => state.token);
  const [items, setItems] = useState<InventoryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');

  useEffect(() => {
    const fetchItems = async () => {
      setLoading(true);
      try {
        // Demo data for now
        setItems([
          { id: '1', sku: 'SKU-001', name: 'Laptop HP ProBook', quantity: 50, unitPrice: 15000000, category: 'Electronics' },
          { id: '2', sku: 'SKU-002', name: 'Office Chair', quantity: 120, unitPrice: 2500000, category: 'Furniture' },
          { id: '3', sku: 'SKU-003', name: 'Printer Canon', quantity: 25, unitPrice: 3500000, category: 'Electronics' },
        ]);
      } catch (error) {
        console.error('Failed to fetch inventory:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchItems();
  }, []);

  const filtered = items.filter(item =>
    item.name.toLowerCase().includes(search.toLowerCase()) ||
    item.sku.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Inventory</h1>
        <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg">
          <Plus className="w-4 h-4" /> Add Item
        </button>
      </div>

      <div className="bg-white dark:bg-slate-800 rounded-xl shadow p-6">
        <div className="flex items-center gap-4 mb-4">
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input
              type="text"
              placeholder="Search items..."
              value={search}
              onChange={e => setSearch(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-slate-200 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-900"
            />
          </div>
          <button className="flex items-center gap-2 px-4 py-2 border border-slate-200 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-800">
            <Filter className="w-4 h-4" /> Filter
          </button>
        </div>

        {loading ? (
          <div className="flex items-center justify-center py-12">
            <Loader2 className="w-8 h-8 animate-spin text-blue-600" />
          </div>
        ) : (
          <table className="w-full">
            <thead>
              <tr className="text-left text-sm text-slate-500 border-b border-slate-200 dark:border-slate-700">
                <th className="pb-3 font-medium">SKU</th>
                <th className="pb-3 font-medium">Name</th>
                <th className="pb-3 font-medium">Category</th>
                <th className="pb-3 font-medium text-right">Qty</th>
                <th className="pb-3 font-medium text-right">Unit Price</th>
                <th className="pb-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="text-sm">
              {filtered.map(item => (
                <tr key={item.id} className="border-b border-slate-100 dark:border-slate-800">
                  <td className="py-3 font-mono text-slate-600">{item.sku}</td>
                  <td className="py-3 font-medium text-slate-900 dark:text-white">{item.name}</td>
                  <td className="py-3 text-slate-600">{item.category}</td>
                  <td className="py-3 text-right">{item.quantity}</td>
                  <td className="py-3 text-right">Rp {item.unitPrice?.toLocaleString('id-ID')}</td>
                  <td className="py-3 text-right">
                    <button className="p-1.5 hover:bg-slate-100 dark:hover:bg-slate-800 rounded">
                      <Edit className="w-4 h-4" />
                    </button>
                    <button className="p-1.5 hover:bg-red-50 dark:hover:bg-red-900/20 rounded text-red-600">
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
        {!loading && filtered.length === 0 && (
          <p className="text-center py-12 text-slate-500">No items found</p>
        )}
      </div>
    </div>
  );
}
