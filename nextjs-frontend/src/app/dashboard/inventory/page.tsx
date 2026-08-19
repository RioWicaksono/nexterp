'use client';

import { useEffect, useState, useCallback } from 'react';
import { stockItemsApi, warehousesApi, type StockItemDto, type WarehouseDto } from '@/lib/api';
import { Plus, Search, Edit2, Trash2, X, Loader2, ChevronLeft, ChevronRight, Package, Warehouse as WarehouseIcon } from 'lucide-react';

export default function InventoryPage() {
  const [items, setItems] = useState<StockItemDto[]>([]);
  const [warehouses, setWarehouses] = useState<WarehouseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [showModal, setShowModal] = useState(false);
  const [editingItem, setEditingItem] = useState<StockItemDto | null>(null);
  const [formData, setFormData] = useState({ name: '', code: '', barcode: '', standardCost: '', standardPrice: '', reorderLevel: '' });
  const [saving, setSaving] = useState(false);
  const [deleteConfirm, setDeleteConfirm] = useState<string | null>(null);
  const pageSize = 10;

  const fetchItems = useCallback(async () => {
    setLoading(true);
    try {
      const result = await stockItemsApi.getAll({ page, pageSize, search: search || undefined });
      if (result?.success && result.data) {
        setItems(result.data.items || []);
        setTotalCount(result.data.totalCount || 0);
        setTotalPages(Math.ceil((result.data.totalCount || 0) / pageSize));
      } else {
        setItems([]);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load items');
    } finally {
      setLoading(false);
    }
  }, [page, search]);

  const fetchWarehouses = useCallback(async () => {
    try {
      const result = await warehousesApi.getAll({ pageSize: 100 });
      if (result?.success && result.data) {
        setWarehouses(result.data.items || []);
      }
    } catch {}
  }, []);

  useEffect(() => { fetchItems(); }, [fetchItems]);
  useEffect(() => { fetchWarehouses(); }, [fetchWarehouses]);

  const openCreate = () => {
    setEditingItem(null);
    setFormData({ name: '', code: '', barcode: '', standardCost: '', standardPrice: '', reorderLevel: '' });
    setShowModal(true);
  };

  const openEdit = (item: StockItemDto) => {
    setEditingItem(item);
    setFormData({
      name: item.name || '',
      code: item.code || '',
      barcode: item.barcode || '',
      standardCost: String(item.standardCost || ''),
      standardPrice: String(item.standardPrice || ''),
      reorderLevel: String(item.reorderLevel || ''),
    });
    setShowModal(true);
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      const data = {
        name: formData.name,
        code: formData.code,
        barcode: formData.barcode,
        standardCost: formData.standardCost ? Number(formData.standardCost) : undefined,
        standardPrice: formData.standardPrice ? Number(formData.standardPrice) : undefined,
        reorderLevel: formData.reorderLevel ? Number(formData.reorderLevel) : undefined,
      };
      if (editingItem) {
        await stockItemsApi.update(editingItem.id, data);
      } else {
        await stockItemsApi.create(data);
      }
      setShowModal(false);
      fetchItems();
    } catch (err: any) {
      alert(err.message || 'Failed to save');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await stockItemsApi.delete(id);
      setDeleteConfirm(null);
      fetchItems();
    } catch (err: any) {
      alert(err.message || 'Failed to delete');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Inventory Management</h1>
          <p className="text-slate-500 dark:text-slate-400 mt-1">Manage stock items, warehouses, and inventory levels</p>
        </div>
        <button onClick={openCreate} className="flex items-center gap-2 px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg transition">
          <Plus className="w-4 h-4" /> Add Item
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-white dark:bg-slate-800 rounded-xl p-5 border border-slate-200 dark:border-slate-700">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-green-500"><Package className="w-5 h-5 text-white" /></div>
            <div><p className="text-2xl font-bold text-slate-900 dark:text-white">{totalCount}</p><p className="text-sm text-slate-500">Stock Items</p></div>
          </div>
        </div>
        <div className="bg-white dark:bg-slate-800 rounded-xl p-5 border border-slate-200 dark:border-slate-700">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-blue-500"><WarehouseIcon className="w-5 h-5 text-white" /></div>
            <div><p className="text-2xl font-bold text-slate-900 dark:text-white">{warehouses.length}</p><p className="text-sm text-slate-500">Warehouses</p></div>
          </div>
        </div>
        <div className="bg-white dark:bg-slate-800 rounded-xl p-5 border border-slate-200 dark:border-slate-700">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-orange-500"><Package className="w-5 h-5 text-white" /></div>
            <div><p className="text-2xl font-bold text-slate-900 dark:text-white">{items.filter(i => i.standardCost && i.standardCost > 0).length}</p><p className="text-sm text-slate-500">Items with Price</p></div>
          </div>
        </div>
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 overflow-hidden">
        <div className="p-4 border-b border-slate-200 dark:border-slate-700 flex gap-3">
          <div className="relative flex-1 max-w-sm">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input
              type="text"
              placeholder="Search items..."
              value={search}
              onChange={(e) => { setSearch(e.target.value); setPage(1); }}
              className="w-full pl-10 pr-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white placeholder-slate-400 focus:ring-2 focus:ring-green-500"
            />
          </div>
        </div>

        {loading ? (
          <div className="flex items-center justify-center h-48"><Loader2 className="w-8 h-8 animate-spin text-green-600" /></div>
        ) : error ? (
          <div className="p-6 text-red-500">{error}</div>
        ) : items.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-48 text-slate-400">
            <Package className="w-12 h-12 mb-2 opacity-50" />
            <p>No items found</p>
            <button onClick={openCreate} className="mt-3 text-green-600 hover:underline">Add your first item</button>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-slate-50 dark:bg-slate-700/50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Code</th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Name</th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Barcode</th>
                    <th className="px-4 py-3 text-right text-xs font-semibold text-slate-500 uppercase">Standard Cost</th>
                    <th className="px-4 py-3 text-right text-xs font-semibold text-slate-500 uppercase">Standard Price</th>
                    <th className="px-4 py-3 text-center text-xs font-semibold text-slate-500 uppercase">Reorder Level</th>
                    <th className="px-4 py-3 text-center text-xs font-semibold text-slate-500 uppercase">Status</th>
                    <th className="px-4 py-3 text-right text-xs font-semibold text-slate-500 uppercase">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                  {items.map((item) => (
                    <tr key={item.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/30">
                      <td className="px-4 py-3 font-mono text-sm text-slate-700 dark:text-slate-300">{item.code}</td>
                      <td className="px-4 py-3 font-medium text-slate-900 dark:text-white">{item.name}</td>
                      <td className="px-4 py-3 text-slate-600 dark:text-slate-400 text-sm font-mono">{item.barcode || '-'}</td>
                      <td className="px-4 py-3 text-right text-slate-600 dark:text-slate-400">{item.standardCost ? `$${Number(item.standardCost).toFixed(2)}` : '-'}</td>
                      <td className="px-4 py-3 text-right text-slate-600 dark:text-slate-400">{item.standardPrice ? `$${Number(item.standardPrice).toFixed(2)}` : '-'}</td>
                      <td className="px-4 py-3 text-center text-slate-600 dark:text-slate-400">{item.reorderLevel ?? '-'}</td>
                      <td className="px-4 py-3 text-center">
                        <span className={`px-2 py-1 text-xs font-medium rounded-full ${item.isActive !== false ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400' : 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400'}`}>
                          {item.isActive !== false ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-right">
                        <button onClick={() => openEdit(item)} className="p-1.5 text-slate-400 hover:text-green-600 hover:bg-green-50 rounded"><Edit2 className="w-4 h-4" /></button>
                        <button onClick={() => setDeleteConfirm(item.id)} className="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded ml-1"><Trash2 className="w-4 h-4" /></button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="p-4 border-t border-slate-200 dark:border-slate-700 flex items-center justify-between">
              <p className="text-sm text-slate-500">Showing {(page - 1) * pageSize + 1} to {Math.min(page * pageSize, totalCount)} of {totalCount}</p>
              <div className="flex items-center gap-2">
                <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page <= 1} className="p-2 rounded-lg border border-slate-300 dark:border-slate-600 disabled:opacity-50 hover:bg-slate-50"><ChevronLeft className="w-4 h-4" /></button>
                <span className="text-sm font-medium px-3">{page} / {totalPages || 1}</span>
                <button onClick={() => setPage(p => Math.min(totalPages, p + 1))} disabled={page >= totalPages} className="p-2 rounded-lg border border-slate-300 dark:border-slate-600 disabled:opacity-50 hover:bg-slate-50"><ChevronRight className="w-4 h-4" /></button>
              </div>
            </div>
          </>
        )}
      </div>

      {/* Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className="bg-white dark:bg-slate-800 rounded-xl shadow-xl w-full max-w-md mx-4">
            <div className="flex items-center justify-between p-5 border-b border-slate-200 dark:border-slate-700">
              <h3 className="text-lg font-semibold text-slate-900 dark:text-white">{editingItem ? 'Edit Item' : 'Add Stock Item'}</h3>
              <button onClick={() => setShowModal(false)} className="p-1 hover:bg-slate-100 rounded"><X className="w-5 h-5" /></button>
            </div>
            <div className="p-5 space-y-4">
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Name *</label>
                <input type="text" value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" required />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Code *</label>
                  <input type="text" value={formData.code} onChange={(e) => setFormData({ ...formData, code: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white font-mono" required />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Barcode</label>
                  <input type="text" value={formData.barcode} onChange={(e) => setFormData({ ...formData, barcode: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white font-mono" />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Standard Cost</label>
                  <input type="number" step="0.01" value={formData.standardCost} onChange={(e) => setFormData({ ...formData, standardCost: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Standard Price</label>
                  <input type="number" step="0.01" value={formData.standardPrice} onChange={(e) => setFormData({ ...formData, standardPrice: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Reorder Level</label>
                <input type="number" value={formData.reorderLevel} onChange={(e) => setFormData({ ...formData, reorderLevel: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" />
              </div>
            </div>
            <div className="p-5 border-t border-slate-200 dark:border-slate-700 flex gap-3 justify-end">
              <button onClick={() => setShowModal(false)} className="px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg hover:bg-slate-50">Cancel</button>
              <button onClick={handleSave} disabled={saving || !formData.name || !formData.code} className="px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg disabled:opacity-50 flex items-center gap-2">
                {saving && <Loader2 className="w-4 h-4 animate-spin" />}
                {editingItem ? 'Update' : 'Create'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Delete Confirm */}
      {deleteConfirm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className="bg-white dark:bg-slate-800 rounded-xl shadow-xl w-full max-w-sm mx-4 p-6">
            <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-2">Delete Item?</h3>
            <p className="text-slate-500 text-sm mb-4">This action cannot be undone.</p>
            <div className="flex gap-3 justify-end">
              <button onClick={() => setDeleteConfirm(null)} className="px-4 py-2 border border-slate-300 rounded-lg">Cancel</button>
              <button onClick={() => handleDelete(deleteConfirm)} className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg">Delete</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
