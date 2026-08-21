'use client';

import { useState } from 'react';
import { Download, X, Check, FileSpreadsheet, FileText } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface ExportColumn {
  key: string;
  label: string;
  selected?: boolean;
}

interface ExportDialogProps {
  isOpen: boolean;
  onClose: () => void;
  columns: ExportColumn[];
  onColumnsChange: (columns: ExportColumn[]) => void;
  onExport: (format: 'csv' | 'excel') => void;
  filename?: string;
  recordCount?: number;
}

export function ExportDialog({
  isOpen,
  onClose,
  columns,
  onColumnsChange,
  onExport,
  filename = 'export',
  recordCount = 0,
}: ExportDialogProps) {
  const [format, setFormat] = useState<'csv' | 'excel'>('csv');

  if (!isOpen) return null;

  const toggleColumn = (key: string) => {
    const updated = columns.map((col) =>
      col.key === key ? { ...col, selected: !col.selected } : col
    );
    onColumnsChange(updated);
  };

  const selectAll = () => {
    onColumnsChange(columns.map((col) => ({ ...col, selected: true })));
  };

  const deselectAll = () => {
    onColumnsChange(columns.map((col) => ({ ...col, selected: false })));
  };

  const selectedCount = columns.filter((c) => c.selected).length;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="bg-white dark:bg-slate-800 rounded-xl shadow-xl w-full max-w-lg mx-4">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-slate-200 dark:border-slate-700">
          <div className="flex items-center gap-2">
            <Download className="w-5 h-5 text-slate-600 dark:text-slate-400" />
            <h2 className="text-lg font-semibold text-slate-900 dark:text-white">Export Data</h2>
          </div>
          <button
            onClick={onClose}
            className="p-1 hover:bg-slate-100 dark:hover:bg-slate-700 rounded transition"
            aria-label="Close dialog"
          >
            <X className="w-5 h-5 text-slate-500" />
          </button>
        </div>

        {/* Content */}
        <div className="p-4 space-y-4">
          {/* Record count */}
          <div className="text-sm text-slate-500 dark:text-slate-400">
            {recordCount > 0 ? (
              <span>Exporting <strong>{recordCount}</strong> records</span>
            ) : (
              <span className="text-orange-500">No records to export</span>
            )}
          </div>

          {/* Format selection */}
          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
              Export Format
            </label>
            <div className="flex gap-3">
              <button
                onClick={() => setFormat('csv')}
                className={cn(
                  'flex-1 flex items-center justify-center gap-2 px-4 py-3 rounded-lg border transition',
                  format === 'csv'
                    ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400'
                    : 'border-slate-300 dark:border-slate-600 hover:bg-slate-50 dark:hover:bg-slate-700'
                )}
              >
                <FileText className="w-4 h-4" />
                CSV
              </button>
              <button
                onClick={() => setFormat('excel')}
                className={cn(
                  'flex-1 flex items-center justify-center gap-2 px-4 py-3 rounded-lg border transition',
                  format === 'excel'
                    ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400'
                    : 'border-slate-300 dark:border-slate-600 hover:bg-slate-50 dark:hover:bg-slate-700'
                )}
              >
                <FileSpreadsheet className="w-4 h-4" />
                Excel
              </button>
            </div>
          </div>

          {/* Column selection */}
          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="text-sm font-medium text-slate-700 dark:text-slate-300">
                Select Columns ({selectedCount}/{columns.length})
              </label>
              <div className="flex gap-2 text-xs">
                <button onClick={selectAll} className="text-blue-600 hover:underline">
                  Select All
                </button>
                <span className="text-slate-400">|</span>
                <button onClick={deselectAll} className="text-slate-500 hover:underline">
                  Deselect All
                </button>
              </div>
            </div>
            <div className="max-h-48 overflow-y-auto border border-slate-200 dark:border-slate-700 rounded-lg p-2 space-y-1">
              {columns.map((col) => (
                <label
                  key={col.key}
                  className="flex items-center gap-2 p-2 hover:bg-slate-50 dark:hover:bg-slate-700 rounded cursor-pointer"
                >
                  <input
                    type="checkbox"
                    checked={col.selected}
                    onChange={() => toggleColumn(col.key)}
                    className="rounded border-slate-300 text-blue-600 focus:ring-blue-500"
                  />
                  <span className="text-sm text-slate-700 dark:text-slate-300">{col.label}</span>
                </label>
              ))}
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="flex items-center justify-end gap-3 p-4 border-t border-slate-200 dark:border-slate-700">
          <button
            onClick={onClose}
            className="px-4 py-2 text-sm font-medium text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-lg transition"
          >
            Cancel
          </button>
          <button
            onClick={() => {
              onExport(format);
              onClose();
            }}
            disabled={selectedCount === 0 || recordCount === 0}
            className={cn(
              'flex items-center gap-2 px-4 py-2 text-sm font-medium rounded-lg transition',
              selectedCount === 0 || recordCount === 0
                ? 'bg-slate-300 text-slate-500 cursor-not-allowed'
                : 'bg-blue-600 text-white hover:bg-blue-700'
            )}
          >
            <Download className="w-4 h-4" />
            Export {selectedCount} Column{selectedCount !== 1 ? 's' : ''}
          </button>
        </div>
      </div>
    </div>
  );
}
