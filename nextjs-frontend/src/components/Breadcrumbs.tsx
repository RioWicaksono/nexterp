'use client';

import Link from 'next/link';
import { ChevronRight, Home } from 'lucide-react';
import { clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

export interface BreadcrumbItem {
  label: string;
  href?: string;
}

interface BreadcrumbsProps {
  items: BreadcrumbItem[];
}

export function Breadcrumbs({ items }: BreadcrumbsProps) {
  if (!items || items.length === 0) return null;

  return (
    <nav aria-label="Breadcrumb" className="mb-4">
      <ol className="flex items-center gap-1 text-sm text-slate-500 dark:text-slate-400">
        {items.map((item, index) => {
          const isLast = index === items.length - 1;
          const isFirst = index === 0;

          return (
            <li key={index} className="flex items-center gap-1">
              {index > 0 && (
                <ChevronRight className="w-4 h-4 text-slate-400" />
              )}
              {isFirst && (
                <Link
                  href={item.href ?? '/'}
                  className={twMerge(
                    clsx(
                      'flex items-center gap-1.5 hover:text-slate-700 dark:hover:text-slate-200 transition-colors',
                      isLast && 'pointer-events-none text-slate-700 dark:text-slate-200 font-medium'
                    )
                  )}
                  aria-current={isLast ? 'page' : undefined}
                >
                  {isFirst && <Home className="w-4 h-4" />}
                  <span>{item.label}</span>
                </Link>
              )}
              {isLast && !isFirst && (
                <span
                  className="text-slate-700 dark:text-slate-200 font-medium"
                  aria-current="page"
                >
                  {item.label}
                </span>
              )}
              {!isLast && !isFirst && (
                <Link
                  href={item.href ?? '#'}
                  className="hover:text-slate-700 dark:hover:text-slate-200 transition-colors"
                >
                  <span>{item.label}</span>
                </Link>
              )}
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
