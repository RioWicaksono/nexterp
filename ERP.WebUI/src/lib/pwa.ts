"use client";

import { useEffect, useState } from "react";

interface PWAInstallPrompt {
  supported: boolean;
  canInstall: boolean;
  install: () => Promise<void>;
  dismiss: () => void;
}

interface PWAState {
  isOnline: boolean;
  isStandalone: boolean;
  updateAvailable: boolean;
  installPrompt: PWAInstallPrompt | null;
}

export function usePWA() {
  const [state, setState] = useState<PWAState>({
    isOnline: true,
    isStandalone: false,
    updateAvailable: false,
    installPrompt: null,
  });

  useEffect(() => {
    // Check if already in standalone mode
    const checkStandalone = () => {
      const isStandalone =
        window.matchMedia("(display-mode: standalone)").matches ||
        (window.navigator as Navigator & { standalone?: boolean }).standalone === true;
      setState((prev) => ({ ...prev, isStandalone }));
    };

    checkStandalone();

    // Online/Offline detection
    const handleOnline = () => setState((prev) => ({ ...prev, isOnline: true }));
    const handleOffline = () => setState((prev) => ({ ...prev, isOnline: false }));

    window.addEventListener("online", handleOnline);
    window.addEventListener("offline", handleOffline);

    // PWA Install prompt
    const handleInstallPrompt = (e: Event) => {
      e.preventDefault();
      const installPrompt = e as EnterPromptEvent;

      setState((prev) => ({
        ...prev,
        installPrompt: {
          supported: true,
          canInstall: true,
          install: async () => {
            installPrompt.prompt();
            const { outcome } = await installPrompt.userChoice;
            if (outcome === "accepted") {
              setState((prev) => ({ ...prev, installPrompt: null }));
            }
          },
          dismiss: () => {
            setState((prev) => ({ ...prev, installPrompt: null }));
          },
        },
      }));
    };

    window.addEventListener("beforeinstallprompt", handleInstallPrompt);

    // Service Worker Update detection
    if ("serviceWorker" in navigator) {
      navigator.serviceWorker.ready.then((registration) => {
        registration.addEventListener("updatefound", () => {
          const newWorker = registration.installing;
          if (newWorker) {
            newWorker.addEventListener("statechange", () => {
              if (
                newWorker.state === "installed" &&
                navigator.serviceWorker.controller
              ) {
                setState((prev) => ({ ...prev, updateAvailable: true }));
              }
            });
          }
        });
      });
    }

    return () => {
      window.removeEventListener("online", handleOnline);
      window.removeEventListener("offline", handleOffline);
      window.removeEventListener("beforeinstallprompt", handleInstallPrompt);
    };
  }, []);

  const reloadForUpdate = () => {
    window.location.reload();
  };

  return {
    ...state,
    reloadForUpdate,
  };
}

interface EnterPromptEvent extends Event {
  prompt(): Promise<{ outcome: "accepted" | "dismissed" }>;
  userChoice: Promise<{ outcome: "accepted" | "dismissed" }>;
}
