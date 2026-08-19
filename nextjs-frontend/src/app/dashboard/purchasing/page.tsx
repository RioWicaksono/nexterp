'use client';

import { useEffect, useState, useCallback } from 'react';
import { purchaseOrdersApi, suppliersApi, type PurchaseOrderDto, type SupplierDto } from '@/lib/api';
import { PageHeader } from '@/components/PageHeader';
import { SkeletonLoader } from '@/components/SkeletonLoader';
import { ConfirmDialog } from '@/components/ConfirmDialog';
import { useToast } from '@/hooks/useToast';
import { Plus, Search, X, Loader2, ChevronLeft, ChevronRight, ShoppingCart, CheckCircle, XCircle, Clock, Truck, Building2 } from 'lucide-react';

const PAGE_SIZE_OPTIONS = [10, 25, 50];

export default function PurchasingPage() {
  const [orders, setOrders] = useState<PurchaseOrderDto[]>([]);
  const [suppliers, setSuppliers] = useState<SupplierDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [showModal, setShowModal] = useState(false);
  const [showSupplierModal, setShowSupplierModal] = useState(false);
  const [formData, setFormData] = useState({ supplierId: '', expectedDeliveryDate: '' });
  const [supplierForm, setSupplierForm] = useState({ supplierName: '', email: '', phone: '' });
  const [saving, setSaving] = useState(false);
  const [cancelConfirm, setCancelConfirm] = useState<{ isOpen: boolean; orderId: string | null; orderNumber: string }>({
    isOpen: false,
    orderId: null,
    orderNumber: '',
  });

  const toast = useToast();

  const fetchOrders = useCallback(async () => {
    setLoading(true);
    try {
      const result = await purchaseOrdersApi.getAll({ page, pageSize, search: search || undefined });
      if (result?.success && result.data) {
        setOrders(result.data.items || []);
        setTotalCount(result.data.totalCount || 0);
        setTotalPages(Math.ceil((result.data.totalCount || 0) / pageSize));
        setError(null);
      } else {
        setOrders([]);
        setTotalCount(0);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load orders');
      toast('error', 'Error', err.message || 'Failed to load purchase orders');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, toast]);

  const fetchSuppliers = useCallback(async () => {
    try {
      const result = await suppliersApi.getAll({ pageSize: 100 });
      if (result?.success && result.data) {
        setSuppliers(result.data.items || []);
      }
    } catch {}
  }, []);

  useEffect(() => { fetchOrders(); }, [fetchOrders]);
  useEffect(() => { fetchSuppliers(); }, [fetchSuppliers]);

  // Escape key to close modals
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        if (showModal) setShowModal(false);
        if (showSupplierModal) setShowSupplierModal(false);
      }
    };
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [showModal, showSupplierModal]);

  const handleCreate = async () => {
    setSaving(true);
    try {
      await purchaseOrdersApi.create({ supplierId: formData.supplierId, expectedDeliveryDate: formData.expectedDeliveryDate || undefined });
      toast('success', 'Created!', 'Purchase order has been created');
      setShowModal(false);
      fetchOrders();
    } catch (err: any) {
      toast('error', 'Error', err.message || 'Failed to create order');
    } finally {
      setSaving(false);
    }
  };

  const handleSupplierCreate = async () => {
    setSaving(true);
    try {
      await suppliersApi.create({ supplierName: supplierForm.supplierName, email: supplierForm.email, phone: supplierForm.phone });
      toast('success', 'Created!', 'Supplier has been added');
      setShowSupplierModal(false);
      fetchSuppliers();
    } catch (err: any) {
      toast('error', 'Error', err.message || 'Failed to create supplier');
    } finally {
      setSaving(false);
    }
  };

  const handleAction = async (id: string, action: 'submit' | 'approve' | 'cancel', orderNumber: string) => {
    try {
      if (action === 'cancel') {
        setCancelConfirm({ isOpen: true, orderId: id, orderNumber });
        return;
      }
      await (purchaseOrdersApi as any)[action](id);
      toast('success', 'Updated!', `Order has been ${action === 'submit' ? 'submitted' : 'approved'}`);
      fetchOrders();
    } catch (err: any) {
      toast('error', 'Error', err.message || `Failed to ${action}`);
    }
  };

  const confirmCancel = () => {
    if (!cancelConfirm.orderId) return;
    (async () => {
      try {
        await (purchaseOrdersApi as any).cancel(cancelConfirm.orderId);
        toast('success', 'Cancelled!', 'Purchase order has been cancelled');
        setCancelConfirm({ isOpen: false, orderId: null, orderNumber: '' });
        fetchOrders();
      } catch (err: any) {
        toast('error', 'Error', err.message || 'Failed to cancel order');
      }
    })();
  };

  const handlePageSizeChange = (newSize: number) => {
    setPageSize(newSize);
    setPage(1);
  };

  const openNewPOModal = () => {
    setFormData({ supplierId: '', expectedDeliveryDate: '' });
    setShowModal(true);
  };

  const openSupplierModal = () => {
    setSupplierForm({ supplierName: '', email: '', phone: '' });
    setShowSupplierModal(true);
  };

  const statusConfig: Record<string, { label: string; color: string; icon: any }> = {
    Draft: { label: 'Draft', color: 'bg-slate-100 text-slate-700 dark:bg-slate-700 dark:text-slate-300', icon: Clock },
    Submitted: { label: 'Submitted', color: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400', icon: Clock },
    Approved: { label: 'Approved', color: 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400', icon: CheckCircle },
    Cancelled: { label: 'Cancelled', color: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400', icon: XCircle },
    Received: { label: 'Received', color: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400', icon: Truck },
  };

  return (
    <div className="space-y-6">
      <PageHeader
        title="Purchasing"
        subtitle="Manage purchase orders and suppliers"
        breadcrumbs={[
          { label: 'Dashboard', href: '/dashboard' },
          { label: 'Purchasing' },
        ]}
        actions={
          <div className="flex gap-2">
            <button onClick={openSupplierModal} className="flex items-center gap-2 px-4 py-2 border border-orange-300 dark:border-orange-700 text-orange-600 dark:text-orange-400 hover:bg-orange-50 dark:hover:bg-orange-900/20 rounded-lg transition">
              <Building2 className="w-4 h-4" /> Add Supplier
            </button>
            <button onClick={openNewPOModal} className="flex items-center gap-2 px-4 py-2 bg-orange-600 hover:bg-orange-700 text-white rounded-lg transition">
              <Plus className="w-4 h-4" /> New PO
            </button>
          </div>
        }
      />

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white dark:bg-slate-800 rounded-xl p-5 border border-slate-200 dark:border-slate-700">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-orange-500"><ShoppingCart className="w-5 h-5 text-white" /></div>
            <div><p className="text-2xl font-bold text-slate-900 dark:text-white">{totalCount}</p><p className="text-sm text-slate-500">Total Orders</p></div>
          </div>
        </div>
        <div className="bg-white dark:bg-slate-800 rounded-xl p-5 border border-slate-200 dark:border-slate-700">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-blue-500"><Clock className="w-5 h-5 text-white" /></div>
            <div><p className="text-2xl font-bold text-slate-900 dark:text-white">{orders.filter(o => o.status === 'Draft' || o.status === 'Submitted').length}</p><p className="text-sm text-slate-500">Pending</p></div>
          </div>
        </div>
        <div className="bg-white dark:bg-slate-800 rounded-xl p-5 border border-slate-200 dark:border-slate-700">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-green-500"><CheckCircle className="w-5 h-5 text-white" /></div>
            <div><p className="text-2xl font-bold text-slate-900 dark:text-white">{orders.filter(o => o.status === 'Approved' || o.status === 'Received').length}</p><p className="text-sm text-slate-500">Approved</p></div>
          </div>
        </div>
        <div className="bg-white dark:bg-slate-800 rounded-xl p-5 border border-slate-200 dark:border-slate-700">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-purple-500"><Building2 className="w-5 h-5 text-white" /></div>
            <div><p className="text-2xl font-bold text-slate-900 dark:text-white">{suppliers.length}</p><p className="text-sm text-slate-500">Suppliers</p></div>
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
              placeholder="Search orders..."
              value={search}
              onChange={(e) => { setSearch(e.target.value); setPage(1); }}
              className="w-full pl-10 pr-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white placeholder-slate-400 focus:ring-2 focus:ring-orange-500"
            />
          </div>
        </div>

        {loading ? (
          <div className="p-6">
            <SkeletonLoader rows={5} height="h-12" />
          </div>
        ) : error ? (
          <div className="p-6 text-red-500">{error}</div>
        ) : orders.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-48 text-slate-400">
            <ShoppingCart className="w-12 h-12 mb-2 opacity-50" />
            <p>No purchase orders found</p>
            <button onClick={openNewPOModal} className="mt-3 text-orange-600 hover:underline">Create your first PO</button>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-slate-50 dark:bg-slate-700/50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">PO Number</th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Supplier</th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Order Date</th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Expected Delivery</th>
                    <th className="px-4 py-3 text-right text-xs font-semibold text-slate-500 uppercase">Total Amount</th>
                    <th className="px-4 py-3 text-center text-xs font-semibold text-slate-500 uppercase">Status</th>
                    <th className="px-4 py-3 text-center text-xs font-semibold text-slate-500 uppercase">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                  {orders.map((order) => {
                    const config = statusConfig[order.status || 'Draft'] || statusConfig['Draft'];
                    const orderNumber = order.orderNumber || order.id.slice(0, 8);
                    return (
                      <tr key={order.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/30 transition-colors">
                        <td className="px-4 py-3 font-mono text-sm font-medium text-slate-900 dark:text-white">{orderNumber}</td>
                        <td className="px-4 py-3 text-slate-600 dark:text-slate-400">{order.supplierName || '-'}</td>
                        <td className="px-4 py-3 text-slate-600 dark:text-slate-400 text-sm">{order.orderDate ? new Date(order.orderDate).toLocaleDateString() : '-'}</td>
                        <td className="px-4 py-3 text-slate-600 dark:text-slate-400 text-sm">{order.expectedDeliveryDate ? new Date(order.expectedDeliveryDate).toLocaleDateString() : '-'}</td>
                        <td className="px-4 py-3 text-right font-semibold text-slate-900 dark:text-white">{order.totalAmount ? `$${Number(order.totalAmount).toFixed(2)}` : '-'}</td>
                        <td className="px-4 py-3 text-center">
                          <span className={`inline-flex items-center gap-1 px-2 py-1 text-xs font-medium rounded-full ${config.color}`}>
                            <config.icon className="w-3 h-3" /> {config.label}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-center">
                          <div className="flex items-center justify-center gap-1">
                            {(order.status === 'Draft') && (
                              <button onClick={() => handleAction(order.id, 'submit', orderNumber)} className="px-2 py-1 text-xs bg-blue-100 text-blue-700 rounded hover:bg-blue-200 transition-colors">Submit</button>
                            )}
                            {(order.status === 'Submitted') && (
                              <button onClick={() => handleAction(order.id, 'approve', orderNumber)} className="px-2 py-1 text-xs bg-green-100 text-green-700 rounded hover:bg-green-200 transition-colors">Approve</button>
                            )}
                            {(order.status === 'Draft' || order.status === 'Submitted') && (
                              <button onClick={() => handleAction(order.id, 'cancel', orderNumber)} className="px-2 py-1 text-xs bg-red-100 text-red-700 rounded hover:bg-red-200 transition-colors">Cancel</button>
                            )}
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>

            {/* Pagination with Size Selector */}
            <div className="p-4 border-t border-slate-200 dark:border-slate-700 flex flex-col sm:flex-row items-center justify-between gap-4">
              <div className="flex items-center gap-2 text-sm text-slate-500">
                <span>Show</span>
                <select
                  value={pageSize}
                  onChange={(e) => handlePageSizeChange(Number(e.target.value))}
                  className="px-2 py-1 border border-slate-300 dark:border-slate-600 rounded bg-white dark:bg-slate-700 text-slate-900 dark:text-white"
                >
                  {PAGE_SIZE_OPTIONS.map((size) => (
                    <option key={size} value={size}>{size}</option>
                  ))}
                </select>
                <span>of {totalCount}</span>
              </div>
              <div className="flex items-center gap-2">
                <p className="text-sm text-slate-500 mr-2">
                  {(page - 1) * pageSize + 1} - {Math.min(page * pageSize, totalCount)} of {totalCount}
                </p>
                <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page <= 1} className="p-2 rounded-lg border border-slate-300 disabled:opacity-50 hover:bg-slate-50 transition-colors"><ChevronLeft className="w-4 h-4" /></button>
                <span className="text-sm font-medium px-3">{page} / {totalPages || 1}</span>
                <button onClick={() => setPage(p => Math.min(totalPages, p + 1))} disabled={page >= totalPages} className="p-2 rounded-lg border border-slate-300 disabled:opacity-50 hover:bg-slate-50 transition-colors"><ChevronRight className="w-4 h-4" /></button>
              </div>
            </div>
          </>
        )}
      </div>

      {/* Create PO Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50" onClick={(e) => e.target === e.currentTarget && setShowModal(false)}>
          <div className="bg-white dark:bg-slate-800 rounded-xl shadow-xl w-full max-w-md mx-4">
            <div className="flex items-center justify-between p-5 border-b border-slate-200">
              <h3 className="text-lg font-semibold text-slate-900 dark:text-white">New Purchase Order</h3>
              <button onClick={() => setShowModal(false)} className="p-1 hover:bg-slate-100 rounded transition-colors"><X className="w-5 h-5" /></button>
            </div>
            <div className="p-5 space-y-4">
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Supplier *</label>
                <select value={formData.supplierId} onChange={(e) => setFormData({ ...formData, supplierId: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" required>
                  <option value="">Select Supplier</option>
                  {suppliers.map((s) => <option key={s.id} value={s.id}>{s.supplierName}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Expected Delivery Date</label>
                <input type="date" value={formData.expectedDeliveryDate} onChange={(e) => setFormData({ ...formData, expectedDeliveryDate: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" />
              </div>
            </div>
            <div className="p-5 border-t border-slate-200 flex gap-3 justify-end">
              <button onClick={() => setShowModal(false)} className="px-4 py-2 border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors">Cancel</button>
              <button onClick={handleCreate} disabled={saving || !formData.supplierId} className="px-4 py-2 bg-orange-600 hover:bg-orange-700 text-white rounded-lg disabled:opacity-50 flex items-center gap-2 transition-colors">
                {saving && <Loader2 className="w-4 h-4 animate-spin" />}Create PO
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Add Supplier Modal */}
      {showSupplierModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50" onClick={(e) => e.target === e.currentTarget && setShowSupplierModal(false)}>
          <div className="bg-white dark:bg-slate-800 rounded-xl shadow-xl w-full max-w-md mx-4">
            <div className="flex items-center justify-between p-5 border-b border-slate-200">
              <h3 className="text-lg font-semibold text-slate-900 dark:text-white">Add Supplier</h3>
              <button onClick={() => setShowSupplierModal(false)} className="p-1 hover:bg-slate-100 rounded transition-colors"><X className="w-5 h-5" /></button>
            </div>
            <div className="p-5 space-y-4">
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Supplier Name *</label>
                <input type="text" value={supplierForm.supplierName} onChange={(e) => setSupplierForm({ ...supplierForm, supplierName: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" required />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Email</label>
                <input type="email" value={supplierForm.email} onChange={(e) => setSupplierForm({ ...supplierForm, email: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Phone</label>
                <input type="text" value={supplierForm.phone} onChange={(e) => setSupplierForm({ ...supplierForm, phone: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" />
              </div>
            </div>
            <div className="p-5 border-t border-slate-200 flex gap-3 justify-end">
              <button onClick={() => setShowSupplierModal(false)} className="px-4 py-2 border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors">Cancel</button>
              <button onClick={handleSupplierCreate} disabled={saving || !supplierForm.supplierName} className="px-4 py-2 bg-orange-600 hover:bg-orange-700 text-white rounded-lg disabled:opacity-50 flex items-center gap-2 transition-colors">
                {saving && <Loader2 className="w-4 h-4 animate-spin" />}Create
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Cancel Confirmation Dialog */}
      <ConfirmDialog
        isOpen={cancelConfirm.isOpen}
        title="Cancel Order?"
        message={`Are you sure you want to cancel purchase order ${cancelConfirm.orderNumber}? This action cannot be undone.`}
        confirmText="Cancel Order"
        cancelText="Keep Order"
        onConfirm={confirmCancel}
        onCancel={() => setCancelConfirm({ isOpen: false, orderId: null, orderNumber: '' })}
        variant="danger"
      />
    </div>
  );
}
