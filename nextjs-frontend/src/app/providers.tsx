'use client';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useState, useEffect } from 'react';
import { ToastProvider } from '@/components/ToastProvider';

function ToastProviderWrapper({ children }: { children: React.ReactNode }) {
  return <ToastProvider>{children}</ToastProvider>;
}

export function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 60 * 1000,
            retry: 1,
          },
        },
      })
  );

  useEffect(() => {
    if (process.env.NODE_ENV !== 'development') return;

    const enableMocks =
      process.env.NEXT_PUBLIC_MSW_ENABLED === 'true' ||
      new URLSearchParams(window.location.search).get('msw') === 'true';

    if (!enableMocks) return;

    async function initMocks() {
      try {
        const { worker } = await import('@/mocks/browser');
        await worker.start({
          onUnhandledRequest: 'bypass',
        });
        console.info('[MSW] Service worker started - API mocking enabled');
      } catch (err) {
        console.warn('[MSW] Failed to start:', err);
      }
    }

    initMocks();
  }, []);

  return (
    <QueryClientProvider client={queryClient}>
      <ToastProviderWrapper>{children}</ToastProviderWrapper>
    </QueryClientProvider>
  );
}
