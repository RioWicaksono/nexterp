'use client';

import { useState, useMemo } from 'react';
import { PageHeader } from '@/components/PageHeader';
import { Search, Filter, Download, ChevronDown, ChevronRight, Clock, User, Package, Eye, Edit2, Trash2, Plus } from 'lucide-react';
import { cn } from '@/lib/utils';
import { downloadCSV } from '@/lib/export';

interface AuditEntry {
  id: string;
  timestamp: number;
  userId: string;
  userName: string;
  userEmail: string;
  action: 'create' | 'update' | 'delete' | 'view' | 'login' | 'logout';
  entityType: string;
  entityId: string;
  entityName: string;
  changes?: { field: string; oldValue: string; newValue: string }[];
  ipAddress?: string;
  userAgent?: string;
}

// Mock audit data
const mockAuditData: AuditEntry[] = [
  { id: '1', timestamp: Date.now() - 1000, userId: '1', userName: 'Sarah Chen', userEmail: 'sarah@company.com', action: 'create', entityType: 'employee', entityId: '5', entityName: 'John Doe', changes: [{ field: 'status', oldValue: 'inactive', newValue: 'active' }], ipAddress: '192.168.1.100' },
  { id: '2', timestamp: Date.now() - 5000, userId: '2', userName: 'James Wilson', userEmail: 'james@company.com', action: 'update', entityType: 'inventory', entityId: '10', entityName: 'Laptop Dell XPS 15', changes: [{ field: 'quantity', oldValue: '50', newValue: '45' }], ipAddress: '192.168.1.101' },
  { id: '3', timestamp: Date.now() - 15000, userId: '1', userName: 'Sarah Chen', userEmail: 'sarah@company.com', action: 'delete', entityType: 'order', entityId: '25', entityName: 'PO-2024-025', ipAddress: '192.168.1.100' },
  { id: '4', timestamp: Date.now() - 30000, userId: '3', userName: 'Maria Garcia', userEmail: 'maria@company.com', action: 'view', entityType: 'account', entityId: '8', entityName: 'Cash Account', ipAddress: '192.168.1.102' },
  { id: '5', timestamp: Date.now() - 60000, userId: '2', userName: 'James Wilson', userEmail: 'james@company.com', action: 'login', entityType: 'session', entityId: 'sess_123', entityName: 'Web Session', ipAddress: '192.168.1.101' },
  { id: '6', timestamp: Date.now() - 120000, userId: '1', userName: 'Sarah Chen', userEmail: 'sarah@company.com', action: 'update', entityType: 'employee', entityId: '3', entityName: 'Emily Johnson', changes: [{ field: 'department', oldValue: 'Marketing', newValue: 'Sales' }], ipAddress: '192.168.1.100' },
  { id: '7', timestamp: Date.now() - 300000, userId: '3', userName: 'Maria Garcia', userEmail: 'maria@company.com', action: 'create', entityType: 'journal', entityId: '15', entityName: 'JE-2024-015', ipAddress: '192.168.1.102' },
  { id: '8', timestamp: Date.now() - 600000, userId: '2', userName: 'James Wilson', userEmail: 'james@company.com', action: 'logout', entityType: 'session', entityId: 'sess_122', entityName: 'Web Session', ipAddress: '192.168.1.101' },
];

const actionConfig = {
  create: { icon: Plus, color: 'bg-green-100 text-green-600 dark:bg-green-900/30 dark:text-green-400', label: 'Created' },
  update: { icon: Edit2, color: 'bg-blue-100 text-blue-600 dark:bg-blue-900/30 dark:text-blue-400', label: 'Updated' },
  delete: { icon: Trash2, color: 'bg-red-100 text-red-600 dark:bg-red-900/30 dark:text-red-400', label: 'Deleted' },
  view: { icon: Eye, color: 'bg-slate-100 text-slate-600 dark:bg-slate-700 dark:text-slate-400', label: 'Viewed' },
  login: { icon: User, color: 'bg-purple-100 text-purple-600 dark:bg-purple-900/30 dark:text-purple-400', label: 'Logged In' },
  logout: { icon: User, color: 'bg-slate-100 text-slate-500 dark:bg-slate-700 dark:text-slate-400', label: 'Logged Out' },
};

