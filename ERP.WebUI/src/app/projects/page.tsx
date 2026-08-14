"use client";

import { Briefcase, Users, Clock, CheckCircle, Plus, RefreshCw } from "lucide-react";
import { AppShell } from "@/app/components/AppShell";
import { api } from "@/lib/api";
import { useEffect, useState } from "react";

interface Project {
  id: string;
  projectName: string;
  projectCode: string;
  status: string;
  startDate: string;
  endDate: string;
  progress: number;
}

export default function ProjectsPage() {
  const [loading, setLoading] = useState(true);
  const [projects, setProjects] = useState<Project[]>([]);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const res = await api.get<{ items: Project[] }>("/api/v1/projects");
      if (res.success && res.data) {
        setProjects(res.data.items || []);
      }
    } catch (error) {
      console.error("Failed to load projects:", error);
    } finally {
      setLoading(false);
    }
  };

  const activeProjects = projects.filter(p => p.status === "Active" || p.status === "InProgress").length;
  const completedProjects = projects.filter(p => p.status === "Completed").length;

  return (
    <AppShell>
      <div className="space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-slate-800 dark:text-white">Projects</h1>
            <p className="text-slate-500 dark:text-slate-400">Project planning and task management</p>
          </div>
          <div className="flex gap-2">
            <button onClick={loadData} className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl hover:bg-slate-50 dark:hover:bg-slate-700">
              <RefreshCw className="w-4 h-4" />
              Refresh
            </button>
            <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl">
              <Plus className="w-4 h-4" />
              New Project
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard icon={Briefcase} label="Active Projects" value={activeProjects.toString()} color="blue" />
          <StatCard icon={Users} label="Total Projects" value={projects.length.toString()} color="emerald" />
          <StatCard icon={Clock} label="Pending Tasks" value="0" color="purple" />
          <StatCard icon={CheckCircle} label="Completed" value={completedProjects.toString()} color="amber" />
        </div>

        <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-slate-50 dark:bg-slate-700/50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Project</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Code</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Status</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">Progress</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase">End Date</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                {loading ? (
                  <tr>
                    <td colSpan={5} className="px-6 py-12 text-center">
                      <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin mx-auto" />
                    </td>
                  </tr>
                ) : projects.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="px-6 py-12 text-center text-slate-500">No projects found</td>
                  </tr>
                ) : (
                  projects.map(project => (
                    <tr key={project.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/50">
                      <td className="px-6 py-4 font-medium text-slate-800 dark:text-white">{project.projectName}</td>
                      <td className="px-6 py-4 text-slate-500">{project.projectCode}</td>
                      <td className="px-6 py-4">
                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                          project.status === "Completed" ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400" :
                          project.status === "Active" || project.status === "InProgress" ? "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400" :
                          "bg-slate-100 text-slate-500"
                        }`}>{project.status}</span>
                      </td>
                      <td className="px-6 py-4">
                        <div className="w-24 bg-slate-200 dark:bg-slate-700 rounded-full h-2">
                          <div className="bg-blue-600 h-2 rounded-full" style={{ width: `${project.progress || 0}%` }} />
                        </div>
                      </td>
                      <td className="px-6 py-4 text-slate-500">{project.endDate ? new Date(project.endDate).toLocaleDateString() : "-"}</td>
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
