/**
 * Sentry Error Tracking Initialization
 * Add your Sentry DSN to environment variables:
 * NEXT_PUBLIC_SENTRY_DSN
 *
 * Usage:
 * - In your app: import { initSentry } from '@/lib/sentry'
 * - Call initSentry() once at app startup
 */

declare global {
  interface Window {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    Sentry?: any;
  }
}

export async function initSentry(): Promise<void> {
  if (typeof window === 'undefined') return;

  const dsn = process.env.NEXT_PUBLIC_SENTRY_DSN;
  if (!dsn) {
    console.info('[Sentry] DSN not configured, skipping initialization');
    return;
  }

  try {
    // Dynamic import Sentry only when configured
    const Sentry = await import('@sentry/browser' as string);
    const { BrowserTracing } = await import('@sentry/browser' as string);

    Sentry.init({
      dsn,
      environment: process.env.NODE_ENV,
      tracesSampleRate: process.env.NODE_ENV === 'production' ? 0.1 : 1.0,
      replaysOnErrorSampleRate: 1.0,
      attachStacktrace: true,
    });

    console.info('[Sentry] Initialized successfully');
  } catch (error) {
    console.warn('[Sentry] Failed to initialize:', error);
  }
}

export function setSentryUser(user: { id: string; email?: string; username?: string }): void {
  if (window.Sentry) {
    window.Sentry.setUser({
      id: user.id,
      email: user.email,
      username: user.username,
    });
  }
}

export function clearSentryUser(): void {
  if (window.Sentry) {
    window.Sentry.setUser(null);
  }
}

export function captureSentryException(error: Error): void {
  if (window.Sentry) {
    window.Sentry.captureException(error);
  }
}
