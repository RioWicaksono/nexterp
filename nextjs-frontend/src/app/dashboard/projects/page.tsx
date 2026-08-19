'use client';

import { useEffect, useState, useCallback } from 'react';
import { projectsApi, projectTasksApi, type ProjectDto, type ProjectTaskDto } from '@/lib/api';
import { PageHeader } from '@/components/PageHeader';
import { SkeletonLoader } from '@/components/SkeletonLoader';
import { ConfirmDialog } from '@/components/ConfirmDialog';
import { useToast } from '@/hooks/useToast';
import { Plus, Search, X, Loader2, ChevronLeft, ChevronRight, FolderKanban, CheckCircle2, Clock, Circle, AlertTriangle, PlayCircle, CheckCircle } from 'lucide-react';

const PAGE_SIZE_OPTIONS = [10, 25, 50];

export default function ProjectsPage() {
  const [activeTab, setActiveTab] = useState<'projects' | 'tasks'>('projects');
  const [projects, setProjects] = useState<ProjectDto[]>([]);
  const [tasks, setTasks] = useState<ProjectTaskDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [showModal, setShowModal] = useState(false);
  const [formData, setFormData] = useState({ name: '', code: '', description: '', startDate: '', endDate: '', budget: '' });
  const [saving, setSaving] = useState(false);

  const toast = useToast();

  const fetchProjects = useCallback(async () => {
    setLoading(true);
    try {
      const result = await projectsApi.getAll({ page, pageSize, search: search || undefined });
      if (result?.success && result.data) {
        setProjects(result.data.items || []);
        setTotalCount(result.data.totalCount || 0);
        setTotalPages(Math.ceil((result.data.totalCount || 0) / pageSize));
        setError(null);
      } else {
        setProjects([]);
        setTotalCount(0);
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to load projects';
      setError(msg);
      toast('error', 'Error', msg);
      setProjects([]);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, toast]);

  const fetchTasks = useCallback(async () => {
    setLoading(true);
    try {
      const result = await projectTasksApi.getAll({ page, pageSize, search: search || undefined });
      if (result?.success && result.data) {
        setTasks(result.data.items || []);
        setTotalCount(result.data.totalCount || 0);
        setTotalPages(Math.ceil((result.data.totalCount || 0) / pageSize));
        setError(null);
      } else {
        setTasks([]);
        setTotalCount(0);
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to load tasks';
      setError(msg);
      toast('error', 'Error', msg);
      setTasks([]);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, toast]);

  useEffect(() => {
    if (activeTab === 'projects') fetchProjects();
    else fetchTasks();
  }, [activeTab, fetchProjects, fetchTasks]);

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

  const handleCreate = async () => {
    setSaving(true);
    try {
      await projectsApi.create({
        name: formData.name,
        code: formData.code,
        description: formData.description,
        startDate: formData.startDate || undefined,
        endDate: formData.endDate || undefined,
        budget: formData.budget ? Number(formData.budget) : undefined,
      });
      toast('success', 'Created!', 'Project has been created');
      setShowModal(false);
      setFormData({ name: '', code: '', description: '', startDate: '', endDate: '', budget: '' });
      fetchProjects();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to create project';
      toast('error', 'Error', msg);
    } finally {
      setSaving(false);
    }
  };

  const handleAction = async (id: string, action: 'start' | 'complete') => {
    try {
      if (action === 'start') {
        await projectsApi.start(id);
      } else {
        await projectsApi.complete(id);
      }
      toast('success', 'Updated!', `Project has been ${action === 'start' ? 'started' : 'completed'}`);
      fetchProjects();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Unknown error';
      toast('error', 'Error', msg);
    }
  };

  const handlePageSizeChange = (newSize: number) => {
    setPageSize(newSize);
    setPage(1);
  };

  const handleTabChange = (tab: 'projects' | 'tasks') => {
    setActiveTab(tab);
    setPage(1);
    setSearch('');
  };

  const openNewProjectModal = () => {
    setFormData({ name: '', code: '', description: '', startDate: '', endDate: '', budget: '' });
    setShowModal(true);
  };

  const statusConfig: Record<string, { label: string; color: string; icon: any }> = {
    Planning: { label: 'Planning', color: 'bg-slate-100 text-slate-700 dark:bg-slate-700 dark:text-slate-300', icon: Circle },
    Active: { label: 'Active', color: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400', icon: PlayCircle },
    OnHold: { label: 'On Hold', color: 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400', icon: AlertTriangle },
    Completed: { label: 'Completed', color: 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400', icon: CheckCircle },
    Cancelled: { label: 'Cancelled', color: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400', icon: Circle },
    Todo: { label: 'To Do', color: 'bg-slate-100 text-slate-700 dark:bg-slate-700 dark:text-slate-300', icon: Circle },
    InProgress: { label: 'In Progress', color: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400', icon: Clock },
    Review: { label: 'Review', color: 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400', icon: AlertTriangle },
    Done: { label: 'Done', color: 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400', icon: CheckCircle2 },
  };

  return (
    <div className="space-y-6">
      <PageHeader
        title="Projects"
        subtitle="Manage projects and tasks"
        breadcrumbs={[
          { label: 'Dashboard', href: '/dashboard' },
          { label: 'Projects' },
        ]}
        actions={
          activeTab === 'projects' ? (
            <button onClick={openNewProjectModal} className="flex items-center gap-2 px-4 py-2 bg-cyan-600 hover:bg-cyan-700 text-white rounded-lg transition">
              <Plus className="w-4 h-4" /> New Project
            </button>
          ) : undefined
        }
      />

      {/* Tabs */}
      <div className="flex gap-1 bg-slate-100 dark:bg-slate-700 p-1 rounded-lg w-fit">
        <button onClick={() => handleTabChange('projects')} className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition ${activeTab === 'projects' ? 'bg-white dark:bg-slate-600 text-slate-900 dark:text-white shadow-sm' : 'text-slate-500 hover:text-slate-700 dark:hover:text-slate-300'}`}>
          <FolderKanban className="w-4 h-4" /> Projects
        </button>
        <button onClick={() => handleTabChange('tasks')} className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition ${activeTab === 'tasks' ? 'bg-white dark:bg-slate-600 text-slate-900 dark:text-white shadow-sm' : 'text-slate-500 hover:text-slate-700 dark:hover:text-slate-300'}`}>
          <CheckCircle2 className="w-4 h-4" /> Tasks
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        {[
          { label: 'Total Projects', count: projects.length, color: 'bg-cyan-500' },
          { label: 'Active', count: projects.filter(p => p.status === 'Active').length, color: 'bg-blue-500' },
          { label: 'Completed', count: projects.filter(p => p.status === 'Completed').length, color: 'bg-green-500' },
          { label: 'Tasks', count: tasks.length, color: 'bg-purple-500' },
        ].map((stat) => (
          <div key={stat.label} className="bg-white dark:bg-slate-800 rounded-xl p-5 border border-slate-200 dark:border-slate-700">
            <div className="flex items-center gap-3">
              <div className={`p-2.5 rounded-lg ${stat.color}`}><FolderKanban className="w-5 h-5 text-white" /></div>
              <div><p className="text-2xl font-bold text-slate-900 dark:text-white">{stat.count || 0}</p><p className="text-sm text-slate-500">{stat.label}</p></div>
            </div>
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
              placeholder={activeTab === 'projects' ? 'Search projects...' : 'Search tasks...'}
              value={search}
              onChange={(e) => { setSearch(e.target.value); setPage(1); }}
              className="w-full pl-10 pr-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white placeholder-slate-400 focus:ring-2 focus:ring-cyan-500"
            />
          </div>
        </div>

        {/* Projects Table */}
        {activeTab === 'projects' && (
          loading ? (
            <div className="p-6">
              <SkeletonLoader rows={5} height="h-12" />
            </div>
          ) : error ? (
            <div className="p-6 text-red-500">{error}</div>
          ) : projects.length === 0 ? (
            <div className="flex flex-col items-center justify-center h-48 text-slate-400">
              <FolderKanban className="w-12 h-12 mb-2 opacity-50" />
              <p>No projects found</p>
              <button onClick={openNewProjectModal} className="mt-3 text-cyan-600 hover:underline">Create your first project</button>
            </div>
          ) : (
            <>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-slate-50 dark:bg-slate-700/50">
                    <tr>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Code</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Project Name</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Start Date</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">End Date</th>
                      <th className="px-4 py-3 text-right text-xs font-semibold text-slate-500 uppercase">Budget</th>
                      <th className="px-4 py-3 text-center text-xs font-semibold text-slate-500 uppercase">Progress</th>
                      <th className="px-4 py-3 text-center text-xs font-semibold text-slate-500 uppercase">Status</th>
                      <th className="px-4 py-3 text-center text-xs font-semibold text-slate-500 uppercase">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                    {projects.map((proj) => {
                      const config = statusConfig[proj.status || 'Planning'] || statusConfig['Planning'];
                      return (
                        <tr key={proj.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/30 transition-colors">
                          <td className="px-4 py-3 font-mono text-sm font-medium text-slate-900 dark:text-white">{proj.code || proj.id.slice(0, 6)}</td>
                          <td className="px-4 py-3">
                            <div className="font-medium text-slate-900 dark:text-white">{proj.name}</div>
                            {proj.description && <div className="text-xs text-slate-400 mt-0.5 line-clamp-1">{proj.description}</div>}
                          </td>
                          <td className="px-4 py-3 text-slate-600 dark:text-slate-400 text-sm">{proj.startDate ? new Date(proj.startDate).toLocaleDateString() : '-'}</td>
                          <td className="px-4 py-3 text-slate-600 dark:text-slate-400 text-sm">{proj.endDate ? new Date(proj.endDate).toLocaleDateString() : '-'}</td>
                          <td className="px-4 py-3 text-right font-mono text-slate-600 dark:text-slate-400">{proj.budget ? `$${Number(proj.budget).toLocaleString()}` : '-'}</td>
                          <td className="px-4 py-3">
                            <div className="flex items-center gap-2">
                              <div className="w-16 h-1.5 bg-slate-200 dark:bg-slate-600 rounded-full overflow-hidden">
                                <div className="h-full bg-cyan-500 rounded-full" style={{ width: `${proj.progress || 0}%` }} />
                              </div>
                              <span className="text-xs font-medium text-slate-500">{proj.progress || 0}%</span>
                            </div>
                          </td>
                          <td className="px-4 py-3 text-center">
                            <span className={`inline-flex items-center gap-1 px-2 py-1 text-xs font-medium rounded-full ${config.color}`}>
                              <config.icon className="w-3 h-3" /> {config.label}
                            </span>
                          </td>
                          <td className="px-4 py-3 text-center">
                            {(proj.status === 'Planning') && (
                              <button onClick={() => handleAction(proj.id, 'start')} className="px-2 py-1 text-xs bg-cyan-100 text-cyan-700 rounded hover:bg-cyan-200 transition-colors">Start</button>
                            )}
                            {(proj.status === 'Active') && (
                              <button onClick={() => handleAction(proj.id, 'complete')} className="px-2 py-1 text-xs bg-green-100 text-green-700 rounded hover:bg-green-200 transition-colors">Complete</button>
                            )}
                          </td>
                        </tr>
                      );
                    })}
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

        {/* Tasks Table */}
        {activeTab === 'tasks' && (
          loading ? (
            <div className="p-6">
              <SkeletonLoader rows={5} height="h-12" />
            </div>
          ) : error ? (
            <div className="p-6 text-red-500">{error}</div>
          ) : tasks.length === 0 ? (
            <div className="flex flex-col items-center justify-center h-48 text-slate-400">
              <CheckCircle2 className="w-12 h-12 mb-2 opacity-50" />
              <p>No tasks found</p>
            </div>
          ) : (
            <>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-slate-50 dark:bg-slate-700/50">
                    <tr>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Task</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Project</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase">Due Date</th>
                      <th className="px-4 py-3 text-center text-xs font-semibold text-slate-500 uppercase">Priority</th>
                      <th className="px-4 py-3 text-center text-xs font-semibold text-slate-500 uppercase">Progress</th>
                      <th className="px-4 py-3 text-center text-xs font-semibold text-slate-500 uppercase">Status</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                    {tasks.map((task) => {
                      const config = statusConfig[task.status || 'Todo'] || statusConfig['Todo'];
                      const priorityColors: Record<string, string> = { Low: 'text-slate-500', Medium: 'text-blue-500', High: 'text-orange-500', Critical: 'text-red-500' };
                      return (
                        <tr key={task.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/30 transition-colors">
                          <td className="px-4 py-3 font-medium text-slate-900 dark:text-white">{task.title}</td>
                          <td className="px-4 py-3 text-slate-600 dark:text-slate-400 text-sm">{task.projectName || '-'}</td>
                          <td className="px-4 py-3 text-slate-600 dark:text-slate-400 text-sm">{task.dueDate ? new Date(task.dueDate).toLocaleDateString() : '-'}</td>
                          <td className="px-4 py-3 text-center">
                            <span className={`text-xs font-medium ${priorityColors[task.priority || 'Medium'] || 'text-slate-500'}`}>{task.priority || 'Medium'}</span>
                          </td>
                          <td className="px-4 py-3">
                            <div className="flex items-center gap-2">
                              <div className="w-16 h-1.5 bg-slate-200 dark:bg-slate-600 rounded-full overflow-hidden">
                                <div className="h-full bg-cyan-500 rounded-full" style={{ width: `${task.progress || 0}%` }} />
                              </div>
                              <span className="text-xs text-slate-500">{task.progress || 0}%</span>
                            </div>
                          </td>
                          <td className="px-4 py-3 text-center">
                            <span className={`inline-flex items-center gap-1 px-2 py-1 text-xs font-medium rounded-full ${config.color}`}>
                              <config.icon className="w-3 h-3" /> {config.label}
                            </span>
                          </td>
                        </tr>
                      );
                    })}
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

      {/* Create Project Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50" onClick={(e) => e.target === e.currentTarget && setShowModal(false)}>
          <div className="bg-white dark:bg-slate-800 rounded-xl shadow-xl w-full max-w-md mx-4">
            <div className="flex items-center justify-between p-5 border-b border-slate-200 dark:border-slate-700">
              <h3 className="text-lg font-semibold text-slate-900 dark:text-white">New Project</h3>
              <button onClick={() => setShowModal(false)} aria-label="Close dialog" className="p-1 hover:bg-slate-100 rounded transition-colors"><X className="w-5 h-5" /></button>
            </div>
            <div className="p-5 space-y-4">
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Project Name *</label>
                <input type="text" value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" required />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Code</label>
                <input type="text" value={formData.code} onChange={(e) => setFormData({ ...formData, code: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white font-mono" />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Description</label>
                <textarea value={formData.description} onChange={(e) => setFormData({ ...formData, description: e.target.value })} rows={3} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Start Date</label>
                  <input type="date" value={formData.startDate} onChange={(e) => setFormData({ ...formData, startDate: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">End Date</label>
                  <input type="date" value={formData.endDate} onChange={(e) => setFormData({ ...formData, endDate: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Budget</label>
                <input type="number" value={formData.budget} onChange={(e) => setFormData({ ...formData, budget: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white" />
              </div>
            </div>
            <div className="p-5 border-t border-slate-200 dark:border-slate-700 flex gap-3 justify-end">
              <button onClick={() => setShowModal(false)} className="px-4 py-2 border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors">Cancel</button>
              <button onClick={handleCreate} disabled={saving || !formData.name} className="px-4 py-2 bg-cyan-600 hover:bg-cyan-700 text-white rounded-lg disabled:opacity-50 flex items-center gap-2 transition-colors">
                {saving && <Loader2 className="w-4 h-4 animate-spin" />}Create Project
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
