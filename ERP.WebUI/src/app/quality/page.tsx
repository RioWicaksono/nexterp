"use client";

import { CheckCircle, XCircle, AlertTriangle, ClipboardCheck, Plus, RefreshCw } from "lucide-react";
import { AppShell } from "@/app/components/AppShell";
import { api } from "@/lib/api";
import { useEffect, useState } from "react";

interface Inspection {
  id: string;
  inspectionNumber: string;
  inspectionDate: string;
  inspectorName: string;
  result: string;
  notes: string;
}

export default function QualityPage() {
  const [loading, setLoading] = useState(true);
  const [inspections, setInspections] = useState<Inspection[]>([]);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const res = await api.get<{ items: Inspection[] }>("/api/v1/inspections");
      if (res.success && res.data) {
        setInspections(res.data.items || []);
      }
    } catch (error) {
      console.error("Failed to load inspections:", error);
    } finally {
      setLoading(false);
    }
  };

  const passed = inspections.filter(i => i.result === "Passed").length;
  const failed = inspections.filter(i => i.result === "Failed").length;
  const openNCR = inspections.filter(i => i.result === "Pending").length;

  return (
    <AppShell>
      <div className="space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-slate-800 dark:text-white">Quality Management</h1>
            <p className="text-slate-500 dark:text-slate-400">Inspections, NCR, and CAPA management</p>
          </div>
          <div className="flex gap-2">
            <button onClick={loadData} className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl hover:bg-slate-50 dark:hover:bg-slate-700">
              <RefreshCw className="w-4 h-4" />
              Refresh
            </button>
            <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl">
              <Plus className="w-4 h-4" />
              New Inspection
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard icon={ClipboardCheck} label="Total Inspections" value={inspections.length.toString()} color="blue" />
          <StatCard icon={CheckCircle} label="Passed" value={passed.toString()} color="emerald" />
          <StatCard icon={XCircle} label="Failed" value={failed.toString()} color="purple" />
          <StatCard icon={AlertTriangle} label="Open NCR" value={openNCR.toString()} color="amber" />
        </div>

        <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-slate-50 dark:bg-slate-700/50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Inspection #</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Date</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Inspector</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Result</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Notes</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                {loading ? (
                  <tr><td colSpan={5} className="px-6 py-12 text-center"><div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin mx-auto" /></td></tr>
                ) : inspections.length === 0 ? (
                  <tr><td colSpan={5} className="px-6 py-12 text-center text-slate-500">No inspections found</td></tr>
                ) : (
                  inspections.map(inspection => (
                    <tr key={inspection.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/50">
                      <td className="px-6 py-4 font-medium text-slate-800 dark:text-white">{inspection.inspectionNumber}</td>
                      <td className="px-6 py-4 text-slate-500">{new Date(inspection.inspectionDate).toLocaleDateString()}</td>
                      <td className="px-6 py-4 text-slate-500">{inspection.inspectorName}</td>
                      <td className="px-6 py-4">
                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${inspection.result === "Passed" ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400" : inspection.result === "Failed" ? "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400" : "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400"}`}>{inspection.result}</span>
                      </td>
                      <td className="px-6 py-4 text-slate-500 truncate max-w-xs">{inspection.notes || "-"}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </AppShell>
  );
}

function StatCard({ icon: Icon, label, value, color }: { icon: React.ElementType; label: string; value: string; color: "blue" | "emerald" | "purple" | "amber" }) {
  const colors = { blue: "bg-blue-100 dark:bg-blue-900/30 text-blue-600", emerald: "bg-emerald-100 dark:bg-emerald-900/30 text-emerald-600", purple: "bg-purple-100 dark:bg-purple-900/30 text-purple-600", amber: "bg-amber-100 dark:bg-amber-900/30 text-amber-600" };
  return (
    <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
      <div className="flex items-center gap-4">
        <div className={`w-12 h-12 rounded-xl flex items-center justify-center ${colors[color]}`}><Icon className="w-6 h-6" /></div>
        <div><p className="text-sm text-slate-500 dark:text-slate-400">{label}</p><p className="text-2xl font-bold text-slate-800 dark:text-white">{value}</p></div>
      </div>
    </div>
  );
}