function formatTimestamp(timestamp: number): string {
  const date = new Date(timestamp);
  return date.toLocaleString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function formatTimeAgo(timestamp: number): string {
  const diff = Date.now() - timestamp;
  const seconds = Math.floor(diff / 1000);
  const minutes = Math.floor(diff / 60000);
  const hours = Math.floor(diff / 3600000);
  const days = Math.floor(diff / 86400000);

  if (seconds < 60) return 'Just now';
  if (minutes < 60) return `${minutes}m ago`;
  if (hours < 24) return `${hours}h ago`;
  return `${days}d ago`;
}

export default function AuditPage() {
  const [search, setSearch] = useState('');
  const [filterAction, setFilterAction] = useState<string>('all');
  const [filterEntity, setFilterEntity] = useState<string>('all');
  const [dateRange, setDateRange] = useState<string>('all');
  const [expandedId, setExpandedId] = useState<string | null>(null);

  const filteredData = useMemo(() => {
    return mockAuditData.filter((entry) => {
      // Search filter
      if (search) {
        const searchLower = search.toLowerCase();
        const matchesSearch =
          entry.userName.toLowerCase().includes(searchLower) ||
          entry.entityName.toLowerCase().includes(searchLower) ||
          entry.entityType.toLowerCase().includes(searchLower) ||
          entry.userEmail.toLowerCase().includes(searchLower);
        if (!matchesSearch) return false;
      }

      // Action filter
      if (filterAction !== 'all' && entry.action !== filterAction) return false;

      // Entity filter
      if (filterEntity !== 'all' && entry.entityType !== filterEntity) return false;

      // Date filter
      if (dateRange !== 'all') {
        const diff = Date.now() - entry.timestamp;
        switch (dateRange) {
          case 'today':
            if (diff > 24 * 60 * 60 * 1000) return false;
            break;
          case 'week':
            if (diff > 7 * 24 * 60 * 60 * 1000) return false;
            break;
          case 'month':
            if (diff > 30 * 24 * 60 * 60 * 1000) return false;
            break;
        }
      }

      return true;
    });
  }, [search, filterAction, filterEntity, dateRange]);

  const exportAuditLog = () => {
    const columns = [
      { key: 'timestamp', header: 'Timestamp' },
      { key: 'userName', header: 'User' },
      { key: 'action', header: 'Action' },
      { key: 'entityType', header: 'Entity Type' },
      { key: 'entityName', header: 'Entity Name' },
      { key: 'changes', header: 'Changes' },
      { key: 'ipAddress', header: 'IP Address' },
    ];

    const data = filteredData.map((entry) => ({
      ...entry,
      timestamp: new Date(entry.timestamp).toISOString(),
      action: actionConfig[entry.action].label,
      changes: entry.changes?.map((c) => `${c.field}: ${c.oldValue} → ${c.newValue}`).join(', ') || '',
    }));

    downloadCSV(data as Record<string, unknown>[], columns as { key: keyof Record<string, unknown>; header: string }[], {
      filename: 'audit-log',
    });
  };

  const uniqueEntities = [...new Set(mockAuditData.map((e) => e.entityType))];

  return (
    <div className="space-y-6">
      <PageHeader
        title="Audit Trail"
        subtitle="Track all system changes and user activities"
        breadcrumbs={[
          { label: 'Dashboard', href: '/dashboard' },
          { label: 'Audit Trail' },
        ]}
        actions={
          <button
            onClick={exportAuditLog}
            className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 text-sm"
          >
            <Download className="w-4 h-4" />
            Export
          </button>
        }
      />

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <StatCard
          label="Total Events"
          value={filteredData.length}
          icon={Clock}
          color="bg-blue-500"
        />
        <StatCard
          label="Today"
          value={filteredData.filter((e) => Date.now() - e.timestamp < 86400000).length}
          icon={User}
          color="bg-green-500"
        />
        <StatCard
          label="This Week"
          value={filteredData.filter((e) => Date.now() - e.timestamp < 604800000).length}
          icon={Eye}
          color="bg-purple-500"
        />
        <StatCard
          label="Users Active"
          value={new Set(filteredData.map((e) => e.userId)).size}
          icon={User}
          color="bg-orange-500"
        />
      </div>

      {/* Filters */}
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-4">
        <div className="flex flex-wrap gap-4">
          {/* Search */}
          <div className="relative flex-1 min-w-[200px]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input
              type="text"
              placeholder="Search users, entities..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white placeholder-slate-400 focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* Action Filter */}
          <select
            value={filterAction}
            onChange={(e) => setFilterAction(e.target.value)}
            className="px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white"
          >
            <option value="all">All Actions</option>
            <option value="create">Created</option>
            <option value="update">Updated</option>
            <option value="delete">Deleted</option>
            <option value="view">Viewed</option>
            <option value="login">Login</option>
            <option value="logout">Logout</option>
          </select>

          {/* Entity Filter */}
          <select
            value={filterEntity}
            onChange={(e) => setFilterEntity(e.target.value)}
            className="px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white"
          >
            <option value="all">All Entities</option>
            {uniqueEntities.map((entity) => (
              <option key={entity} value={entity}>
                {entity.charAt(0).toUpperCase() + entity.slice(1)}
              </option>
            ))}
          </select>

          {/* Date Range */}
          <select
            value={dateRange}
            onChange={(e) => setDateRange(e.target.value)}
            className="px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white"
          >
            <option value="all">All Time</option>
            <option value="today">Today</option>
            <option value="week">This Week</option>
            <option value="month">This Month</option>
          </select>
        </div>
      </div>

      {/* Audit Table */}
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 overflow-hidden">
        {filteredData.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-16 text-slate-400">
            <Clock className="w-12 h-12 mb-4 opacity-50" />
            <p className="text-lg font-medium">No audit entries found</p>
            <p className="text-sm mt-1">Try adjusting your filters</p>
          </div>
        ) : (
          <div className="divide-y divide-slate-200 dark:divide-slate-700">
            {filteredData.map((entry) => {
              const config = actionConfig[entry.action];
              const Icon = config.icon;
              const isExpanded = expandedId === entry.id;

              return (
                <div key={entry.id}>
                  <button
                    onClick={() => setExpandedId(isExpanded ? null : entry.id)}
                    className="w-full px-4 py-3 flex items-center gap-4 hover:bg-slate-50 dark:hover:bg-slate-700/30 transition"
                  >
                    {/* Action Icon */}
                    <div className={cn('p-2 rounded-full shrink-0', config.color)}>
                      <Icon className="w-4 h-4" />
                    </div>

                    {/* Main Content */}
                    <div className="flex-1 min-w-0 text-left">
                      <div className="flex items-center gap-2 flex-wrap">
                        <span className="font-medium text-slate-900 dark:text-white">{entry.userName}</span>
                        <span className="text-slate-500">{config.label.toLowerCase()}</span>
                        <span className="text-slate-400">{entry.entityType}</span>
                        <span className="font-medium text-slate-800 dark:text-slate-200">{entry.entityName}</span>
                      </div>
                      <div className="flex items-center gap-3 mt-1 text-xs text-slate-400">
                        <span>{formatTimeAgo(entry.timestamp)}</span>
                        {entry.ipAddress && <span>IP: {entry.ipAddress}</span>}
                      </div>
                    </div>

                    {/* Expand Icon */}
                    <div className="text-slate-400">
                      {isExpanded ? (
                        <ChevronDown className="w-4 h-4" />
                      ) : (
                        <ChevronRight className="w-4 h-4" />
                      )}
                    </div>
                  </button>

                  {/* Expanded Details */}
                  {isExpanded && (
                    <div className="px-4 py-3 bg-slate-50 dark:bg-slate-900/50 border-t border-slate-100 dark:border-slate-800">
                      <div className="grid grid-cols-2 gap-4 text-sm">
                        <div>
                          <span className="text-slate-500">Timestamp</span>
                          <p className="font-medium text-slate-900 dark:text-white">
                            {new Date(entry.timestamp).toLocaleString()}
                          </p>
                        </div>
                        <div>
                          <span className="text-slate-500">Email</span>
                          <p className="font-medium text-slate-900 dark:text-white">{entry.userEmail}</p>
                        </div>
                        {entry.changes && entry.changes.length > 0 && (
                          <div className="col-span-2">
                            <span className="text-slate-500">Changes</span>
                            <div className="mt-1 space-y-1">
                              {entry.changes.map((change, i) => (
                                <div key={i} className="flex items-center gap-2">
                                  <span className="font-medium text-slate-700 dark:text-slate-300">{change.field}:</span>
                                  <span className="text-red-500 line-through">{change.oldValue}</span>
                                  <span className="text-slate-400">→</span>
                                  <span className="text-green-500">{change.newValue}</span>
                                </div>
                              ))}
                            </div>
                          </div>
                        )}
                        {entry.ipAddress && (
                          <div>
                            <span className="text-slate-500">IP Address</span>
                            <p className="font-mono text-slate-900 dark:text-white">{entry.ipAddress}</p>
                          </div>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}

function StatCard({
  label,
  value,
  icon: Icon,
  color,
}: {
  label: string;
  value: number;
  icon: typeof Clock;
  color: string;
}) {
  return (
    <div className="bg-white dark:bg-slate-800 rounded-xl p-5 border border-slate-200 dark:border-slate-700">
      <div className="flex items-center gap-3">
        <div className={cn('p-2.5 rounded-lg', color)}>
          <Icon className="w-5 h-5 text-white" />
        </div>
        <div>
          <p className="text-2xl font-bold text-slate-900 dark:text-white">{value}</p>
          <p className="text-sm text-slate-500">{label}</p>
        </div>
      </div>
    </div>
  );
}
