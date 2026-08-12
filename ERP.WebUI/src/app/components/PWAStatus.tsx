"use client";

import { useState, useEffect } from "react";
import { usePWA } from "../../lib/pwa";
import { RefreshCw, Download, WifiOff, Wifi, X } from "lucide-react";

export function PWAStatus() {
  const { isOnline, isStandalone, updateAvailable, installPrompt, reloadForUpdate } = usePWA();
  const [showOnlineBanner, setShowOnlineBanner] = useState(false);

  // Show online banner when coming back online
  useEffect(() => {
    if (isOnline) {
      setShowOnlineBanner(true);

      // Auto-dismiss after 10 seconds
      const timer = setTimeout(() => {
        setShowOnlineBanner(false);
      }, 10000);

      return () => clearTimeout(timer);
    }
  }, [isOnline]);

  return (
    <>
      {/* Offline Banner */}
      {!isOnline && (
        <div className="fixed top-0 left-0 right-0 z-50 bg-amber-500 text-white py-2 px-4 text-center text-sm font-medium flex items-center justify-center gap-2">
          <WifiOff className="w-4 h-4" />
          You&apos;re offline. Some features may be limited.
        </div>
      )}

      {/* Online Banner (when comes back) - auto dismiss after 10s or click X */}
      {isOnline && showOnlineBanner && (
        <div className="fixed top-0 left-0 right-0 z-50 bg-emerald-500 text-white py-2 px-4 text-center text-sm font-medium flex items-center justify-center gap-2 animate-fade-in">
          <Wifi className="w-4 h-4" />
          Back online! All features are available.
          <button
            onClick={() => setShowOnlineBanner(false)}
            className="ml-2 p-1 rounded-full hover:bg-emerald-600 transition-colors"
            aria-label="Dismiss"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      {/* Update Available Banner */}
      {updateAvailable && (
        <div className="fixed bottom-20 left-4 right-4 md:left-auto md:right-4 md:w-80 z-50 bg-white dark:bg-slate-800 rounded-2xl shadow-2xl border border-slate-200 dark:border-slate-700 p-4 animate-slide-up">
          <div className="flex items-start gap-3">
            <div className="w-10 h-10 rounded-xl bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center flex-shrink-0">
              <RefreshCw className="w-5 h-5 text-blue-600 dark:text-blue-400" />
            </div>
            <div className="flex-1 min-w-0">
              <h4 className="font-semibold text-slate-800 dark:text-white text-sm">
                Update Available
              </h4>
              <p className="text-xs text-slate-500 dark:text-slate-400 mt-1">
                A new version is ready. Refresh to update.
              </p>
              <div className="flex gap-2 mt-3">
                <button
                  onClick={reloadForUpdate}
                  className="flex-1 py-2 px-3 rounded-lg bg-blue-600 hover:bg-blue-700 text-white text-xs font-medium transition-colors"
                >
                  Refresh Now
                </button>
                <button
                  onClick={() => {
                    /* Dismiss - will show again on next visit */
                  }}
                  className="py-2 px-3 rounded-lg bg-slate-100 dark:bg-slate-700 text-slate-600 dark:text-slate-300 text-xs font-medium transition-colors"
                >
                  Later
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Install App Banner */}
      {installPrompt && (
        <div className="fixed bottom-20 left-4 right-4 md:left-auto md:right-4 md:w-80 z-50 bg-white dark:bg-slate-800 rounded-2xl shadow-2xl border border-slate-200 dark:border-slate-700 p-4 animate-slide-up">
          <div className="flex items-start gap-3">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-blue-600 to-emerald-500 flex items-center justify-center flex-shrink-0">
              <Download className="w-5 h-5 text-white" />
            </div>
            <div className="flex-1 min-w-0">
              <h4 className="font-semibold text-slate-800 dark:text-white text-sm">
                Install NEXTERP App
              </h4>
              <p className="text-xs text-slate-500 dark:text-slate-400 mt-1">
                Install for a better experience with offline access.
              </p>
              <div className="flex gap-2 mt-3">
                <button
                  onClick={() => installPrompt.install()}
                  className="flex-1 py-2 px-3 rounded-lg bg-blue-600 hover:bg-blue-700 text-white text-xs font-medium transition-colors"
                >
                  Install
                </button>
                <button
                  onClick={() => installPrompt.dismiss()}
                  className="py-2 px-3 rounded-lg bg-slate-100 dark:bg-slate-700 text-slate-600 dark:text-slate-300 text-xs font-medium transition-colors"
                >
                  Not Now
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* PWA Install Button (for non-standalone) */}
      {!isStandalone && !installPrompt && (
        <div className="fixed bottom-4 right-4 z-40">
          <button
            onClick={() => {
              // Trigger install prompt
              document.dispatchEvent(new Event("beforeinstallprompt"));
            }}
            className="p-3 rounded-full bg-blue-600 hover:bg-blue-700 text-white shadow-lg hover:shadow-xl transition-all"
            aria-label="Install app"
          >
            <Download className="w-5 h-5" />
          </button>
        </div>
      )}
    </>
  );
}
