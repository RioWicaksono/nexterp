'use client';

import { ReactNode } from 'react';
import { ChevronUp, ChevronDown, ChevronsUpDown } from 'lucide-react';
import { clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

export interface Column<T = Record<string, unknown>> {
  key: string;
  label: string;
  sortable?: boolean;
  render?: (value: unknown, row: T) => ReactNode;
  className?: string;
}

interface DataTableProps<T extends Record<string, unknown> = Record<string, unknown>> {
  columns: Column<T>[];
  data: T[];
  loading?: boolean;
  emptyText?: string;
  onRowClick?: (row: T) => void;
  hoverHighlight?: boolean;
  skeletonRows?: number;
  skeletonCols?: number;
  skeletonHeight?: string;
  // Pagination
  page?: number;
  pageSize?: number;
  totalCount?: number;
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
}

export function DataTable<T extends Record<string, unknown>>({
  columns,
  data,
  loading = false,
  emptyText = 'No data available',
  onRowClick,
  hoverHighlight = true,
  skeletonRows = 5,
  skeletonCols,
  skeletonHeight = 'h-10',
  page = 1,
  pageSize = 10,
  totalCount,
  onPageChange,
  onPageSizeChange,
}: DataTableProps<T>) {
  const totalPages = totalCount ? Math.ceil(totalCount / pageSize) : 1;
  const pageSizeOptions = [10, 25, 50];
  const effectiveCols = skeletonCols ?? columns.length;

  const renderSkeletonRows = () => {
    return Array.from({ length: skeletonRows }).map((_, rowIndex) => (
      <tr key={`skeleton-${rowIndex}`} className="border-b border-slate-200 dark:border-slate-700">
        {columns.map((col, colIndex) => (
          <td
            key={`skeleton-${rowIndex}-${colIndex}`}
            className={twMerge(
              clsx(
                'px-4 py-3',
                col.className
              )
            )}
          >
            <div
              className={twMerge(
                clsx(
                  'bg-slate-200 dark:bg-slate-700 rounded animate-pulse',
                  skeletonHeight
                )
              )}
            />
          </td>
        ))}
      </tr>
    ));
  };

  const renderDataRows = () => {
    if (data.length === 0) {
      return (
        <tr>
          <td
            colSpan={columns.length}
            className="px-4 py-12 text-center text-slate-500 dark:text-slate-400"
          >
            {emptyText}
          </td>
        </tr>
      );
    }

    return data.map((row, rowIndex) => (
      <tr
        key={`row-${rowIndex}`}
        className={twMerge(
          clsx(
            'border-b border-slate-200 dark:border-slate-700',
            'transition-colors',
            hoverHighlight && 'hover:bg-slate-50 dark:hover:bg-slate-800/50',
            onRowClick && 'cursor-pointer'
          )
        )}
        onClick={() => onRowClick?.(row)}
      >
        {columns.map((col) => {
          const value = row[col.key as keyof T];
          return (
            <td
              key={col.key}
              className={twMerge(
                clsx(
                  'px-4 py-3 text-sm text-slate-700 dark:text-slate-300',
                  col.className
                )
              )}
            >
              {col.render ? col.render(value, row) : String(value ?? '-')}
            </td>
          );
        })}
      </tr>
    ));
  };

  return (
    <div className="flex flex-col">
      <div className="overflow-x-auto -mx-4 -my-2">
        <div className="inline-block min-w-full py-2 align-middle px-4">
          <div className="overflow-hidden border border-slate-200 dark:border-slate-700 rounded-lg">
            <table className="min-w-full divide-y divide-slate-200 dark:divide-slate-700">
              <thead className="bg-slate-50 dark:bg-slate-800">
                <tr>
                  {columns.map((col) => (
                    <th
                      key={col.key}
                      scope="col"
                      className={twMerge(
                        clsx(
                          'px-4 py-3 text-left text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider',
                          col.sortable && 'cursor-pointer select-none hover:text-slate-700 dark:hover:text-slate-200',
                          col.className
                        )
                      )}
                    >
                      <div className="flex items-center gap-1">
                        {col.label}
                        {col.sortable && (
                          <button className="p-0.5 hover:bg-slate-200 dark:hover:bg-slate-700 rounded">
                            <ChevronsUpDown className="w-3.5 h-3.5" />
                          </button>
                        )}
                      </div>
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="bg-white dark:bg-slate-900 divide-y divide-slate-200 dark:divide-slate-700">
                {loading ? renderSkeletonRows() : renderDataRows()}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* Pagination */}
      {onPageChange && onPageSizeChange && (
        <div className="flex items-center justify-between px-4 py-3 border-t border-slate-200 dark:border-slate-700">
          <div className="flex items-center gap-2 text-sm text-slate-500 dark:text-slate-400">
            <span>Rows per page:</span>
            <select
              value={pageSize}
              onChange={(e) => onPageSizeChange(Number(e.target.value))}
              className={twMerge(
                clsx(
                  'px-2 py-1 text-sm rounded border border-slate-300 dark:border-slate-600',
                  'bg-white dark:bg-slate-800',
                  'focus:outline-none focus:ring-2 focus:ring-slate-500'
                )
              )}
            >
              {pageSizeOptions.map((size) => (
                <option key={size} value={size}>
                  {size}
                </option>
              ))}
            </select>
          </div>

          <div className="flex items-center gap-2 text-sm text-slate-500 dark:text-slate-400">
            <span>
              {totalCount
                ? `${(page - 1) * pageSize + 1}-${Math.min(page * pageSize, totalCount)} of ${totalCount}`
                : `${data.length} items`}
            </span>

            <div className="flex items-center gap-1">
              <button
                onClick={() => onPageChange(page - 1)}
                disabled={page <= 1}
                className={twMerge(
                  clsx(
                    'p-1.5 rounded hover:bg-slate-100 dark:hover:bg-slate-800',
                    'disabled:opacity-50 disabled:cursor-not-allowed',
                    'transition-colors'
                  )
                )}
                aria-label="Previous page"
              >
                <ChevronUp className="w-4 h-4 rotate-90" />
              </button>

              {Array.from({ length: Math.min(5, totalPages) }).map((_, i) => {
                let pageNum: number;
                if (totalPages <= 5) {
                  pageNum = i + 1;
                } else if (page <= 3) {
                  pageNum = i + 1;
                } else if (page >= totalPages - 2) {
                  pageNum = totalPages - 4 + i;
                } else {
                  pageNum = page - 2 + i;
                }

                return (
                  <button
                    key={i}
                    onClick={() => onPageChange(pageNum)}
                    className={twMerge(
                      clsx(
                        'w-8 h-8 text-sm rounded',
                        page === pageNum
                          ? 'bg-slate-800 dark:bg-slate-200 text-white dark:text-slate-900 font-medium'
                          : 'hover:bg-slate-100 dark:hover:bg-slate-800',
                        'transition-colors'
                      )
                    )}
                  >
                    {pageNum}
                  </button>
                );
              })}

              <button
                onClick={() => onPageChange(page + 1)}
                disabled={page >= totalPages}
                className={twMerge(
                  clsx(
                    'p-1.5 rounded hover:bg-slate-100 dark:hover:bg-slate-800',
                    'disabled:opacity-50 disabled:cursor-not-allowed',
                    'transition-colors'
                  )
                )}
                aria-label="Next page"
              >
                <ChevronDown className="w-4 h-4 rotate-90" />
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
