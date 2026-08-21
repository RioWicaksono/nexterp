'use client';

import { type ReactNode } from 'react';
import { NotificationBell } from './NotificationBell';
import { useNotificationPolling } from '@/hooks/useNotifications';

interface NotificationProviderProps {
  children: ReactNode;
  enablePolling?: boolean;
  pollingInterval?: number;
}

/**
 * Notification provider - wraps the app with notification bell and polling
 */
export function NotificationProvider({
  children,
  enablePolling = true,
  pollingInterval = 30000,
}: NotificationProviderProps) {
  // Enable polling if requested
  if (enablePolling) {
    useNotificationPolling(pollingInterval);
  }

  return (
    <>
      {children}
      {/* Global notification bell - can be placed in header */}
      <div className="fixed top-4 right-4 z-40">
        <NotificationBell />
      </div>
    </>
  );
}
