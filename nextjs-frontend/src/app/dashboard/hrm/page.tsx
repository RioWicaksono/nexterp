'use client';

import { useEffect, useState } from 'react';
import api from '@/lib/api';
import { useAuthStore } from '@/lib/store';
import { Users, Plus, Search, Loader2, Edit, Trash2 } from 'lucide-react';

interface Employee {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  isActive: boolean;
  department?: string;
}

export default function HrmPage() {
  const { token } = useAuthStore();
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');

  useEffect(() => {
    const fetchEmployees = async () => {
      setLoading(true);
      try {
        const res = await api.get('/users?page=1&pageSize=50', {
          headers: { Authorization: `Bearer ${token}` }
        });
        if (res.data?.data?.items) {
          setEmployees(res.data.data.items);
        }
      } catch (error) {
        console.error('Failed to fetch employees:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchEmployees();
  }, [token]);

  const filtered = employees.filter(e =>
    `${e.firstName} ${e.lastName} ${e.email}`.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Human Resources</h1>
        <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg">
          <Plus className="w-4 h-4" /> Add Employee
        </button>
      </div>

      <div className="bg-white dark:bg-slate-800 rounded-xl shadow p-6">
        <div className="flex items-center gap-4 mb-4">
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input
              type="text"
              placeholder="Search employees..."
              value={search}
              onChange={e => setSearch(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-slate-200 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-900"
            />
          </div>
        </div>

        {loading ? (
          <div className="flex items-center justify-center py-12">
            <Loader2 className="w-8 h-8 animate-spin text-blue-600" />
          </div>
        ) : (
          <table className="w-full">
            <thead>
              <tr className="text-left text-sm text-slate-500 border-b border-slate-200 dark:border-slate-700">
                <th className="pb-3 font-medium">Name</th>
                <th className="pb-3 font-medium">Email</th>
                <th className="pb-3 font-medium">Phone</th>
                <th className="pb-3 font-medium">Status</th>
                <th className="pb-3 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="text-sm">
              {filtered.map(emp => (
                <tr key={emp.id} className="border-b border-slate-100 dark:border-slate-800">
                  <td className="py-3">
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 rounded-full bg-slate-200 flex items-center justify-center font-medium">
                        {emp.firstName[0]}
                      </div>
                      <div>
                        <p className="font-medium text-slate-900 dark:text-white">
                          {emp.firstName} {emp.lastName}
                        </p>
                      </div>
                    </div>
                  </td>
                  <td className="py-3 text-slate-600 dark:text-slate-400">{emp.email}</td>
                  <td className="py-3 text-slate-600 dark:text-slate-400">{emp.phone || '-'}</td>
                  <td className="py-3">
                    <span className={`px-2 py-1 text-xs rounded-full ${emp.isActive ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
                      {emp.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
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
          <p className="text-center py-12 text-slate-500">No employees found</p>
        )}
      </div>
    </div>
  );
}
