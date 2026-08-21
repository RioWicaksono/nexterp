'use client';

import { useState } from 'react';
import { Download } from 'lucide-react';
import { ExportDialog, type ExportColumn } from './ExportDialog';
import { downloadCSV, downloadExcel } from '@/lib/export';
import type { ExportColumn as ExportColumnType } from '@/lib/export';

interface ExportButtonProps {
  /** Data to export */
  data: Record<string, unknown>[];
  /** Column definitions */
  columns: ExportColumn[];
  /** Export filename (without extension) */
  filename?: string;
  /** Additional className */
  className?: string;
}

export function ExportButton({ data, columns, filename = 'export', className }: ExportButtonProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [selectedColumns, setSelectedColumns] = useState<ExportColumn[]>(
    columns.map((col) => ({ ...col, selected: col.selected ?? true }))
  );

  const handleExport = (format: 'csv' | 'excel') => {
    // Convert to the format expected by export utility
    const exportColumns: ExportColumnType<Record<string, unknown>>[] = selectedColumns
      .filter((col) => col.selected)
      .map((col) => ({
        key: col.key as keyof Record<string, unknown> | string,
        header: col.label,
      }));

    if (format === 'csv') {
      downloadCSV(data, exportColumns, { filename });
    } else {
      downloadExcel(data, exportColumns, { filename });
    }
  };

  return (
    <>
      <button
        onClick={() => setIsOpen(true)}
        className={className}
        title="Export data"
      >
        <Download className="w-4 h-4" />
        Export
      </button>

      <ExportDialog
        isOpen={isOpen}
        onClose={() => setIsOpen(false)}
        columns={selectedColumns}
        onColumnsChange={setSelectedColumns}
        onExport={handleExport}
        filename={filename}
        recordCount={data.length}
      />
    </>
  );
}
