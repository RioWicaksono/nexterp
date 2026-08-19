'use client';

import { clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

interface SkeletonLoaderProps {
  rows?: number;
  height?: string;
  className?: string;
}

export function SkeletonLoader({
  rows = 5,
  height = 'h-12',
  className,
}: SkeletonLoaderProps) {
  return (
    <div className={twMerge(clsx('space-y-3', className))} role="status">
      {Array.from({ length: rows }).map((_, index) => (
        <div
          key={index}
          className={twMerge(
            clsx(
              'bg-slate-200 dark:bg-slate-700 rounded animate-pulse',
              height
            )
          )}
        />
      ))}
      <span className="sr-only">Loading...</span>
    </div>
  );
}
