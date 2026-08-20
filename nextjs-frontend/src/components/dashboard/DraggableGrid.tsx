'use client';

import {
  DndContext,
  DragOverlay,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  DragStartEvent,
  DragEndEvent,
} from '@dnd-kit/core';
import {
  SortableContext,
  sortableKeyboardCoordinates,
  rectSortingStrategy,
} from '@dnd-kit/sortable';
import { useState } from 'react';
import { useDashboardStore, Widget } from '@/stores/dashboardStore';
import { WidgetWrapper } from './WidgetWrapper';
import { StatsCard } from './widgets/StatsCard';
import { ChartWidget } from './widgets/ChartWidget';
import { RecentActivity } from './widgets/RecentActivity';
import { QuickActions } from './widgets/QuickActions';
import { Users, Package, ShoppingCart, Building2, DollarSign, Activity } from 'lucide-react';

interface StatsData {
  employees: string;
  inventory: string;
  orders: string;
  suppliers: string;
  projects: string;
  accounts: string;
}

interface DraggableGridProps {
  stats?: StatsData;
  isLoading?: boolean;
}

const widgetConfig: Record<string, {
  icon: React.ComponentType<{ className?: string }>;
  bgClass: string;
  href: string;
}> = {
  'stats-employees': { icon: Users, bgClass: 'bg-blue-500', href: '/dashboard/hrm' },
  'stats-inventory': { icon: Package, bgClass: 'bg-green-500', href: '/dashboard/inventory' },
  'stats-orders': { icon: ShoppingCart, bgClass: 'bg-orange-500', href: '/dashboard/purchasing' },
  'stats-suppliers': { icon: Building2, bgClass: 'bg-purple-500', href: '/dashboard/purchasing' },
  'stats-projects': { icon: Activity, bgClass: 'bg-cyan-500', href: '/dashboard/projects' },
  'stats-accounts': { icon: DollarSign, bgClass: 'bg-emerald-500', href: '/dashboard/accounting' },
};

const widgetContent: Record<string, (props: DraggableGridProps) => React.ReactNode> = {
  'stats-employees': ({ stats, isLoading }) => (
    <StatsCard
      label="Total Employees"
      value={stats?.employees ?? '-'}
      icon={Users}
      bgClass="bg-blue-500"
      href="/dashboard/hrm"
      isLoading={isLoading}
    />
  ),
  'stats-inventory': ({ stats, isLoading }) => (
    <StatsCard
      label="Inventory Items"
      value={stats?.inventory ?? '-'}
      icon={Package}
      bgClass="bg-green-500"
      href="/dashboard/inventory"
      isLoading={isLoading}
    />
  ),
  'stats-orders': ({ stats, isLoading }) => (
    <StatsCard
      label="Purchase Orders"
      value={stats?.orders ?? '-'}
      icon={ShoppingCart}
      bgClass="bg-orange-500"
      href="/dashboard/purchasing"
      isLoading={isLoading}
    />
  ),
  'stats-suppliers': ({ stats, isLoading }) => (
    <StatsCard
      label="Total Suppliers"
      value={stats?.suppliers ?? '-'}
      icon={Building2}
      bgClass="bg-purple-500"
      href="/dashboard/purchasing"
      isLoading={isLoading}
    />
  ),
  'stats-projects': ({ stats, isLoading }) => (
    <StatsCard
      label="Active Projects"
      value={stats?.projects ?? '-'}
      icon={Activity}
      bgClass="bg-cyan-500"
      href="/dashboard/projects"
      isLoading={isLoading}
    />
  ),
  'stats-accounts': ({ stats, isLoading }) => (
    <StatsCard
      label="Chart of Accounts"
      value={stats?.accounts ?? '-'}
      icon={DollarSign}
      bgClass="bg-emerald-500"
      href="/dashboard/accounting"
      isLoading={isLoading}
    />
  ),
  'chart-overview': ({ isLoading }) => <ChartWidget isLoading={isLoading} />,
  'quick-actions': ({ isLoading }) => <QuickActions isLoading={isLoading} />,
  'recent-activity': ({ isLoading }) => <RecentActivity isLoading={isLoading} />,
};

export function DraggableGrid({ stats, isLoading }: DraggableGridProps) {
  const { widgets, reorderWidgets } = useDashboardStore();
  const [activeId, setActiveId] = useState<string | null>(null);

  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 8,
      },
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const handleDragStart = (event: DragStartEvent) => {
    setActiveId(event.active.id as string);
  };

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    setActiveId(null);

    if (over && active.id !== over.id) {
      reorderWidgets(active.id as string, over.id as string);
    }
  };

  const visibleWidgets = widgets
    .filter((w) => w.visible)
    .sort((a, b) => a.order - b.order);

  const activeWidget = activeId ? widgets.find((w) => w.id === activeId) : null;

  return (
    <DndContext
      sensors={sensors}
      collisionDetection={closestCenter}
      onDragStart={handleDragStart}
      onDragEnd={handleDragEnd}
    >
      <SortableContext items={visibleWidgets.map((w) => w.id)} strategy={rectSortingStrategy}>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {visibleWidgets.map((widget) => {
            const renderContent = widgetContent[widget.id];
            if (!renderContent) return null;

            return (
              <WidgetWrapper key={widget.id} widget={widget}>
                {renderContent({ stats, isLoading })}
              </WidgetWrapper>
            );
          })}
        </div>
      </SortableContext>

      <DragOverlay>
        {activeWidget && (
          <div className="bg-white dark:bg-slate-800 rounded-xl border border-blue-500 shadow-2xl p-4 opacity-90">
            <p className="font-semibold text-slate-900 dark:text-white">{activeWidget.title}</p>
          </div>
        )}
      </DragOverlay>
    </DndContext>
  );
}
