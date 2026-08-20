import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { arrayMove } from '@dnd-kit/sortable';

export interface Widget {
  id: string;
  type: 'stats' | 'chart' | 'activity' | 'quick-actions';
  title: string;
  visible: boolean;
  size: 'small' | 'medium' | 'large';
  order: number;
}

interface DashboardState {
  widgets: Widget[];
  isLayoutLocked: boolean;
  setWidgets: (widgets: Widget[]) => void;
  reorderWidgets: (activeId: string, overId: string) => void;
  toggleWidget: (id: string) => void;
  updateWidgetSize: (id: string, size: Widget['size']) => void;
  setLayoutLocked: (locked: boolean) => void;
  resetToDefault: () => void;
}

const defaultWidgets: Widget[] = [
  { id: 'stats-employees', type: 'stats', title: 'Total Employees', visible: true, size: 'small', order: 0 },
  { id: 'stats-inventory', type: 'stats', title: 'Inventory Items', visible: true, size: 'small', order: 1 },
  { id: 'stats-orders', type: 'stats', title: 'Purchase Orders', visible: true, size: 'small', order: 2 },
  { id: 'stats-suppliers', type: 'stats', title: 'Total Suppliers', visible: true, size: 'small', order: 3 },
  { id: 'stats-projects', type: 'stats', title: 'Active Projects', visible: true, size: 'small', order: 4 },
  { id: 'stats-accounts', type: 'stats', title: 'Chart of Accounts', visible: true, size: 'small', order: 5 },
  { id: 'chart-overview', type: 'chart', title: 'Overview Chart', visible: true, size: 'medium', order: 6 },
  { id: 'quick-actions', type: 'quick-actions', title: 'Quick Actions', visible: true, size: 'medium', order: 7 },
  { id: 'recent-activity', type: 'activity', title: 'Recent Activity', visible: true, size: 'large', order: 8 },
];

export const useDashboardStore = create<DashboardState>()(
  persist(
    (set, get) => ({
      widgets: defaultWidgets,
      isLayoutLocked: false,

      setWidgets: (widgets) => set({ widgets }),

      reorderWidgets: (activeId, overId) => {
        const { widgets } = get();
        const oldIndex = widgets.findIndex((w) => w.id === activeId);
        const newIndex = widgets.findIndex((w) => w.id === overId);

        if (oldIndex === -1 || newIndex === -1) return;

        const reordered = arrayMove(
          widgets.map((w) => ({ ...w })),
          oldIndex,
          newIndex
        ).map((w, i) => ({ ...w, order: i }));

        set({ widgets: reordered });
      },

      toggleWidget: (id) => {
        const { widgets } = get();
        set({
          widgets: widgets.map((w) =>
            w.id === id ? { ...w, visible: !w.visible } : w
          ),
        });
      },

      updateWidgetSize: (id, size) => {
        const { widgets } = get();
        set({
          widgets: widgets.map((w) => (w.id === id ? { ...w, size } : w)),
        });
      },

      setLayoutLocked: (locked) => set({ isLayoutLocked: locked }),

      resetToDefault: () => set({ widgets: defaultWidgets }),
    }),
    {
      name: 'nexterp-dashboard',
    }
  )
);
