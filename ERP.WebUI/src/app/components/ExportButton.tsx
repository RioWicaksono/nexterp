"use client";

import { useState } from "react";
import { Download, FileSpreadsheet, FileText, Loader2 } from "lucide-react";

interface ExportButtonProps {
  onExportCSV: () => void;
  onExportPDF?: () => void;
  filename?: string;
  isLoading?: boolean;
  disabled?: boolean;
}

export function ExportButton({
  onExportCSV,
  onExportPDF,
  filename = "export",
  isLoading = false,
  disabled = false,
}: ExportButtonProps) {
  const [showMenu, setShowMenu] = useState(false);

  return (
    <div className="relative">
      <button
        onClick={() => setShowMenu(!showMenu)}
        disabled={disabled || isLoading}
        className="flex items-center gap-2 px-4 py-2 rounded-xl bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white transition-colors"
      >
        {isLoading ? (
          <Loader2 className="w-4 h-4 animate-spin" />
        ) : (
          <Download className="w-4 h-4" />
        )}
        <span className="text-sm font-medium">Export</span>
        <svg
          className={`w-4 h-4 transition-transform ${showMenu ? "rotate-180" : ""}`}
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
        </svg>
      </button>

      {showMenu && (
        <>
          <div
            className="fixed inset-0 z-10"
            onClick={() => setShowMenu(false)}
          />
          <div className="absolute right-0 mt-2 w-48 bg-white dark:bg-slate-800 rounded-xl shadow-lg border border-slate-200 dark:border-slate-700 py-2 z-20 animate-scale-in">
            <button
              onClick={() => {
                onExportCSV();
                setShowMenu(false);
              }}
              className="w-full flex items-center gap-3 px-4 py-2.5 hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
            >
              <FileSpreadsheet className="w-4 h-4 text-emerald-600" />
              <span className="text-sm text-slate-700 dark:text-slate-200">
                Export as CSV
              </span>
            </button>
            {onExportPDF && (
              <button
                onClick={() => {
                  onExportPDF();
                  setShowMenu(false);
                }}
                className="w-full flex items-center gap-3 px-4 py-2.5 hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
              >
                <FileText className="w-4 h-4 text-red-600" />
                <span className="text-sm text-slate-700 dark:text-slate-200">
                  Export as PDF
                </span>
              </button>
            )}
          </div>
        </>
      )}
    </div>
  );
}

// Quick export button for tables
interface QuickExportProps {
  tableId: string;
  filename?: string;
}

export function QuickExportCSV({ tableId, filename }: QuickExportProps) {
  const [isLoading, setIsLoading] = useState(false);

  const handleExport = async () => {
    setIsLoading(true);
    try {
      const { exportTableToCSV } = await import("@/lib/export");
      exportTableToCSV(tableId, filename);
    } catch (error) {
      console.error("Export failed:", error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <button
      onClick={handleExport}
      disabled={isLoading}
      className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-slate-100 dark:bg-slate-700 hover:bg-slate-200 dark:hover:bg-slate-600 text-slate-600 dark:text-slate-300 text-sm transition-colors"
    >
      {isLoading ? (
        <Loader2 className="w-4 h-4 animate-spin" />
      ) : (
        <FileSpreadsheet className="w-4 h-4" />
      )}
      CSV
    </button>
  );
}

// Export menu for data tables
interface DataExportMenuProps {
  onExport: (format: "csv" | "pdf") => void;
  filename?: string;
}

export function DataExportMenu({ onExport, filename }: DataExportMenuProps) {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div className="relative inline-block">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="flex items-center gap-2 px-4 py-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
      >
        <Download className="w-4 h-4 text-slate-500" />
        <span className="text-sm font-medium text-slate-600 dark:text-slate-300">
          Export Data
        </span>
      </button>

      {isOpen && (
        <>
          <div className="fixed inset-0 z-10" onClick={() => setIsOpen(false)} />
          <div className="absolute right-0 mt-2 w-56 bg-white dark:bg-slate-800 rounded-xl shadow-lg border border-slate-200 dark:border-slate-700 py-2 z-20">
            <div className="px-4 py-2 border-b border-slate-100 dark:border-slate-700">
              <p className="text-xs font-medium text-slate-400 uppercase">Export Format</p>
            </div>
            <button
              onClick={() => {
                onExport("csv");
                setIsOpen(false);
              }}
              className="w-full flex items-center gap-3 px-4 py-3 hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
            >
              <div className="w-8 h-8 rounded-lg bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center">
                <FileSpreadsheet className="w-4 h-4 text-emerald-600" />
              </div>
              <div className="text-left">
                <p className="text-sm font-medium text-slate-700 dark:text-slate-200">
                  CSV (Excel Compatible)
                </p>
                <p className="text-xs text-slate-400">Best for data analysis</p>
              </div>
            </button>
            <button
              onClick={() => {
                onExport("pdf");
                setIsOpen(false);
              }}
              className="w-full flex items-center gap-3 px-4 py-3 hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
            >
              <div className="w-8 h-8 rounded-lg bg-red-100 dark:bg-red-900/30 flex items-center justify-center">
                <FileText className="w-4 h-4 text-red-600" />
              </div>
              <div className="text-left">
                <p className="text-sm font-medium text-slate-700 dark:text-slate-200">
                  PDF Document
                </p>
                <p className="text-xs text-slate-400">Best for printing</p>
              </div>
            </button>
          </div>
        </>
      )}
    </div>
  );
}
