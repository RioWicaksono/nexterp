'use client';

import { useMemo } from 'react';
import { useActivityStore } from '@/stores/activityStore';
import { PageHeader } from '@/components/PageHeader';
import { Clock, User, FileText, ShoppingCart, Package, DollarSign, LogIn, LogOut, Edit2, Trash2, Plus, CheckCircle, XCircle } from 'lucide-react';
import { cn } from '@/lib/utils';

const actionIcons: Record<string, typeof User> = {
  login: LogIn,
  logout: LogOut,
  create: Plus,
  update: Edit2,
  delete: Trash2,
  view: Clock,
  approve: CheckCircle,
  reject: XCircle,
};

const entityIcons: Record<string, typeof User> = {
  employee: User,
  inventory: Package,
  order: ShoppingCart,
  supplier: Package,
  account: DollarSign,
  journal: FileText,
};

function formatTime(timestamp: number): string {
  const now = Date.now();
  const diff = now - timestamp;
  const seconds = Math.floor(diff / 1000);
  const minutes = Math.floor(diff / 60000);
  const hours = Math.floor(diff / 3600000);
  const days = Math.floor(diff / 86400000);

  if (seconds < 60) return 'Just now';
  if (minutes < 60) return `${minutes} minute${minutes > 1 ? 's' : ''} ago`;
  if (hours < 24) return `${hours} hour${hours > 1 ? 's' : ''} ago`;
  if (days < 7) return `${days} day${days > 1 ? 's' : ''} ago`;
  return new Date(timestamp).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function getActionColor(action: string): string {
  switch (action) {
    case 'create':
      return 'bg-green-100 dark:bg-green-900/30 text-green-600 dark:text-green-400';
    case 'update':
      return 'bg-blue-100 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400';
    case 'delete':
      return 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400';
    case 'approve':
      return 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-600 dark:text-emerald-400';
    case 'reject':
      return 'bg-orange-100 dark:bg-orange-900/30 text-orange-600 dark:text-orange-400';
    case 'login':
      return 'bg-purple-100 dark:bg-purple-900/30 text-purple-600 dark:text-purple-400';
    case 'logout':
      return 'bg-slate-100 dark:bg-slate-700 text-slate-600 dark:text-slate-400';
    default:
      return 'bg-slate-100 dark:bg-slate-700 text-slate-600 dark:text-slate-400';
  }
}

export default function ActivityPage() {
  const { activities, clearActivities } = useActivityStore();

  const groupedActivities = useMemo(() => {
    const groups: Record<string, typeof activities> = {};
    activities.forEach((activity) => {
      const date = new Date(activity.timestamp).toLocaleDateString('en-US', {
        weekday: 'long',
        month: 'long',
        day: 'numeric',
      });
      if (!groups[date]) {
        groups[date] = [];
      }
      groups[date].push(activity);
    });
    return groups;
  }, [activities]);

  return (
    <div className="space-y-6">
      <PageHeader
        title="Activity Log"
        subtitle="Track all user activities and system events"
        breadcrumbs={[
          { label: 'Dashboard', href: '/dashboard' },
          { label: 'Activity Log' },
        ]}
        actions={
          activities.length > 0 && (
            <button
              onClick={() => {
                if (confirm('Clear all activity logs?')) {
                  clearActivities();
                }
              }}
              className="px-3 py-2 text-sm text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition"
            >
              Clear All
            </button>
          )
        }
      />

      {/* Activity Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <StatCard
          label="Total Activities"
          value={activities.length}
          icon={Clock}
          color="bg-blue-500"
        />
        <StatCard
          label="Today"
          value={activities.filter((a) => {
            const today = new Date();
            const actDate = new Date(a.timestamp);
            return actDate.toDateString() === today.toDateString();
          }).length}
          icon={User}
          color="bg-green-500"
        />
        <StatCard
          label="This Week"
          value={activities.filter((a) => {
            const weekAgo = Date.now() - 7 * 24 * 60 * 60 * 1000;
            return a.timestamp > weekAgo;
          }).length}
          icon={FileText}
          color="bg-purple-500"
        />
        <StatCard
          label="Unique Users"
          value={new Set(activities.map((a) => a.userId)).size}
          icon={User}
          color="bg-orange-500"
        />
      </div>

      {/* Activity Timeline */}
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 overflow-hidden">
        {activities.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-16 text-slate-400">
            <Clock className="w-12 h-12 mb-4 opacity-50" />
            <p className="text-lg font-medium">No activity recorded yet</p>
            <p className="text-sm mt-1">Activities will appear here as users interact with the system</p>
          </div>
        ) : (
          <div className="divide-y divide-slate-200 dark:divide-slate-700">
            {Object.entries(groupedActivities).map(([date, dayActivities]) => (
              <div key={date}>
                <div className="px-4 py-2 bg-slate-50 dark:bg-slate-900/50">
                  <h3 className="text-sm font-medium text-slate-600 dark:text-slate-400">{date}</h3>
                </div>
                <div className="divide-y divide-slate-100 dark:divide-slate-800">
                  {dayActivities.map((activity) => (
                    <ActivityRow key={activity.id} activity={activity} />
                  ))}
                </div>
              </div>
            ))}
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
  icon: typeof User;
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

function ActivityRow({ activity }: { activity: ReturnType<typeof useActivityStore.getState>['activities'][0] }) {
  const ActionIcon = actionIcons[activity.action] || Clock;
  const EntityIcon = entityIcons[activity.entityType] || FileText;

  return (
    <div className="px-4 py-3 hover:bg-slate-50 dark:hover:bg-slate-700/30 transition">
      <div className="flex items-start gap-4">
        {/* Action Icon */}
        <div className={cn('p-2 rounded-full shrink-0', getActionColor(activity.action))}>
          <ActionIcon className="w-4 h-4" />
        </div>

        {/* Content */}
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 flex-wrap">
            <span className="font-medium text-slate-900 dark:text-white">{activity.userName}</span>
            <span className="text-slate-500">{activity.action}</span>
            <span className="flex items-center gap-1 text-slate-600 dark:text-slate-400">
              <EntityIcon className="w-3.5 h-3.5" />
              {activity.entityType}
            </span>
            <span className="font-medium text-slate-800 dark:text-slate-200">{activity.entityName}</span>
          </div>
          {activity.details && (
            <p className="text-sm text-slate-500 mt-1">{activity.details}</p>
          )}
          <div className="flex items-center gap-3 mt-1 text-xs text-slate-400">
            <span>{formatTime(activity.timestamp)}</span>
            {activity.ipAddress && <span>IP: {activity.ipAddress}</span>}
          </div>
        </div>
      </div>
    </div>
  );
}
