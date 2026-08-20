'use client';

import { ResponsiveContainer, BarChart, Bar, XAxis, YAxis, Tooltip, CartesianGrid } from 'recharts';

interface ChartData {
  name: string;
  employees?: number;
  inventory?: number;
  orders?: number;
  [key: string]: string | number | undefined;
}

interface ChartWidgetProps {
  data?: ChartData[];
  isLoading?: boolean;
}

export function ChartWidget({ data, isLoading }: ChartWidgetProps) {
  const defaultData: ChartData[] = [
    { name: 'Jan', employees: 45, inventory: 120, orders: 89 },
    { name: 'Feb', employees: 48, inventory: 135, orders: 95 },
    { name: 'Mar', employees: 52, inventory: 150, orders: 102 },
    { name: 'Apr', employees: 55, inventory: 142, orders: 110 },
    { name: 'May', employees: 58, inventory: 168, orders: 125 },
    { name: 'Jun', employees: 62, inventory: 185, orders: 138 },
  ];

  const chartData = data || defaultData;

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-4">
        <div className="h-64 bg-slate-200 dark:bg-slate-700 rounded-lg" />
      </div>
    );
  }

  return (
    <div className="h-64">
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={chartData} margin={{ top: 10, right: 10, left: -10, bottom: 0 }}>
          <CartesianGrid strokeDasharray="3 3" className="stroke-slate-200 dark:stroke-slate-700" />
          <XAxis
            dataKey="name"
            tick={{ fill: 'var(--slate-500)', fontSize: 12 }}
            axisLine={{ stroke: 'var(--slate-300)' }}
          />
          <YAxis
            tick={{ fill: 'var(--slate-500)', fontSize: 12 }}
            axisLine={{ stroke: 'var(--slate-300)' }}
          />
          <Tooltip
            contentStyle={{
              backgroundColor: 'var(--slate-800)',
              border: 'none',
              borderRadius: '8px',
              color: 'white',
            }}
          />
          <Bar dataKey="employees" fill="#3b82f6" radius={[4, 4, 0, 0]} name="Employees" />
          <Bar dataKey="orders" fill="#f97316" radius={[4, 4, 0, 0]} name="Orders" />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
