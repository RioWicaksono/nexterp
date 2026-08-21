'use client';

import { useCallback, useEffect } from 'react';
import { useNotificationStore, type NotificationType } from '@/stores/notificationStore';

export function useNotifications() {
  const store = useNotificationStore();

  const notify = useCallback(
    (type: NotificationType, title: string, message?: string, link?: string) => {
      store.addNotification({ type, title, message, link });
    },
    [store]
  );

  const info = useCallback(
    (title: string, message?: string) => notify('info', title, message),
    [notify]
  );

  const success = useCallback(
    (title: string, message?: string) => notify('success', title, message),
    [notify]
  );

  const warning = useCallback(
    (title: string, message?: string) => notify('warning', title, message),
    [notify]
  );

  const error = useCallback(
    (title: string, message?: string) => notify('error', title, message),
    [notify]
  );

  return {
    notifications: store.notifications,
    unreadCount: store.unreadCount,
    markAsRead: store.markAsRead,
    markAllAsRead: store.markAllAsRead,
    removeNotification: store.removeNotification,
    clearAll: store.clearAll,
    notify,
    info,
    success,
    warning,
    error,
  };
}

/**
 * Hook to poll for new notifications (simulated)
 * In production, this would call your notification API
 */
export function useNotificationPolling(intervalMs = 30000) {
  const { addNotification } = useNotificationStore();

  useEffect(() => {
    // Simulated polling - in production, replace with actual API call
    const poll = () => {
      // Example: Random notification every few polls (for demo)
      if (Math.random() < 0.1) {
        const types: NotificationType[] = ['info', 'success', 'warning', 'error'];
        const messages = [
          { title: 'New Employee Added', message: 'Sarah Chen has been added to Engineering' },
          { title: 'Low Stock Alert', message: 'Item ITM-001 is below reorder level' },
          { title: 'PO Approved', message: 'Purchase Order #PO-2024-001 has been approved' },
          { title: 'System Update', message: 'Scheduled maintenance tonight at 2 AM' },
        ];
        const randomMsg = messages[Math.floor(Math.random() * messages.length)];
        const randomType = types[Math.floor(Math.random() * types.length)];

        addNotification({
          type: randomType,
          title: randomMsg.title,
          message: randomMsg.message,
        });
      }
    };

    const interval = setInterval(poll, intervalMs);
    return () => clearInterval(interval);
  }, [addNotification, intervalMs]);
}
