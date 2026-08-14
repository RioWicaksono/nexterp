/// <reference lib="webworker" />

const CACHE_NAME = 'nexterp-v1';
const OFFLINE_URL = '/offline';

const PRECACHE_ASSETS = [
  '/',
  '/offline',
  '/manifest.json',
  '/icons/icon-192x192.png',
  '/icons/icon-512x512.png',
];

// Install event - cache static assets
self.addEventListener('install', (event: ExtendableEvent) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => {
      return cache.addAll(PRECACHE_ASSETS);
    })
  );
  (self as unknown as ServiceWorkerGlobalScope).skipWaiting();
});

// Activate event - clean up old caches
self.addEventListener('activate', (event: ExtendableEvent) => {
  event.waitUntil(
    caches.keys().then((keys) => {
      return Promise.all(
        keys
          .filter((key) => key !== CACHE_NAME)
          .map((key) => caches.delete(key))
      );
    })
  );
  (self as unknown as ServiceWorkerGlobalScope).clients.claim();
});

// Fetch event - network first, fallback to cache
self.addEventListener('fetch', (event: FetchEvent) => {
  const { request } = event;

  // Skip non-GET requests
  if (request.method !== 'GET') return;

  // Skip API requests (always go to network)
  if (request.url.includes('/api/')) {
    event.respondWith(
      fetch(request).catch(() => {
        return caches.match('/offline');
      })
    );
    return;
  }

  // For page navigations, use network-first strategy
  if (request.mode === 'navigate') {
    event.respondWith(
      fetch(request)
        .then((response) => {
          // Clone and cache the response
          const responseClone = response.clone();
          caches.open(CACHE_NAME).then((cache) => {
            cache.put(request, responseClone);
          });
          return response;
        })
        .catch(() => {
          return caches.match(request).then((cached) => {
            return cached || caches.match(OFFLINE_URL);
          });
        })
    );
    return;
  }

  // For other assets, use cache-first strategy
  event.respondWith(
    caches.match(request).then((cached) => {
      if (cached) {
        // Return cached, but also update cache in background
        fetch(request).then((response) => {
          caches.open(CACHE_NAME).then((cache) => {
            cache.put(request, response);
          });
        });
        return cached;
      }

      return fetch(request).then((response) => {
        // Cache the response
        const responseClone = response.clone();
        caches.open(CACHE_NAME).then((cache) => {
          cache.put(request, responseClone);
        });
        return response;
      });
    })
  );
});

// Background sync for offline attendance submissions
self.addEventListener('sync', (event: SyncEvent) => {
  if (event.tag === 'attendance-sync') {
    event.waitUntil(syncAttendance());
  }
});

async function syncAttendance(): Promise<void> {
  try {
    // Get pending attendance records from IndexedDB
    const pendingRecords = await getPendingAttendanceRecords();

    for (const record of pendingRecords) {
      try {
        await fetch('/api/v1/mobile/attendance/' + record.type, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(record.data),
        });
        await removePendingRecord(record.id);
      } catch (error) {
        console.error('Failed to sync attendance record:', error);
      }
    }
  } catch (error) {
    console.error('Attendance sync failed:', error);
  }
}

async function getPendingAttendanceRecords(): Promise<Array<{ id: string; type: string; data: unknown }>> {
  // Placeholder - implement with IndexedDB
  return [];
}

async function removePendingRecord(id: string): Promise<void> {
  // Placeholder - implement with IndexedDB
}

// Push notification handling
self.addEventListener('push', (event: PushEvent) => {
  if (!event.data) return;

  const data = event.data.json();
  const options: NotificationOptions = {
    body: data.body,
    icon: '/icons/icon-192x192.png',
    badge: '/icons/badge.png',
    vibrate: [200, 100, 200],
    tag: data.tag || 'default',
    data: data.url,
    actions: data.actions || [],
  };

  event.waitUntil(
    (self as unknown as ServiceWorkerGlobalScope).registration.showNotification(
      data.title,
      options
    )
  );
});

// Notification click handling
self.addEventListener('notificationclick', (event: NotificationEvent) => {
  event.notification.close();

  const url = event.notification.data || '/';

  event.waitUntil(
    (self as unknown as ServiceWorkerGlobalScope).clients
      .matchAll({ type: 'window', includeUncontrolled: true })
      .then((clients) => {
        // Focus existing window if available
        for (const client of clients) {
          if (client.url === url && 'focus' in client) {
            return client.focus();
          }
        }
        // Otherwise open new window
        return (self as unknown as ServiceWorkerGlobalScope).clients.openWindow(url);
      })
  );
});

// Type declarations for service worker events
declare const self: ServiceWorkerGlobalScope;

interface SyncEvent extends ExtendableEvent {
  tag: string;
}

interface PushEvent extends ExtendableEvent {
  data: PushMessageData | null;
}

interface NotificationEvent extends ExtendableEvent {
  notification: Notification;
}

export {};
