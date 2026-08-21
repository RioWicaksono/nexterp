'use client';

import { type ReactNode, Suspense } from 'react';
import { Skeleton } from './SkeletonLoader';

export function LazyComponent({ children }: { children: ReactNode }) {
  return <Suspense fallback={<Skeleton className="h-10 w-full" />}>{children}</Suspense>;
}
