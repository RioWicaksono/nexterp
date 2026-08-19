'use client';

import { useState } from 'react';
import { Plus, Building2, MapPin, Phone, Mail, Edit2, Trash2 } from 'lucide-react';

const ORGS = [
  { id: '1', name: 'Demo Corporation', code: 'DEMO', taxId: '01.234.567.8-901.000', email: 'admin@demo.com', phone: '+62 21 1234 5678', city: 'Jakarta Selatan', country: 'Indonesia', active: true, users: 8, modules: 5 },
];

export default function OrganizationsPage() {
  const [orgs, setOrgs] = useState(ORGS);

  const handleDelete = (id: string) => {
    setOrgs(prev => prev.filter(o => o.id !== id));
  };

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Organizations</h1>
          <p className="text-sm text-slate-500 mt-1">Manage organizations and tenants</p>
        </div>
        <button className="flex items-center gap-2 px-3 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm">
          <Plus className="w-4 h-4" /> Add Organization
        </button>
      </div>

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
                    <span className={`px-2 py-0.5 text-xs rounded-full ${org.active ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400' : 'bg-slate-100 text-slate-500'}`}>
                      {org.active ? 'Active' : 'Inactive'}
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
                <button className="p-2 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg">
                  <Edit2 className="w-4 h-4" />
                </button>
                <button onClick={() => handleDelete(org.id)} className="p-2 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg">
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
