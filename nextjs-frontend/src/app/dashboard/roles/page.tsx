'use client';

import { useState } from 'react';
import { Breadcrumbs } from '@/components/Breadcrumbs';
import { PageHeader } from '@/components/PageHeader';
import { useToast } from '@/hooks/useToast';
import { ConfirmDialog } from '@/components/ConfirmDialog';
import { Plus, Shield, Users, Edit2, Trash2, X, Loader2 } from 'lucide-react';

interface Role {
  id: string;
  name: string;
  description: string;
  isSystemRole: boolean;
  permissions: string[];
  userCount: number;
}

export default function RolesPage() {
  const toast = useToast();
  const [roles, setRoles] = useState<Role[]>([
    { id: '1', name: 'Super Admin', description: 'Full system access', isSystemRole: true, permissions: ['*'], userCount: 1 },
    { id: '2', name: 'Admin', description: 'Full module access except system settings', isSystemRole: true, permissions: ['*'], userCount: 2 },
    { id: '3', name: 'HR Manager', description: 'HRM and Payroll access', isSystemRole: false, permissions: ['HRM', 'Payroll'], userCount: 5 },
    { id: '4', name: 'Inventory Manager', description: 'Inventory and warehouse management', isSystemRole: false, permissions: ['Inventory'], userCount: 3 },
    { id: '5', name: 'Purchasing Staff', description: 'Create and manage purchase orders', isSystemRole: false, permissions: ['Purchasing'], userCount: 4 },
    { id: '6', name: 'Accountant', description: 'Accounting and financial reports', isSystemRole: false, permissions: ['Accounting'], userCount: 2 },
  ]);
  const [showModal, setShowModal] = useState(false);
  const [editingRole, setEditingRole] = useState<Role | null>(null);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [formData, setFormData] = useState({ name: '', description: '' });
  const [saving, setSaving] = useState(false);

  const handleSave = async () => {
    setSaving(true);
    try {
      if (editingRole) {
        setRoles(prev => prev.map(r => r.id === editingRole.id ? { ...r, ...formData } : r));
        toast('success', 'Updated!', 'Role updated successfully');
      } else {
        setRoles(prev => [...prev, { ...formData, id: Date.now().toString(), isSystemRole: false, permissions: [], userCount: 0 }]);
        toast('success', 'Created!', 'Role created successfully');
      }
      setShowModal(false);
    } catch (e: any) {
      toast('error', 'Error', e.message);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    setRoles(prev => prev.filter(r => r.id !== deleteId));
    toast('success', 'Deleted!', 'Role deleted successfully');
    setDeleteId(null);
  };

  return (
    <div className="space-y-4">
      <Breadcrumbs items={[{ label: 'Dashboard', href: '/dashboard' }, { label: 'Roles' }]} />

      <PageHeader title="Roles & Permissions" subtitle="Manage user roles and access control">
        <button
          onClick={() => { setEditingRole(null); setFormData({ name: '', description: '' }); setShowModal(true); }}
          className="flex items-center gap-2 px-3 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm transition"
        >
          <Plus className="w-4 h-4" /> Add Role
        </button>
      </PageHeader>

      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-slate-50 dark:bg-slate-700/50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Role</th>
                <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Permissions</th>
                <th className="px-4 py-3 text-center text-xs font-semibold text-slate-500 uppercase">Users</th>
                <th className="px-4 py-3 text-center text-xs font-semibold text-slate-500 uppercase">Type</th>
                <th className="px-4 py-3 text-right text-xs font-semibold text-slate-500 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-700">
              {roles.map((role) => (
                <tr key={role.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/30">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 rounded-lg bg-slate-100 dark:bg-slate-700 flex items-center justify-center">
                        <Shield className="w-4 h-4 text-slate-500" />
                      </div>
                      <div>
                        <p className="font-medium text-slate-900 dark:text-white">{role.name}</p>
                        <p className="text-sm text-slate-500">{role.description}</p>
                      </div>
                    </div>
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex flex-wrap gap-1">
                      {role.permissions.slice(0, 3).map(p => (
                        <span key={p} className="px-2 py-0.5 bg-slate-100 dark:bg-slate-700 text-xs rounded text-slate-600 dark:text-slate-300">{p}</span>
                      ))}
                      {role.permissions.length > 3 && (
                        <span className="px-2 py-0.5 text-xs text-slate-400">+{role.permissions.length - 3}</span>
                      )}
                    </div>
                  </td>
                  <td className="px-4 py-3 text-center">
                    <span className="text-sm text-slate-600 dark:text-slate-300">{role.userCount}</span>
                  </td>
                  <td className="px-4 py-3 text-center">
                    <span className={`px-2 py-1 text-xs font-medium rounded-full ${role.isSystemRole ? 'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400' : 'bg-slate-100 text-slate-600 dark:bg-slate-700 dark:text-slate-300'}`}>
                      {role.isSystemRole ? 'System' : 'Custom'}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-right">
                    <button onClick={() => { setEditingRole(role); setFormData({ name: role.name, description: role.description }); setShowModal(true); }} className="p-1.5 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded"><Edit2 className="w-4 h-4" /></button>
                    {!role.isSystemRole && (
                      <button onClick={() => setDeleteId(role.id)} className="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded"><Trash2 className="w-4 h-4" /></button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm" onClick={() => setShowModal(false)}>
          <div className="bg-white dark:bg-slate-800 rounded-xl shadow-xl w-full max-w-md mx-4" onClick={e => e.stopPropagation()}>
            <div className="flex items-center justify-between p-4 border-b border-slate-200 dark:border-slate-700">
              <h3 className="font-semibold text-slate-900 dark:text-white">{editingRole ? 'Edit Role' : 'New Role'}</h3>
              <button onClick={() => setShowModal(false)} className="p-1 hover:bg-slate-100 rounded"><X className="w-5 h-5" /></button>
            </div>
            <div className="p-4 space-y-4">
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Role Name *</label>
                <input type="text" value={formData.name} onChange={e => setFormData({ ...formData, name: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" required />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Description</label>
                <textarea value={formData.description} onChange={e => setFormData({ ...formData, description: e.target.value })} rows={2} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" />
              </div>
            </div>
            <div className="p-4 border-t border-slate-200 dark:border-slate-700 flex gap-2 justify-end">
              <button onClick={() => setShowModal(false)} className="px-4 py-2 border border-slate-300 rounded-lg text-sm hover:bg-slate-50">Cancel</button>
              <button onClick={handleSave} disabled={saving || !formData.name} className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm disabled:opacity-50 flex items-center gap-2">
                {saving && <Loader2 className="w-4 h-4 animate-spin" />}
                {editingRole ? 'Update' : 'Create'}
              </button>
            </div>
          </div>
        </div>
      )}

      <ConfirmDialog isOpen={!!deleteId} title="Delete Role?" message="This action cannot be undone." confirmText="Delete" onConfirm={handleDelete} onCancel={() => setDeleteId(null)} variant="danger" />
    </div>
  );
}
