'use client';

import { useState, useCallback } from 'react';
import { Trash2, Edit2, CheckSquare, Square, X } from 'lucide-react';
import { cn } from '@/lib/utils';
import { ConfirmDialog } from './ConfirmDialog';

export interface BulkAction<T> {
  id: string;
  label: string;
  icon?: typeof Trash2;
  variant?: 'default' | 'danger';
  onClick: (selectedItems: T[]) => Promise<void> | void;
}

interface BulkActionsBarProps<T> {
  selectedItems: T[];
  actions: BulkAction<T>[];
  onClearSelection: () => void;
  itemLabel?: string;
}

export function BulkActionsBar<T extends { id: string }>({
  selectedItems,
  actions,
  onClearSelection,
  itemLabel = 'items',
}: BulkActionsBarProps<T>) {
  const [showConfirm, setShowConfirm] = useState<BulkAction<T> | null>(null);
  const [isProcessing, setIsProcessing] = useState(false);

  const handleAction = useCallback(async (action: BulkAction<T>) => {
    if (action.variant === 'danger') {
      setShowConfirm(action);
    } else {
      setIsProcessing(true);
      try {
        await action.onClick(selectedItems);
      } finally {
        setIsProcessing(false);
      }
    }
  }, [selectedItems]);

  const confirmAction = useCallback(async () => {
    if (!showConfirm) return;
    setIsProcessing(true);
    setShowConfirm(null);
    try {
      await showConfirm.onClick(selectedItems);
      onClearSelection();
    } finally {
      setIsProcessing(false);
    }
  }, [showConfirm, selectedItems, onClearSelection]);

  if (selectedItems.length === 0) return null;

  return (
    <>
      <div className="fixed bottom-6 left-1/2 -translate-x-1/2 z-30">
        <div className="flex items-center gap-4 px-4 py-3 bg-slate-900 dark:bg-slate-800 text-white rounded-xl shadow-2xl border border-slate-700">
          {/* Selection Count */}
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 rounded-full bg-blue-500 flex items-center justify-center text-sm font-bold">
              {selectedItems.length}
            </div>
            <span className="text-sm">
              {itemLabel} selected
            </span>
          </div>

          {/* Divider */}
          <div className="w-px h-8 bg-slate-600" />

          {/* Actions */}
          <div className="flex items-center gap-2">
            {actions.map((action) => (
              <button
                key={action.id}
                onClick={() => handleAction(action)}
                disabled={isProcessing}
                className={cn(
                  'flex items-center gap-2 px-3 py-1.5 rounded-lg text-sm font-medium transition',
                  action.variant === 'danger'
                    ? 'bg-red-600 hover:bg-red-700'
                    : 'bg-slate-700 hover:bg-slate-600'
                )}
              >
                {action.icon && <action.icon className="w-4 h-4" />}
                {action.label}
              </button>
            ))}
          </div>

          {/* Divider */}
          <div className="w-px h-8 bg-slate-600" />

          {/* Clear Selection */}
          <button
            onClick={onClearSelection}
            disabled={isProcessing}
            className="p-1.5 hover:bg-slate-700 rounded-lg transition"
            title="Clear selection"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      </div>

      {/* Confirm Dialog */}
      {showConfirm && (
        <ConfirmDialog
          isOpen={true}
          title={`${showConfirm.label} ${selectedItems.length} ${itemLabel}?`}
          message={`This action will ${showConfirm.label.toLowerCase()} the selected ${itemLabel}. This action cannot be undone.`}
          confirmText={showConfirm.label}
          cancelText="Cancel"
          onConfirm={confirmAction}
          onCancel={() => setShowConfirm(null)}
          variant="danger"
        />
      )}
    </>
  );
}

interface SelectAllCheckboxProps {
  allItems: { id: string }[];
  selectedItems: { id: string }[];
  onSelectAll: () => void;
  onClearSelection: () => void;
}

export function SelectAllCheckbox({
  allItems,
  selectedItems,
  onSelectAll,
  onClearSelection,
}: SelectAllCheckboxProps) {
  const isAllSelected = allItems.length > 0 && selectedItems.length === allItems.length;
  const isIndeterminate = selectedItems.length > 0 && selectedItems.length < allItems.length;

  const handleClick = () => {
    if (isAllSelected) {
      onClearSelection();
    } else {
      onSelectAll();
    }
  };

  return (
    <button
      onClick={handleClick}
      className={cn(
        'p-1 rounded hover:bg-slate-100 dark:hover:bg-slate-700 transition',
        isAllSelected && 'text-blue-600',
        isIndeterminate && 'text-blue-600',
        !isAllSelected && !isIndeterminate && 'text-slate-400'
      )}
      title={isAllSelected ? 'Deselect all' : 'Select all'}
    >
      {isAllSelected || isIndeterminate ? (
        <CheckSquare className="w-4 h-4" />
      ) : (
        <Square className="w-4 h-4" />
      )}
    </button>
  );
}

interface RowCheckboxProps<T> {
  item: T;
  isSelected: boolean;
  onToggle: (item: T) => void;
}

export function RowCheckbox<T extends { id: string }>({
  item,
  isSelected,
  onToggle,
}: RowCheckboxProps<T>) {
  return (
    <input
      type="checkbox"
      checked={isSelected}
      onChange={() => onToggle(item)}
      className="w-4 h-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500 cursor-pointer"
    />
  );
}

/**
 * Hook for managing bulk selection state
 */
export function useBulkSelection<T extends { id: string }>(items: T[]) {
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  const selectedItems = items.filter((item) => selectedIds.has(item.id));
  const isAllSelected = items.length > 0 && selectedIds.size === items.length;
  const isIndeterminate = selectedIds.size > 0 && selectedIds.size < items.length;

  const toggle = useCallback((item: T) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(item.id)) {
        next.delete(item.id);
      } else {
        next.add(item.id);
      }
      return next;
    });
  }, []);

  const selectAll = useCallback(() => {
    const ids = items.map((item) => item.id);
    setSelectedIds(new Set(ids));
  }, [items]);

  const clearSelection = useCallback(() => {
    setSelectedIds(new Set());
  }, []);

  const isSelected = useCallback((item: T) => {
    return selectedIds.has(item.id);
  }, [selectedIds]);

  return {
    selectedItems,
    selectedIds,
    isAllSelected,
    isIndeterminate,
    toggle,
    selectAll,
    clearSelection,
    isSelected,
  };
}
