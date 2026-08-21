'use client';

import { useState, useRef, useCallback } from 'react';
import { Pencil, Check, X } from 'lucide-react';
import { cn } from '@/lib/utils';

interface InlineEditProps {
  value: string | number;
  onSave: (value: string) => Promise<void> | void;
  type?: 'text' | 'number' | 'email';
  className?: string;
  disabled?: boolean;
  placeholder?: string;
  validate?: (value: string) => string | null;
}

export function InlineEdit({
  value,
  onSave,
  type = 'text',
  className,
  disabled = false,
  placeholder = 'Click to edit',
  validate,
}: InlineEditProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [editValue, setEditValue] = useState(String(value));
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const startEditing = useCallback(() => {
    if (disabled) return;
    setIsEditing(true);
    setEditValue(String(value));
    setError(null);
    // Focus input after render
    setTimeout(() => inputRef.current?.select(), 0);
  }, [disabled, value]);

  const cancelEditing = useCallback(() => {
    setIsEditing(false);
    setEditValue(String(value));
    setError(null);
  }, [value]);

  const saveValue = useCallback(async () => {
    // Validate
    if (validate) {
      const validationError = validate(editValue);
      if (validationError) {
        setError(validationError);
        return;
      }
    }

    // Check if value changed
    if (editValue === String(value)) {
      setIsEditing(false);
      return;
    }

    setIsSaving(true);
    try {
      await onSave(editValue);
      setIsEditing(false);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save');
    } finally {
      setIsSaving(false);
    }
  }, [editValue, value, onSave, validate]);

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      saveValue();
    } else if (e.key === 'Escape') {
      e.preventDefault();
      cancelEditing();
    }
  };

  if (!isEditing) {
    return (
      <div
        className={cn(
          'group inline-flex items-center gap-2 cursor-pointer',
          disabled && 'cursor-not-allowed opacity-50',
          className
        )}
        onDoubleClick={startEditing}
      >
        <span className={cn(
          'px-2 py-1 -mx-2 rounded',
          !disabled && 'group-hover:bg-slate-100 dark:group-hover:bg-slate-700'
        )}>
          {value || <span className="text-slate-400 italic">{placeholder}</span>}
        </span>
        {!disabled && (
          <Pencil className="w-3.5 h-3.5 text-slate-400 opacity-0 group-hover:opacity-100 transition-opacity" />
        )}
      </div>
    );
  }

  return (
    <div className={cn('inline-flex items-center gap-1', className)}>
      <input
        ref={inputRef}
        type={type}
        value={editValue}
        onChange={(e) => setEditValue(e.target.value)}
        onKeyDown={handleKeyDown}
        disabled={isSaving}
        className={cn(
          'px-2 py-1 text-sm border rounded focus:outline-none focus:ring-2 focus:ring-blue-500 w-full',
          error
            ? 'border-red-500 focus:border-red-500'
            : 'border-slate-300 dark:border-slate-600',
          'dark:bg-slate-700 dark:text-white'
        )}
        autoFocus
      />
      <button
        onClick={saveValue}
        disabled={isSaving}
        className="p-1 text-green-600 hover:bg-green-50 dark:hover:bg-green-900/20 rounded transition disabled:opacity-50"
        title="Save (Enter)"
      >
        <Check className="w-4 h-4" />
      </button>
      <button
        onClick={cancelEditing}
        disabled={isSaving}
        className="p-1 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded transition disabled:opacity-50"
        title="Cancel (Esc)"
      >
        <X className="w-4 h-4" />
      </button>
      {error && (
        <span className="text-xs text-red-500 mt-1">{error}</span>
      )}
    </div>
  );
}
