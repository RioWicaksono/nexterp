'use client';

import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export interface ActivityItem {
  id: string;
  userId: string;
  userName: string;
  action: string;
  entityType: string;
  entityId: string;
  entityName: string;
  details?: string;
  ipAddress?: string;
  userAgent?: string;
  timestamp: number;
}

interface ActivityState {
  activities: ActivityItem[];
  addActivity: (activity: Omit<ActivityItem, 'id' | 'timestamp'>) => void;
  clearActivities: () => void;
  getActivitiesByUser: (userId: string) => ActivityItem[];
  getRecentActivities: (limit?: number) => ActivityItem[];
}

let activityIdCounter = 0;

export const useActivityStore = create<ActivityState>()(
  persist(
    (set, get) => ({
      activities: [],

      addActivity: (activity) => {
        const newActivity: ActivityItem = {
          ...activity,
          id: `activity_${Date.now()}_${++activityIdCounter}`,
          timestamp: Date.now(),
        };

        set((state) => ({
          activities: [newActivity, ...state.activities].slice(0, 500), // Keep last 500
        }));
      },

      clearActivities: () => {
        set({ activities: [] });
      },

      getActivitiesByUser: (userId) => {
        return get().activities.filter((a) => a.userId === userId);
      },

      getRecentActivities: (limit = 50) => {
        return get().activities.slice(0, limit);
      },
    }),
    {
      name: 'nexterp-activity-log',
    }
  )
);

// Helper to log activity
export function logActivity(
  action: string,
  entityType: string,
  entityId: string,
  entityName: string,
  options?: {
    userId?: string;
    userName?: string;
    details?: string;
    ipAddress?: string;
    userAgent?: string;
  }
) {
  const { addActivity } = useActivityStore.getState();
  addActivity({
    action,
    entityType,
    entityId,
    entityName,
    userId: options?.userId || 'system',
    userName: options?.userName || 'System',
    details: options?.details,
    ipAddress: options?.ipAddress,
    userAgent: options?.userAgent,
  });
}
