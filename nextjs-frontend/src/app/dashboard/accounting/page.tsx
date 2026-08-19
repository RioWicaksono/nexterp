'use client';

import { useEffect, useState, useCallback } from 'react';
import { accountsApi, journalEntriesApi, type AccountDto, type JournalEntryDto } from '@/lib/api';
import { PageHeader } from '@/components/PageHeader';
import { SkeletonLoader } from '@/components/SkeletonLoader';
import { ConfirmDialog } from '@/components/ConfirmDialog';
import { useToast } from '@/hooks/useToast';
import { Plus, Search, Edit2, Trash2, X, Loader2, ChevronLeft, ChevronRight, DollarSign, FileText, BookOpen } from 'lucide-react';

const PAGE_SIZE_OPTIONS = [10, 25, 50];

export default function AccountingPage() {
  const [activeTab, setActiveTab] = useState<'accounts' | 'journals'>('accounts');
  const [accounts, setAccounts] = useState<AccountDto[]>([]);
  const [journals, setJournals] = useState<JournalEntryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [showModal, setShowModal] = useState(false);
  const [editingAccount, setEditingAccount] = useState<AccountDto | null>(null);
  const [formData, setFormData] = useState({ accountCode: '', name: '', accountType: 'Asset', class: 'Debit', openingBalance: '' });
  const [saving, setSaving] = useState(false);
  const [deleteConfirm, setDeleteConfirm] = useState<{ isOpen: boolean; accountId: string | null; accountName: string }>({
    isOpen: false,
    accountId: null,
    accountName: '',
  });

  const toast = useToast();

  const fetchAccounts = useCallback(async () => {
    setLoading(true);
    try {
      const result = await accountsApi.getAll({ page, pageSize, search: search || undefined });
      if (result?.success && result.data) {
        setAccounts(result.data.items || []);
        setTotalCount(result.data.totalCount || 0);
        setTotalPages(Math.ceil((result.data.totalCount || 0) / pageSize));
        setError(null);
      } else {
        setAccounts([]);
        setTotalCount(0);
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to load accounts';
      setError(msg);
      toast('error', 'Error', msg);
      setAccounts([]);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, toast]);

  const fetchJournals = useCallback(async () => {
    setLoading(true);
    try {
      const result = await journalEntriesApi.getAll({ page, pageSize, search: search || undefined });
      if (result?.success && result.data) {
        setJournals(result.data.items || []);
        setTotalCount(result.data.totalCount || 0);
        setTotalPages(Math.ceil((result.data.totalCount || 0) / pageSize));
        setError(null);
      } else {
        setJournals([]);
        setTotalCount(0);
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to load journal entries';
      setError(msg);
      toast('error', 'Error', msg);
      setJournals([]);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, toast]);

  useEffect(() => {
    if (activeTab === 'accounts') fetchAccounts();
    else fetchJournals();
  }, [activeTab, fetchAccounts, fetchJournals]);

  // Escape key to close modal
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && showModal) {
        setShowModal(false);
      }
    };
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [showModal]);

  const openCreate = () => {
    setEditingAccount(null);
    setFormData({ accountCode: '', name: '', accountType: 'Asset', class: 'Debit', openingBalance: '' });
    setShowModal(true);
  };

  const openEdit = (acc: AccountDto) => {
    setEditingAccount(acc);
    setFormData({
      accountCode: acc.accountCode || '',
      name: acc.name || '',
      accountType: acc.accountType || 'Asset',
      class: acc.class || 'Debit',
      openingBalance: String(acc.openingBalance || ''),
    });
    setShowModal(true);
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      const data = {
        accountCode: formData.accountCode,
        name: formData.name,
        accountType: formData.accountType,
        class: formData.class,
        openingBalance: formData.openingBalance ? Number(formData.openingBalance) : 0,
      };
      if (editingAccount) {
        await accountsApi.update(editingAccount.id, data);
        toast('success', 'Updated!', 'Account has been updated');
      } else {
        await accountsApi.create(data);
        toast('success', 'Created!', 'Account has been created');
      }
      setShowModal(false);
      fetchAccounts();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to save account';
      toast('error', 'Error', msg);
    } finally {
      setSaving(false);
    }
  };

  const confirmDelete = (acc: AccountDto) => {
    setDeleteConfirm({ isOpen: true, accountId: acc.id, accountName: acc.name || 'this account' });
  };

  const handleDelete = async () => {
    if (!deleteConfirm.accountId) return;
    try {
      await accountsApi.delete(deleteConfirm.accountId);
      toast('success', 'Deleted!', 'Account has been removed');
      setDeleteConfirm({ isOpen: false, accountId: null, accountName: '' });
      fetchAccounts();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to delete account';
      toast('error', 'Error', msg);
    }
  };

  const handlePageSizeChange = (newSize: number) => {
    setPageSize(newSize);
    setPage(1);
  };

  const handleTabChange = (tab: 'accounts' | 'journals') => {
    setActiveTab(tab);
    setPage(1);
    setSearch('');
  };

  const accountTypeColors: Record<string, string> = {
    Asset: 'bg-blue-100 text-blue-700',
    Liability: 'bg-red-100 text-red-700',
    Equity: 'bg-purple-100 text-purple-700',
    Revenue: 'bg-green-100 text-green-700',
    Expense: 'bg-orange-100 text-orange-700',
  };

  return (
    <div className="space-y-6">
      <PageHeader
        title="Accounting"
        subtitle="Chart of accounts and journal entries"
        breadcrumbs={[
          { label: 'Dashboard', href: '/dashboard' },
          { label: 'Accounting' },
        ]}
        actions={
          activeTab === 'accounts' ? (
            <button onClick={openCreate} className="flex items-center gap-2 px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg transition">
              <Plus className="w-4 h-4" /> Add Account
            </button>
          ) : undefined
        }
      />

      {/* Tabs */}
      <div className="flex gap-1 bg-slate-100 dark:bg-slate-700 p-1 rounded-lg w-fit">
        <button onClick={() => handleTabChange('accounts')} className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition ${activeTab === 'accounts' ? 'bg-white dark:bg-slate-600 text-slate-900 dark:text-white shadow-sm' : 'text-slate-500 hover:text-slate-700 dark:hover:text-slate-300'}`}>
          <BookOpen className="w-4 h-4" /> Chart of Accounts
        </button>
        <button onClick={() => handleTabChange('journals')} className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition ${activeTab === 'journals' ? 'bg-white dark:bg-slate-600 text-slate-900 dark:text-white shadow-sm' : 'text-slate-500 hover:text-slate-700 dark:hover:text-slate-300'}`}>
          <FileText className="w-4 h-4" /> Journal Entries
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
        {['Asset', 'Liability', 'Equity', 'Revenue', 'Expense'].map((type) => (
          <div key={type} className="bg-white dark:bg-slate-800 rounded-xl p-4 border border-slate-200 dark:border-slate-700">
            <div className="flex items-center gap-2 mb-1">
              <div className={`w-2 h-2 rounded-full ${accountTypeColors[type]?.replace('bg-', 'bg-').replace('-100', '-500')}`} />
              <span className="text-xs font-medium text-slate-500 uppercase tracking-wide">{type}</span>
            </div>
            <p className="text-2xl font-bold text-slate-900 dark:text-white">{accounts.filter(a => a.accountType === type).length}</p>
          </div>
        ))}
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 overflow-hidden">
        <div className="p-4 border-b border-slate-200 dark:border-slate-700 flex gap-3">
          <div className="relative flex-1 max-w-sm">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input
              type="text"
              placeholder={activeTab === 'accounts' ? 'Search accounts...' : 'Search journals...'}
              value={search}
              onChange={(e) => { setSearch(e.target.value); setPage(1); }}
              className="w-full pl-10 pr-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white placeholder-slate-400 focus:ring-2 focus:ring-purple-500"
            />
          </div>
        </div>

        {/* Accounts Table */}
        {activeTab === 'accounts' && (
          loading ? (
            <div className="p-6">
              <SkeletonLoader rows={5} height="h-12" />
            </div>
          ) : error ? (
            <div className="p-6 text-red-500">{error}</div>
          ) : accounts.length === 0 ? (
            <div className="flex flex-col items-center justify-center h-48 text-slate-400">
              <BookOpen className="w-12 h-12 mb-2 opacity-50" />
              <p>No accounts found</p>
              <button onClick={openCreate} className="mt-3 text-purple-600 hover:underline">Add your first account</button>
            </div>
          ) : (
            <>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-slate-50 dark:bg-slate-700/50">
                    <tr>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Code</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Account Name</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Type</th>
                      <th className="px-4 py-3 text-center text-xs font-semibold text-slate-500 uppercase">Class</th>
                      <th className="px-4 py-3 text-right text-xs font-semibold text-slate-500 uppercase">Opening Balance</th>
                      <th className="px-4 py-3 text-right text-xs font-semibold text-slate-500 uppercase">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                    {accounts.map((acc) => (
                      <tr key={acc.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/30 transition-colors">
                        <td className="px-4 py-3 font-mono text-sm font-medium text-slate-900 dark:text-white">{acc.accountCode}</td>
                        <td className="px-4 py-3 font-medium text-slate-900 dark:text-white">{acc.name}</td>
                        <td className="px-4 py-3">
                          <span className={`px-2 py-1 text-xs font-medium rounded-full ${accountTypeColors[acc.accountType || 'Asset'] || 'bg-slate-100 text-slate-700'}`}>{acc.accountType || 'Asset'}</span>
                        </td>
                        <td className="px-4 py-3 text-center text-sm text-slate-600 dark:text-slate-400">{acc.class || 'Debit'}</td>
                        <td className="px-4 py-3 text-right font-mono text-slate-600 dark:text-slate-400">{acc.openingBalance ? Number(acc.openingBalance).toFixed(2) : '0.00'}</td>
                        <td className="px-4 py-3 text-right">
                          <button onClick={() => openEdit(acc)} className="p-1.5 text-slate-400 hover:text-purple-600 hover:bg-purple-50 rounded transition-colors"><Edit2 className="w-4 h-4" /></button>
                          <button onClick={() => confirmDelete(acc)} className="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded transition-colors ml-1"><Trash2 className="w-4 h-4" /></button>
                        </td>
                      </tr>
                    ))}
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
          )
        )}

        {/* Journals Table */}
        {activeTab === 'journals' && (
          loading ? (
            <div className="p-6">
              <SkeletonLoader rows={5} height="h-12" />
            </div>
          ) : error ? (
            <div className="p-6 text-red-500">{error}</div>
          ) : journals.length === 0 ? (
            <div className="flex flex-col items-center justify-center h-48 text-slate-400">
              <FileText className="w-12 h-12 mb-2 opacity-50" />
              <p>No journal entries found</p>
            </div>
          ) : (
            <>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-slate-50 dark:bg-slate-700/50">
                    <tr>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Entry #</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Date</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Title</th>
                      <th className="px-4 py-3 text-center text-xs font-semibold text-slate-500 uppercase">Status</th>
                      <th className="px-4 py-3 text-right text-xs font-semibold text-slate-500 uppercase">Total Debit</th>
                      <th className="px-4 py-3 text-right text-xs font-semibold text-slate-500 uppercase">Total Credit</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                    {journals.map((j) => (
                      <tr key={j.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/30 transition-colors">
                        <td className="px-4 py-3 font-mono text-sm font-medium text-slate-900 dark:text-white">{j.entryNumber || j.id.slice(0, 8)}</td>
                        <td className="px-4 py-3 text-slate-600 dark:text-slate-400 text-sm">{j.entryDate ? new Date(j.entryDate).toLocaleDateString() : '-'}</td>
                        <td className="px-4 py-3 font-medium text-slate-900 dark:text-white">{j.title || '-'}</td>
                        <td className="px-4 py-3 text-center">
                          <span className={`px-2 py-1 text-xs font-medium rounded-full ${j.status === 'Posted' ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400' : 'bg-slate-100 text-slate-700 dark:bg-slate-700 dark:text-slate-300'}`}>{j.status || 'Draft'}</span>
                        </td>
                        <td className="px-4 py-3 text-right font-mono text-slate-600 dark:text-slate-400">{j.totalDebit ? Number(j.totalDebit).toFixed(2) : '0.00'}</td>
                        <td className="px-4 py-3 text-right font-mono text-slate-600 dark:text-slate-400">{j.totalCredit ? Number(j.totalCredit).toFixed(2) : '0.00'}</td>
                      </tr>
                    ))}
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
          )
        )}
      </div>

      {/* Account Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50" onClick={(e) => e.target === e.currentTarget && setShowModal(false)}>
          <div className="bg-white dark:bg-slate-800 rounded-xl shadow-xl w-full max-w-md mx-4">
            <div className="flex items-center justify-between p-5 border-b border-slate-200 dark:border-slate-700">
              <h3 className="text-lg font-semibold text-slate-900 dark:text-white">{editingAccount ? 'Edit Account' : 'Add Account'}</h3>
              <button onClick={() => setShowModal(false)} className="p-1 hover:bg-slate-100 rounded transition-colors"><X className="w-5 h-5" /></button>
            </div>
            <div className="p-5 space-y-4">
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Account Code *</label>
                <input type="text" value={formData.accountCode} onChange={(e) => setFormData({ ...formData, accountCode: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white font-mono" required />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Account Name *</label>
                <input type="text" value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" required />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Type</label>
                  <select value={formData.accountType} onChange={(e) => setFormData({ ...formData, accountType: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white">
                    {['Asset', 'Liability', 'Equity', 'Revenue', 'Expense'].map(t => <option key={t} value={t}>{t}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Class</label>
                  <select value={formData.class} onChange={(e) => setFormData({ ...formData, class: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white">
                    <option value="Debit">Debit</option>
                    <option value="Credit">Credit</option>
                  </select>
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Opening Balance</label>
                <input type="number" step="0.01" value={formData.openingBalance} onChange={(e) => setFormData({ ...formData, openingBalance: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" />
              </div>
            </div>
            <div className="p-5 border-t border-slate-200 dark:border-slate-700 flex gap-3 justify-end">
              <button onClick={() => setShowModal(false)} className="px-4 py-2 border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors">Cancel</button>
              <button onClick={handleSave} disabled={saving || !formData.accountCode || !formData.name} className="px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg disabled:opacity-50 flex items-center gap-2 transition-colors">
                {saving && <Loader2 className="w-4 h-4 animate-spin" />}
                {editingAccount ? 'Update' : 'Create'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Delete Confirmation Dialog */}
      <ConfirmDialog
        isOpen={deleteConfirm.isOpen}
        title="Delete Account?"
        message={`Are you sure you want to delete account "${deleteConfirm.accountName}"? This action cannot be undone.`}
        confirmText="Delete"
        cancelText="Cancel"
        onConfirm={handleDelete}
        onCancel={() => setDeleteConfirm({ isOpen: false, accountId: null, accountName: '' })}
        variant="danger"
      />
    </div>
  );
}
