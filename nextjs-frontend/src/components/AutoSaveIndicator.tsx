'use client';

import { Check, Loader2, RotateCcw, AlertCircle, Cloud } from 'lucide-react';
import { cn } from '@/lib/utils';

export type AutoSaveStatus = 'idle' | 'saving' | 'saved' | 'restored' | 'error';

interface AutoSaveIndicatorProps {
  status: AutoSaveStatus;
  lastSavedAt: number | null;
  hasDraft: boolean;
  onRestore?: () => void;
  onClear?: () => void;
  className?: string;
}

function formatTime(timestamp: number): string {
  const now = Date.now();
  const diff = now - timestamp;
  const seconds = Math.floor(diff / 1000);
  const minutes = Math.floor(diff / 60000);
  const hours = Math.floor(diff / 3600000);

  if (seconds < 10) return 'just now';
  if (seconds < 60) return `${seconds}s ago`;
  if (minutes < 60) return `${minutes}m ago`;
  if (hours < 24) return `${hours}h ago`;
  return new Date(timestamp).toLocaleDateString();
}

export function AutoSaveIndicator({
  status,
  lastSavedAt,
  hasDraft,
  onRestore,
  onClear,
  className,
}: AutoSaveIndicatorProps) {
  if (status === 'idle' && !hasDraft) return null;

  return (
    <div className={cn('flex items-center gap-2', className)}>
      {/* Status Icon */}
      <div className="flex items-center gap-1.5">
        {status === 'saving' && (
          <>
            <Loader2 className="w-3.5 h-3.5 text-slate-400 animate-spin" />
            <span className="text-xs text-slate-400">Saving...</span>
          </>
        )}
        {status === 'saved' && (
          <>
            <Check className="w-3.5 h-3.5 text-green-500" />
            <span className="text-xs text-slate-500">
              Saved {lastSavedAt ? formatTime(lastSavedAt) : ''}
            </span>
          </>
        )}
        {status === 'restored' && (
          <>
            <RotateCcw className="w-3.5 h-3.5 text-blue-500" />
            <span className="text-xs text-blue-500">Draft restored</span>
          </>
        )}
        {status === 'error' && (
          <>
            <AlertCircle className="w-3.5 h-3.5 text-red-500" />
            <span className="text-xs text-red-500">Save failed</span>
          </>
        )}
        {status === 'idle' && hasDraft && (
          <>
            <Cloud className="w-3.5 h-3.5 text-slate-400" />
            <span className="text-xs text-slate-500">
              Draft saved {lastSavedAt ? formatTime(lastSavedAt) : ''}
            </span>
          </>
        )}
      </div>

      {/* Actions */}
      {hasDraft && status !== 'saving' && (
        <div className="flex items-center gap-1">
          {onRestore && (
            <button
              onClick={onRestore}
              className="text-xs text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300 transition"
              title="Restore draft"
            >
              Restore
            </button>
          )}
          {onClear && (
            <>
              <span className="text-slate-300 dark:text-slate-600">|</span>
              <button
                onClick={onClear}
                className="text-xs text-slate-400 hover:text-slate-600 dark:text-slate-500 dark:hover:text-slate-300 transition"
                title="Clear draft"
              >
                Clear
              </button>
            </>
          )}
        </div>
      )}
    </div>
  );
}
