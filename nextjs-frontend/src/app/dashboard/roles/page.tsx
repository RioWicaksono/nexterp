'use client';

import { useState } from 'react';
import { Plus, Shield, Users, Edit2, Trash2, X, Loader2 } from 'lucide-react';

const ROLES = [
  { id: '1', name: 'Super Admin', description: 'Full system access', permissions: ['*'], users: 1, system: true },
  { id: '2', name: 'Admin', description: 'Full module access', permissions: ['*'], users: 2, system: true },
  { id: '3', name: 'HR Manager', description: 'HRM and Payroll access', permissions: ['HRM', 'Payroll'], users: 5, system: false },
  { id: '4', name: 'Inventory Manager', description: 'Inventory and warehouse', permissions: ['Inventory'], users: 3, system: false },
  { id: '5', name: 'Purchasing Staff', description: 'Create and manage POs', permissions: ['Purchasing'], users: 4, system: false },
  { id: '6', name: 'Accountant', description: 'Accounting and reports', permissions: ['Accounting'], users: 2, system: false },
];

export default function RolesPage() {
  const [roles] = useState(ROLES);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Roles & Permissions</h1>
          <p className="text-sm text-slate-500 mt-1">Manage user roles and access control</p>
        </div>
        <button className="flex items-center gap-2 px-3 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm">
          <Plus className="w-4 h-4" /> Add Role
        </button>
      </div>
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 overflow-hidden">
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
            {roles.map(role => (
              <tr key={role.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/30">
                <td className="px-4 py-3">
                  <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded-lg bg-slate-100 dark:bg-slate-700 flex items-center justify-center">
                      <Shield className="w-4 h-4 text-slate-500" />
                    </div>
                    <div>
                      <p className="font-medium text-slate-900 dark:text-white">{role.name}</p>
                      <p className="text-xs text-slate-500">{role.description}</p>
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
                  <span className="text-sm text-slate-600 dark:text-slate-300">{role.users}</span>
                </td>
                <td className="px-4 py-3 text-center">
                  <span className={`px-2 py-1 text-xs rounded-full font-medium ${role.system ? 'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400' : 'bg-slate-100 text-slate-600 dark:bg-slate-700 dark:text-slate-300'}`}>
                    {role.system ? 'System' : 'Custom'}
                  </span>
                </td>
                <td className="px-4 py-3 text-right">
                  <button className="p-1.5 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded"><Edit2 className="w-4 h-4" /></button>
                  {!role.system && (
                    <button className="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded"><Trash2 className="w-4 h-4" /></button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
