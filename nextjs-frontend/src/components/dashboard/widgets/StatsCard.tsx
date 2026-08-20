'use client';

import { ArrowUpRight } from 'lucide-react';
import Link from 'next/link';

interface StatsCardProps {
  label: string;
  value: string | number;
  icon: React.ComponentType<{ className?: string }>;
  bgClass: string;
  href: string;
  isLoading?: boolean;
}

export function StatsCard({ label, value, icon: Icon, bgClass, href, isLoading }: StatsCardProps) {
  return (
    <Link
      href={href}
      className="flex items-center gap-3 group"
    >
      <div className={`p-2.5 rounded-lg ${bgClass}`}>
        <Icon className="w-5 h-5 text-white" />
      </div>
      <div className="flex-1 min-w-0">
        {isLoading ? (
          <div className="animate-pulse">
            <div className="h-7 w-12 bg-slate-200 dark:bg-slate-700 rounded" />
            <div className="h-4 w-20 bg-slate-200 dark:bg-slate-700 rounded mt-1" />
          </div>
        ) : (
          <>
            <p className="text-2xl font-bold text-slate-900 dark:text-white truncate">
              {value}
            </p>
            <p className="text-sm text-slate-500 truncate">{label}</p>
          </>
        )}
      </div>
      <ArrowUpRight className="w-4 h-4 text-slate-400 group-hover:text-blue-500 transition flex-shrink-0" />
    </Link>
  );
}
