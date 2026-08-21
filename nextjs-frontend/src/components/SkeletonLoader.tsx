'use client';

import { cn } from '@/lib/utils';

/**
 * Consistent skeleton loader component
 */
interface SkeletonProps {
  className?: string;
  variant?: 'text' | 'circular' | 'rectangular';
  width?: string | number;
  height?: string | number;
  animation?: 'pulse' | 'wave' | 'none';
}

export function Skeleton({
  className,
  variant = 'rectangular',
  width,
  height,
  animation = 'pulse',
}: SkeletonProps) {
  const variantClasses = {
    text: 'rounded',
    circular: 'rounded-full',
    rectangular: 'rounded-lg',
  };

  const animationClasses = {
    pulse: 'animate-pulse',
    wave: 'animate-shimmer',
    none: '',
  };

  return (
    <div
      className={cn(
        'bg-slate-200 dark:bg-slate-700',
        variantClasses[variant],
        animationClasses[animation],
        className
      )}
      style={{
        width: typeof width === 'number' ? `${width}px` : width,
        height: typeof height === 'number' ? `${height}px` : height,
      }}
    />
  );
}

/**
 * Legacy skeleton loader for backward compatibility
 * Use Skeleton component directly for new code
 */
interface SkeletonLoaderProps {
  rows?: number;
  height?: string;
}

export function SkeletonLoader({ rows = 5, height = 'h-12' }: SkeletonLoaderProps) {
  return (
    <div className="space-y-3">
      {Array.from({ length: rows }).map((_, i) => (
        <div key={i} className={cn('bg-slate-200 dark:bg-slate-700 rounded animate-pulse', height)} />
      ))}
    </div>
  );
}

/**
 * Table row skeleton loader
 */
interface TableRowSkeletonProps {
  columns?: number;
  rows?: number;
  columnWidths?: (string | number)[];
}

export function TableRowSkeleton({ columns = 5, rows = 5, columnWidths }: TableRowSkeletonProps) {
  const defaultWidths = ['20%', '25%', '20%', '15%', '20%'];

  return (
    <>
      {Array.from({ length: rows }).map((_, rowIndex) => (
        <tr key={rowIndex} className="border-b border-slate-200 dark:border-slate-700">
          {Array.from({ length: columns }).map((_, colIndex) => (
            <td key={colIndex} className="px-4 py-3">
              <Skeleton
                height={16}
                width={columnWidths?.[colIndex] || defaultWidths[colIndex % defaultWidths.length]}
              />
            </td>
          ))}
        </tr>
      ))}
    </>
  );
}

/**
 * Card skeleton loader
 */
interface CardSkeletonProps {
  count?: number;
  columns?: number;
}

export function CardSkeleton({ count = 3, columns = 3 }: CardSkeletonProps) {
  return (
    <div className={`grid grid-cols-1 md:grid-cols-${columns} gap-4`}>
      {Array.from({ length: count }).map((_, i) => (
        <div
          key={i}
          className="bg-white dark:bg-slate-800 rounded-xl p-5 border border-slate-200 dark:border-slate-700"
        >
          <div className="flex items-center gap-3 mb-3">
            <Skeleton variant="circular" width={40} height={40} />
            <div className="space-y-2">
              <Skeleton height={12} width={80} />
              <Skeleton height={10} width={60} />
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}

/**
 * Form skeleton loader
 */
interface FormSkeletonProps {
  fields?: number;
}

export function FormSkeleton({ fields = 4 }: FormSkeletonProps) {
  return (
    <div className="space-y-4">
      {Array.from({ length: fields }).map((_, i) => (
        <div key={i} className="space-y-2">
          <Skeleton height={14} width={100} />
          <Skeleton height={40} width="100%" />
        </div>
      ))}
    </div>
  );
}

/**
 * Detail page skeleton loader
 */
export function DetailPageSkeleton() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Skeleton variant="circular" width={64} height={64} />
        <div className="space-y-2">
          <Skeleton height={24} width={200} />
          <Skeleton height={16} width={150} />
        </div>
      </div>

      {/* Content */}
      <div className="bg-white dark:bg-slate-800 rounded-xl p-6 border border-slate-200 dark:border-slate-700 space-y-4">
        <Skeleton height={20} width={120} />
        <div className="grid grid-cols-2 gap-4">
          <div className="space-y-2">
            <Skeleton height={14} width={80} />
            <Skeleton height={18} width="100%" />
          </div>
          <div className="space-y-2">
            <Skeleton height={14} width={80} />
            <Skeleton height={18} width="100%" />
          </div>
        </div>
      </div>
    </div>
  );
}
