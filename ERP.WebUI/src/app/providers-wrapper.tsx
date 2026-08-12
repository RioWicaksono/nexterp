"use client";

import { ThemeProvider } from "./providers/ThemeProvider";
import { ToastProvider } from "./providers/ToastProvider";
import { I18nProvider } from "./providers/I18nProvider";
import { ToastContainer } from "./components/Toast";
import { PWAStatus } from "./components/PWAStatus";

export function Providers({ children }: { children: React.ReactNode }) {
  return (
    <ThemeProvider>
      <ToastProvider>
        <I18nProvider>
          <ToastContainer />
          <PWAStatus />
          {children}
        </I18nProvider>
      </ToastProvider>
    </ThemeProvider>
  );
}
