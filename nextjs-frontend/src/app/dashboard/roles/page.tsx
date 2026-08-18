'use client';

import { useState, useEffect } from 'react';
import { Key, Plus, Search, Shield } from 'lucide-react';

interface Role {
  id: string;
  name: string;
  description?: string;
  userCount: number;
  isSystemRole?: boolean;
}

export default function RolesPage() {
  const [roles] = useState<Role[]>([
    { id: '1', name: 'Admin', description: 'Full system access', userCount: 1, isSystemRole: true },
  ]);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Roles</h1>
        <button type="button" className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg">
          <Plus className="w-4 h-4" /> New Role
        </button>
      </div>
      <div className="bg-white dark:bg-slate-800 rounded-xl shadow overflow-hidden">
        <table className="w-full text-left">
          <thead className="bg-slate-50 dark:bg-slate-900">
            <tr className="text-sm text-slate-500">
              <th className="px-4 py-3 font-medium">Role</th>
              <th className="px-4 py-3 font-medium">Users</th>
              <th className="px-4 py-3 text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            {roles.map((role) => (
              <tr key={role.id} className="hover:bg-slate-50 dark:hover:bg-slate-800">
                <td className="px-4 py-3">
                  <div className="flex items-center gap-2">
                    {role.isSystemRole && <Shield className="w-4 h-4 text-blue-500" />}
                    <span className="font-medium text-slate-900 dark:text-white">{role.name}</span>
                  </div>
                </td>
                <td className="px-4 py-3 text-slate-600">{role.userCount} user(s)</td>
                <td className="px-4 py-3 text-right">
                  <button type="button" className="px-3 py-1 text-sm hover:bg-slate-100 dark:hover:bg-slate-700 rounded">Edit</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {roles.length === 0 && (
          <p className="py-12 text-center text-slate-500">No roles found</p>
        )}
      </div>
    </div>
  );
}
