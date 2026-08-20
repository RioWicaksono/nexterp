'use client';

import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { GripVertical, Maximize2, Minimize2, EyeOff, Lock } from 'lucide-react';
import { Widget } from '@/stores/dashboardStore';
import { useDashboardStore } from '@/stores/dashboardStore';
import { cn } from '@/lib/utils';

interface WidgetWrapperProps {
  widget: Widget;
  children: React.ReactNode;
  className?: string;
}

export function WidgetWrapper({ widget, children, className }: WidgetWrapperProps) {
  const { isLayoutLocked, toggleWidget, updateWidgetSize } = useDashboardStore();

  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({
    id: widget.id,
    disabled: isLayoutLocked,
  });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  const sizeClasses = {
    small: 'col-span-1',
    medium: 'col-span-1 lg:col-span-1',
    large: 'col-span-1 lg:col-span-2',
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={cn(
        'bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700',
        'transition-shadow duration-200',
        isDragging && 'opacity-50 shadow-2xl ring-2 ring-blue-500 z-50',
        sizeClasses[widget.size],
        className
      )}
    >
      <div className="flex items-center justify-between px-4 py-3 border-b border-slate-200 dark:border-slate-700">
        <h3 className="font-semibold text-sm text-slate-900 dark:text-white truncate">
          {widget.title}
        </h3>

        <div className="flex items-center gap-1">
          {!isLayoutLocked && (
            <>
              <button
                onClick={() => toggleWidget(widget.id)}
                className="p-1.5 rounded hover:bg-slate-100 dark:hover:bg-slate-700 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 transition"
                aria-label={widget.visible ? 'Hide widget' : 'Show widget'}
                title={widget.visible ? 'Hide' : 'Show'}
              >
                {widget.visible ? (
                  <Maximize2 className="w-3.5 h-3.5" />
                ) : (
                  <EyeOff className="w-3.5 h-3.5" />
                )}
              </button>

              <button
                onClick={() => {
                  const sizes: Widget['size'][] = ['small', 'medium', 'large'];
                  const currentIndex = sizes.indexOf(widget.size);
                  const nextSize = sizes[(currentIndex + 1) % sizes.length];
                  updateWidgetSize(widget.id, nextSize);
                }}
                className="p-1.5 rounded hover:bg-slate-100 dark:hover:bg-slate-700 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 transition"
                aria-label="Resize widget"
                title={`Size: ${widget.size}`}
              >
                {widget.size === 'small' ? (
                  <Maximize2 className="w-3.5 h-3.5" />
                ) : (
                  <Minimize2 className="w-3.5 h-3.5" />
                )}
              </button>

              <div
                {...attributes}
                {...listeners}
                className="p-1.5 rounded hover:bg-slate-100 dark:hover:bg-slate-700 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 cursor-grab active:cursor-grabbing transition"
                aria-label="Drag to reorder"
                title="Drag to reorder"
              >
                <GripVertical className="w-3.5 h-3.5" />
              </div>
            </>
          )}

          {isLayoutLocked && (
            <Lock className="w-3.5 h-3.5 text-slate-400" />
          )}
        </div>
      </div>

      <div className="p-4">
        {children}
      </div>
    </div>
  );
}
