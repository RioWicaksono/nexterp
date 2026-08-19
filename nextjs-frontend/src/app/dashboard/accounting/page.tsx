'use client';

import { useEffect, useState, useCallback } from 'react';
import { accountsApi, journalEntriesApi, type AccountDto, type JournalEntryDto } from '@/lib/api';
import { Plus, Search, Edit2, Trash2, X, Loader2, ChevronLeft, ChevronRight, DollarSign, FileText, BookOpen } from 'lucide-react';

export default function AccountingPage() {
  const [activeTab, setActiveTab] = useState<'accounts' | 'journals'>('accounts');
  const [accounts, setAccounts] = useState<AccountDto[]>([]);
  const [journals, setJournals] = useState<JournalEntryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [showModal, setShowModal] = useState(false);
  const [editingAccount, setEditingAccount] = useState<AccountDto | null>(null);
  const [formData, setFormData] = useState({ accountCode: '', name: '', accountType: 'Asset', class: 'Debit', openingBalance: '' });
  const [saving, setSaving] = useState(false);
  const [deleteConfirm, setDeleteConfirm] = useState<string | null>(null);
  const pageSize = 10;

  const fetchAccounts = useCallback(async () => {
    setLoading(true);
    try {
      const result = await accountsApi.getAll({ page, pageSize, search: search || undefined });
      if (result?.success && result.data) {
        setAccounts(result.data.items || []);
        setTotalCount(result.data.totalCount || 0);
        setTotalPages(Math.ceil((result.data.totalCount || 0) / pageSize));
      } else {
        setAccounts([]);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load accounts');
      setAccounts([]);
    } finally {
      setLoading(false);
    }
  }, [page, search]);

  const fetchJournals = useCallback(async () => {
    setLoading(true);
    try {
      const result = await journalEntriesApi.getAll({ page, pageSize, search: search || undefined });
      if (result?.success && result.data) {
        setJournals(result.data.items || []);
        setTotalCount(result.data.totalCount || 0);
        setTotalPages(Math.ceil((result.data.totalCount || 0) / pageSize));
      } else {
        setJournals([]);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load journal entries');
      setJournals([]);
    } finally {
      setLoading(false);
    }
  }, [page, search]);

  useEffect(() => {
    if (activeTab === 'accounts') fetchAccounts();
    else fetchJournals();
  }, [activeTab, fetchAccounts, fetchJournals]);

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
      } else {
        await accountsApi.create(data);
      }
      setShowModal(false);
      fetchAccounts();
    } catch (err: any) {
      alert(err.message || 'Failed to save');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await accountsApi.delete(id);
      setDeleteConfirm(null);
      fetchAccounts();
    } catch (err: any) {
      alert(err.message || 'Failed to delete');
    }
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
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Accounting</h1>
          <p className="text-slate-500 dark:text-slate-400 mt-1">Chart of accounts and journal entries</p>
        </div>
        {activeTab === 'accounts' && (
          <button onClick={openCreate} className="flex items-center gap-2 px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg transition">
            <Plus className="w-4 h-4" /> Add Account
          </button>
        )}
      </div>

      {/* Tabs */}
      <div className="flex gap-1 bg-slate-100 dark:bg-slate-700 p-1 rounded-lg w-fit">
        <button onClick={() => { setActiveTab('accounts'); setPage(1); setSearch(''); }} className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition ${activeTab === 'accounts' ? 'bg-white dark:bg-slate-600 text-slate-900 dark:text-white shadow-sm' : 'text-slate-500 hover:text-slate-700 dark:hover:text-slate-300'}`}>
          <BookOpen className="w-4 h-4" /> Chart of Accounts
        </button>
        <button onClick={() => { setActiveTab('journals'); setPage(1); setSearch(''); }} className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition ${activeTab === 'journals' ? 'bg-white dark:bg-slate-600 text-slate-900 dark:text-white shadow-sm' : 'text-slate-500 hover:text-slate-700 dark:hover:text-slate-300'}`}>
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
            <div className="flex items-center justify-center h-48"><Loader2 className="w-8 h-8 animate-spin text-purple-600" /></div>
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
                      <tr key={acc.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/30">
                        <td className="px-4 py-3 font-mono text-sm font-medium text-slate-900 dark:text-white">{acc.accountCode}</td>
                        <td className="px-4 py-3 font-medium text-slate-900 dark:text-white">{acc.name}</td>
                        <td className="px-4 py-3">
                          <span className={`px-2 py-1 text-xs font-medium rounded-full ${accountTypeColors[acc.accountType || 'Asset'] || 'bg-slate-100 text-slate-700'}`}>{acc.accountType || 'Asset'}</span>
                        </td>
                        <td className="px-4 py-3 text-center text-sm text-slate-600 dark:text-slate-400">{acc.class || 'Debit'}</td>
                        <td className="px-4 py-3 text-right font-mono text-slate-600 dark:text-slate-400">{acc.openingBalance ? Number(acc.openingBalance).toFixed(2) : '0.00'}</td>
                        <td className="px-4 py-3 text-right">
                          <button onClick={() => openEdit(acc)} className="p-1.5 text-slate-400 hover:text-purple-600 hover:bg-purple-50 rounded"><Edit2 className="w-4 h-4" /></button>
                          <button onClick={() => setDeleteConfirm(acc.id)} className="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded ml-1"><Trash2 className="w-4 h-4" /></button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <div className="p-4 border-t border-slate-200 dark:border-slate-700 flex items-center justify-between">
                <p className="text-sm text-slate-500">Showing {(page - 1) * pageSize + 1} to {Math.min(page * pageSize, totalCount)} of {totalCount}</p>
                <div className="flex items-center gap-2">
                  <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page <= 1} className="p-2 rounded-lg border border-slate-300 disabled:opacity-50"><ChevronLeft className="w-4 h-4" /></button>
                  <span className="text-sm font-medium px-3">{page} / {totalPages || 1}</span>
                  <button onClick={() => setPage(p => Math.min(totalPages, p + 1))} disabled={page >= totalPages} className="p-2 rounded-lg border border-slate-300 disabled:opacity-50"><ChevronRight className="w-4 h-4" /></button>
                </div>
              </div>
            </>
          )
        )}

        {/* Journals Table */}
        {activeTab === 'journals' && (
          loading ? (
            <div className="flex items-center justify-center h-48"><Loader2 className="w-8 h-8 animate-spin text-purple-600" /></div>
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
                      <tr key={j.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/30">
                        <td className="px-4 py-3 font-mono text-sm font-medium text-slate-900 dark:text-white">{j.entryNumber || j.id.slice(0, 8)}</td>
                        <td className="px-4 py-3 text-slate-600 dark:text-slate-400 text-sm">{j.entryDate ? new Date(j.entryDate).toLocaleDateString() : '-'}</td>
                        <td className="px-4 py-3 font-medium text-slate-900 dark:text-white">{j.title || '-'}</td>
                        <td className="px-4 py-3 text-center">
                          <span className={`px-2 py-1 text-xs font-medium rounded-full ${j.status === 'Posted' ? 'bg-green-100 text-green-700' : 'bg-slate-100 text-slate-700'}`}>{j.status || 'Draft'}</span>
                        </td>
                        <td className="px-4 py-3 text-right font-mono text-slate-600 dark:text-slate-400">{j.totalDebit ? Number(j.totalDebit).toFixed(2) : '0.00'}</td>
                        <td className="px-4 py-3 text-right font-mono text-slate-600 dark:text-slate-400">{j.totalCredit ? Number(j.totalCredit).toFixed(2) : '0.00'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <div className="p-4 border-t border-slate-200 dark:border-slate-700 flex items-center justify-between">
                <p className="text-sm text-slate-500">Showing {(page - 1) * pageSize + 1} to {Math.min(page * pageSize, totalCount)} of {totalCount}</p>
                <div className="flex items-center gap-2">
                  <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page <= 1} className="p-2 rounded-lg border border-slate-300 disabled:opacity-50"><ChevronLeft className="w-4 h-4" /></button>
                  <span className="text-sm font-medium px-3">{page} / {totalPages || 1}</span>
                  <button onClick={() => setPage(p => Math.min(totalPages, p + 1))} disabled={page >= totalPages} className="p-2 rounded-lg border border-slate-300 disabled:opacity-50"><ChevronRight className="w-4 h-4" /></button>
                </div>
              </div>
            </>
          )
        )}
      </div>

      {/* Account Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className="bg-white dark:bg-slate-800 rounded-xl shadow-xl w-full max-w-md mx-4">
            <div className="flex items-center justify-between p-5 border-b border-slate-200 dark:border-slate-700">
              <h3 className="text-lg font-semibold text-slate-900 dark:text-white">{editingAccount ? 'Edit Account' : 'Add Account'}</h3>
              <button onClick={() => setShowModal(false)} className="p-1 hover:bg-slate-100 rounded"><X className="w-5 h-5" /></button>
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
              <button onClick={() => setShowModal(false)} className="px-4 py-2 border border-slate-300 rounded-lg">Cancel</button>
              <button onClick={handleSave} disabled={saving || !formData.accountCode || !formData.name} className="px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg disabled:opacity-50 flex items-center gap-2">
                {saving && <Loader2 className="w-4 h-4 animate-spin" />}
                {editingAccount ? 'Update' : 'Create'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Delete Confirm */}
      {deleteConfirm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className="bg-white dark:bg-slate-800 rounded-xl shadow-xl w-full max-w-sm mx-4 p-6">
            <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-2">Delete Account?</h3>
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
