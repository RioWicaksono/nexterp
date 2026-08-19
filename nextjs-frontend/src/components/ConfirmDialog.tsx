'use client';

import { useEffect, useRef, useCallback } from 'react';
import { X, AlertTriangle, Trash2, AlertCircle } from 'lucide-react';
import { clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

export type ConfirmVariant = 'danger' | 'warning' | 'default';

export interface ConfirmDialogProps {
  isOpen: boolean;
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  onConfirm: () => void;
  onCancel: () => void;
  variant?: ConfirmVariant;
}

const variantConfig: Record<ConfirmVariant, { icon: React.ElementType; button: string }> = {
  danger: {
    icon: Trash2,
    button: 'bg-red-500 hover:bg-red-600 focus:ring-red-500',
  },
  warning: {
    icon: AlertTriangle,
    button: 'bg-yellow-500 hover:bg-yellow-600 focus:ring-yellow-500',
  },
  default: {
    icon: AlertCircle,
    button: 'bg-slate-600 hover:bg-slate-700 focus:ring-slate-500',
  },
};

export function ConfirmDialog({
  isOpen,
  title,
  message,
  confirmText = 'Delete',
  cancelText = 'Cancel',
  onConfirm,
  onCancel,
  variant = 'danger',
}: ConfirmDialogProps) {
  const dialogRef = useRef<HTMLDivElement>(null);
  const confirmButtonRef = useRef<HTMLButtonElement>(null);
  const { icon: Icon, button: buttonClass } = variantConfig[variant];

  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        onCancel();
      }
    },
    [onCancel]
  );

  const handleBackdropClick = useCallback(
    (e: React.MouseEvent) => {
      if (e.target === e.currentTarget) {
        onCancel();
      }
    },
    [onCancel]
  );

  useEffect(() => {
    if (isOpen) {
      document.addEventListener('keydown', handleKeyDown);
      confirmButtonRef.current?.focus();
      document.body.style.overflow = 'hidden';
    }
    return () => {
      document.removeEventListener('keydown', handleKeyDown);
      document.body.style.overflow = '';
    };
  }, [isOpen, handleKeyDown]);

  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
      onClick={handleBackdropClick}
    >
      {/* Backdrop */}
      <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" />

      {/* Dialog */}
      <div
        ref={dialogRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby="dialog-title"
        className={twMerge(
          clsx(
            'relative bg-white dark:bg-slate-800 rounded-xl shadow-xl',
            'w-full max-w-md p-6 animate-in fade-in zoom-in-95 duration-200'
          )
        )}
      >
        {/* Close button */}
        <button
          onClick={onCancel}
          className="absolute top-4 right-4 text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 transition-colors"
          aria-label="Close"
        >
          <X className="w-5 h-5" />
        </button>

        {/* Icon */}
        <div className={twMerge(
          clsx(
            'flex items-center justify-center w-12 h-12 rounded-full mb-4',
            variant === 'danger' && 'bg-red-100 dark:bg-red-900/30',
            variant === 'warning' && 'bg-yellow-100 dark:bg-yellow-900/30',
            variant === 'default' && 'bg-slate-100 dark:bg-slate-700'
          )
        )}>
          <Icon
            className={twMerge(
              clsx('w-6 h-6',
                variant === 'danger' && 'text-red-500',
                variant === 'warning' && 'text-yellow-500',
                variant === 'default' && 'text-slate-500'
              )
            )}
          />
        </div>

        {/* Content */}
        <h2
          id="dialog-title"
          className="text-lg font-semibold text-slate-900 dark:text-white mb-2"
        >
          {title}
        </h2>
        <p className="text-slate-600 dark:text-slate-300 text-sm mb-6">
          {message}
        </p>

        {/* Actions */}
        <div className="flex justify-end gap-3">
          <button
            onClick={onCancel}
            className={twMerge(
              clsx(
                'px-4 py-2 text-sm font-medium rounded-lg',
                'text-slate-700 dark:text-slate-200',
                'bg-slate-100 dark:bg-slate-700',
                'hover:bg-slate-200 dark:hover:bg-slate-600',
                'transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-slate-500'
              )
            )}
          >
            {cancelText}
          </button>
          <button
            ref={confirmButtonRef}
            onClick={onConfirm}
            className={twMerge(
              clsx(
                'px-4 py-2 text-sm font-medium rounded-lg text-white',
                'transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2',
                buttonClass
              )
            )}
          >
            {confirmText}
          </button>
        </div>
      </div>
    </div>
  );
}
