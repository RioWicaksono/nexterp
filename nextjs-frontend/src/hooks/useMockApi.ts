'use client';

import { useEffect, useState } from 'react';

let workerStarted = false;

export function useMockApi() {
  const [isMocking, setIsMocking] = useState(false);
  const [isReady, setIsReady] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (typeof window === 'undefined' || workerStarted) return;

    // Only enable mocking in development
    if (process.env.NODE_ENV !== 'development') {
      return;
    }

    // Check for MSW_ENABLED environment variable or URL param
    const urlParams = new URLSearchParams(window.location.search);
    const mswEnabled = urlParams.get('msw') === 'true' ||
                       process.env.NEXT_PUBLIC_MSW_ENABLED === 'true';

    if (!mswEnabled) return;

    async function startWorker() {
      try {
        const { worker } = await import('@/mocks/browser');
        await worker.start({
          onUnhandledRequest: 'bypass',
          serviceWorker: {
            url: '/msw.js',
          },
        });
        workerStarted = true;
        setIsMocking(true);
        setIsReady(true);
      } catch (err) {
        console.warn('MSW: Failed to start service worker', err);
        setError(err instanceof Error ? err.message : 'Failed to start MSW');
      }
    }

    startWorker();
  }, []);

  return { isMocking, isReady, error };
}
