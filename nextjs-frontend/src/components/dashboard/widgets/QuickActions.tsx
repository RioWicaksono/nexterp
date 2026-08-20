'use client';

import Link from 'next/link';
import { Users, Package, ShoppingCart, DollarSign } from 'lucide-react';

interface QuickActionsProps {
  isLoading?: boolean;
}

const defaultActions = [
  { label: 'Add Employee', href: '/dashboard/hrm', icon: Users, bgClass: 'bg-blue-50 dark:bg-blue-900/20', textClass: 'text-blue-600 dark:text-blue-400', hoverClass: 'hover:bg-blue-100 dark:hover:bg-blue-900/30' },
  { label: 'Add Item', href: '/dashboard/inventory', icon: Package, bgClass: 'bg-green-50 dark:bg-green-900/20', textClass: 'text-green-600 dark:text-green-400', hoverClass: 'hover:bg-green-100 dark:hover:bg-green-900/30' },
  { label: 'New PO', href: '/dashboard/purchasing', icon: ShoppingCart, bgClass: 'bg-orange-50 dark:bg-orange-900/20', textClass: 'text-orange-600 dark:text-orange-400', hoverClass: 'hover:bg-orange-100 dark:hover:bg-orange-900/30' },
  { label: 'Create Journal', href: '/dashboard/accounting', icon: DollarSign, bgClass: 'bg-purple-50 dark:bg-purple-900/20', textClass: 'text-purple-600 dark:text-purple-400', hoverClass: 'hover:bg-purple-100 dark:hover:bg-purple-900/30' },
];

export function QuickActions({ isLoading }: QuickActionsProps) {
  if (isLoading) {
    return (
      <div className="grid grid-cols-2 gap-3 animate-pulse">
        {Array.from({ length: 4 }).map((_, i) => (
          <div key={i} className="h-12 bg-slate-200 dark:bg-slate-700 rounded-lg" />
        ))}
      </div>
    );
  }

  return (
    <div className="grid grid-cols-2 gap-3">
      {defaultActions.map((action) => (
        <Link
          key={action.label}
          href={action.href}
          className={`flex items-center gap-2 px-4 py-3 ${action.bgClass} ${action.textClass} ${action.hoverClass} rounded-lg transition text-sm font-medium`}
        >
          <action.icon className="w-5 h-5" />
          {action.label}
        </Link>
      ))}
    </div>
  );
}
