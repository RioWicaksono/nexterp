'use client';

import { Clock, User, FileText, ShoppingCart, CheckCircle, AlertCircle } from 'lucide-react';

interface Activity {
  id: string;
  type: 'user' | 'order' | 'document' | 'approval' | 'alert';
  message: string;
  timestamp: string;
  user?: string;
}

interface RecentActivityProps {
  activities?: Activity[];
  isLoading?: boolean;
}

const getActivityIcon = (type: Activity['type']) => {
  switch (type) {
    case 'user':
      return User;
    case 'order':
      return ShoppingCart;
    case 'document':
      return FileText;
    case 'approval':
      return CheckCircle;
    case 'alert':
      return AlertCircle;
    default:
      return Clock;
  }
};

const getActivityColor = (type: Activity['type']) => {
  switch (type) {
    case 'user':
      return 'bg-blue-100 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400';
    case 'order':
      return 'bg-orange-100 dark:bg-orange-900/30 text-orange-600 dark:text-orange-400';
    case 'document':
      return 'bg-green-100 dark:bg-green-900/30 text-green-600 dark:text-green-400';
    case 'approval':
      return 'bg-purple-100 dark:bg-purple-900/30 text-purple-600 dark:text-purple-400';
    case 'alert':
      return 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400';
    default:
      return 'bg-slate-100 dark:bg-slate-700 text-slate-600 dark:text-slate-400';
  }
};

const defaultActivities: Activity[] = [
  { id: '1', type: 'order', message: 'Purchase Order #PO-2024-001 approved', timestamp: '5 min ago', user: 'Sarah Chen' },
  { id: '2', type: 'user', message: 'New employee John Doe added to HRM', timestamp: '15 min ago', user: 'HR Admin' },
  { id: '3', type: 'document', message: 'Invoice #INV-2024-045 created', timestamp: '30 min ago', user: 'Finance' },
  { id: '4', type: 'approval', message: 'Budget request BR-2024-012 approved', timestamp: '1 hour ago', user: 'Manager' },
  { id: '5', type: 'order', message: 'New supplier contract signed: TechParts Inc', timestamp: '2 hours ago', user: 'Procurement' },
  { id: '6', type: 'alert', message: 'Low stock alert: Item ITM-001 below threshold', timestamp: '3 hours ago' },
];

export function RecentActivity({ activities, isLoading }: RecentActivityProps) {
  const items = activities || defaultActivities;

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-3">
        {Array.from({ length: 5 }).map((_, i) => (
          <div key={i} className="flex items-center gap-3">
            <div className="w-8 h-8 bg-slate-200 dark:bg-slate-700 rounded-full" />
            <div className="flex-1">
              <div className="h-4 bg-slate-200 dark:bg-slate-700 rounded w-3/4" />
              <div className="h-3 bg-slate-200 dark:bg-slate-700 rounded w-1/4 mt-1" />
            </div>
          </div>
        ))}
      </div>
    );
  }

  return (
    <div className="space-y-3 max-h-80 overflow-y-auto">
      {items.map((activity) => {
        const Icon = getActivityIcon(activity.type);
        return (
          <div
            key={activity.id}
            className="flex items-start gap-3 p-2 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700/50 transition"
          >
            <div className={`p-2 rounded-full ${getActivityColor(activity.type)} flex-shrink-0`}>
              <Icon className="w-3.5 h-3.5" />
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm text-slate-900 dark:text-white">{activity.message}</p>
              <div className="flex items-center gap-2 mt-0.5">
                {activity.user && (
                  <span className="text-xs text-slate-500">{activity.user}</span>
                )}
                <span className="text-xs text-slate-400 flex items-center gap-1">
                  <Clock className="w-3 h-3" />
                  {activity.timestamp}
                </span>
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}
