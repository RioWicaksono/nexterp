'use client';

import { useState } from 'react';
import { Breadcrumbs } from '@/components/Breadcrumbs';
import { PageHeader } from '@/components/PageHeader';
import { useToast } from '@/hooks/useToast';
import { ConfirmDialog } from '@/components/ConfirmDialog';
import { Plus, Building2, MapPin, Phone, Mail, Edit2, Trash2, X, Loader2 } from 'lucide-react';

interface Organization {
  id: string;
  name: string;
  code: string;
  taxId: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  country: string;
  isActive: boolean;
  modules: number;
  users: number;
}

export default function OrganizationsPage() {
  const toast = useToast();
  const [orgs, setOrgs] = useState<Organization[]>([
    { id: '1', name: 'Demo Corporation', code: 'DEMO', taxId: '01.234.567.8-901.000', email: 'admin@demo.com', phone: '+62 21 1234 5678', address: 'Jl. Sudirman No. 1', city: 'Jakarta Selatan', country: 'Indonesia', isActive: true, modules: 5, users: 8 },
  ]);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<Organization | null>(null);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [formData, setFormData] = useState({ name: '', code: '', taxId: '', email: '', phone: '', address: '', city: '', country: '' });
  const [saving, setSaving] = useState(false);

  const handleSave = async () => {
    setSaving(true);
    await new Promise(r => setTimeout(r, 500));
    if (editing) {
      setOrgs(prev => prev.map(o => o.id === editing.id ? { ...o, ...formData } : o));
      toast('success', 'Updated!', 'Organization updated');
    } else {
      setOrgs(prev => [...prev, { ...formData, id: Date.now().toString(), isActive: true, modules: 0, users: 0 }]);
      toast('success', 'Created!', 'Organization created');
    }
    setShowModal(false);
    setSaving(false);
  };

  const handleDelete = async () => {
    setOrgs(prev => prev.filter(o => o.id !== deleteId));
    toast('success', 'Deleted!', 'Organization deleted');
    setDeleteId(null);
  };

  return (
    <div className="space-y-4">
      <Breadcrumbs items={[{ label: 'Dashboard', href: '/dashboard' }, { label: 'Organizations' }]} />
      <PageHeader title="Organizations" subtitle="Manage organizations and tenants">
        <button
          onClick={() => { setEditing(null); setFormData({ name: '', code: '', taxId: '', email: '', phone: '', address: '', city: '', country: '' }); setShowModal(true); }}
          className="flex items-center gap-2 px-3 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm"
        >
          <Plus className="w-4 h-4" /> Add Organization
        </button>
      </PageHeader>

      <div className="grid gap-4">
        {orgs.map(org => (
          <div key={org.id} className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-4">
            <div className="flex items-start justify-between">
              <div className="flex items-start gap-4">
                <div className="w-12 h-12 rounded-xl bg-slate-100 dark:bg-slate-700 flex items-center justify-center">
                  <Building2 className="w-6 h-6 text-slate-500" />
                </div>
                <div>
                  <div className="flex items-center gap-2">
                    <h3 className="font-semibold text-slate-900 dark:text-white">{org.name}</h3>
                    <span className={`px-2 py-0.5 text-xs rounded-full ${org.isActive ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400' : 'bg-slate-100 text-slate-500'}`}>
                      {org.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </div>
                  <p className="text-sm text-slate-500">{org.code} | NPWP: {org.taxId}</p>
                  <div className="flex items-center gap-4 mt-2 text-sm text-slate-500">
                    <span className="flex items-center gap-1"><Mail className="w-3.5 h-3.5" />{org.email}</span>
                    <span className="flex items-center gap-1"><Phone className="w-3.5 h-3.5" />{org.phone}</span>
                    <span className="flex items-center gap-1"><MapPin className="w-3.5 h-3.5" />{org.city}, {org.country}</span>
                  </div>
                </div>
              </div>
              <div className="flex items-center gap-4">
                <div className="text-right">
                  <p className="text-lg font-bold text-slate-900 dark:text-white">{org.users}</p>
                  <p className="text-xs text-slate-500">Users</p>
                </div>
                <div className="text-right mr-2">
                  <p className="text-lg font-bold text-slate-900 dark:text-white">{org.modules}</p>
                  <p className="text-xs text-slate-500">Modules</p>
                </div>
                <button onClick={() => { setEditing(org); setFormData({ name: org.name, code: org.code, taxId: org.taxId, email: org.email, phone: org.phone, address: org.address, city: org.city, country: org.country }); setShowModal(true); }} className="p-2 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg"><Edit2 className="w-4 h-4" /></button>
                <button onClick={() => setDeleteId(org.id)} className="p-2 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg"><Trash2 className="w-4 h-4" /></button>
              </div>
            </div>
          </div>
        ))}
      </div>

      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm" onClick={() => setShowModal(false)}>
          <div className="bg-white dark:bg-slate-800 rounded-xl shadow-xl w-full max-w-lg mx-4" onClick={e => e.stopPropagation()}>
            <div className="flex items-center justify-between p-4 border-b border-slate-200 dark:border-slate-700">
              <h3 className="font-semibold text-slate-900 dark:text-white">{editing ? 'Edit Organization' : 'New Organization'}</h3>
              <button onClick={() => setShowModal(false)} className="p-1 hover:bg-slate-100 rounded"><X className="w-5 h-5" /></button>
            </div>
            <div className="p-4 grid grid-cols-2 gap-4">
              <div className="col-span-2">
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Organization Name *</label>
                <input type="text" value={formData.name} onChange={e => setFormData({ ...formData, name: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-sm" required />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Code *</label>
                <input type="text" value={formData.code} onChange={e => setFormData({ ...formData, code: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-sm" required />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">NPWP</label>
                <input type="text" value={formData.taxId} onChange={e => setFormData({ ...formData, taxId: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-sm" />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Email</label>
                <input type="email" value={formData.email} onChange={e => setFormData({ ...formData, email: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-sm" />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Phone</label>
                <input type="text" value={formData.phone} onChange={e => setFormData({ ...formData, phone: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-sm" />
              </div>
              <div className="col-span-2">
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Address</label>
                <input type="text" value={formData.address} onChange={e => setFormData({ ...formData, address: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-sm" />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">City</label>
                <input type="text" value={formData.city} onChange={e => setFormData({ ...formData, city: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-sm" />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Country</label>
                <input type="text" value={formData.country} onChange={e => setFormData({ ...formData, country: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-sm" />
              </div>
            </div>
            <div className="p-4 border-t border-slate-200 dark:border-slate-700 flex gap-2 justify-end">
              <button onClick={() => setShowModal(false)} className="px-4 py-2 border border-slate-300 rounded-lg text-sm hover:bg-slate-50">Cancel</button>
              <button onClick={handleSave} disabled={saving || !formData.name || !formData.code} className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm disabled:opacity-50 flex items-center gap-2">
                {saving && <Loader2 className="w-4 h-4 animate-spin" />}
                {editing ? 'Update' : 'Create'}
              </button>
            </div>
          </div>
        </div>
      )}

      <ConfirmDialog isOpen={!!deleteId} title="Delete Organization?" message="This will remove all associated data." confirmText="Delete" onConfirm={handleDelete} onCancel={() => setDeleteId(null)} variant="danger" />
    </div>
  );
}
