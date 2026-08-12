"use client";

import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  AreaChart,
  Area,
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  Legend,
  TooltipProps,
} from "recharts";

const COLORS = {
  primary: "#2563eb",
  secondary: "#10b981",
  tertiary: "#f59e0b",
  quaternary: "#ef4444",
  gray: "#94a3b8",
};

// Sales Data
const salesData: ChartDataPoint[] = [
  { name: "Jan", sales: 4200, revenue: 12500 },
  { name: "Feb", sales: 3800, revenue: 11200 },
  { name: "Mar", sales: 5100, revenue: 14800 },
  { name: "Apr", sales: 4600, revenue: 13900 },
  { name: "May", sales: 5800, revenue: 16500 },
  { name: "Jun", sales: 6200, revenue: 18200 },
  { name: "Jul", sales: 5900, revenue: 17100 },
];

// Revenue by Category
interface CategoryDataPoint extends ChartDataPoint {
  color: string;
}

const categoryData: CategoryDataPoint[] = [
  { name: "Electronics", value: 4500, color: COLORS.primary },
  { name: "Furniture", value: 2800, color: COLORS.secondary },
  { name: "Office", value: 2100, color: COLORS.tertiary },
  { name: "IT Services", value: 1800, color: "#8b5cf6" },
  { name: "Other", value: 1200, color: COLORS.gray },
];

// Monthly Comparison
const monthlyData: ChartDataPoint[] = [
  { name: "Week 1", current: 4200, previous: 3800 },
  { name: "Week 2", current: 4800, previous: 4200 },
  { name: "Week 3", current: 5100, previous: 4600 },
  { name: "Week 4", current: 5900, previous: 5200 },
];

// Type definitions for chart data
interface ChartDataPoint {
  name: string;
  [key: string]: string | number;
}

interface TooltipPayloadEntry {
  name?: string;
  value?: string | number;
  color?: string;
}

interface CustomTooltipProps extends TooltipProps<number, string> {
  label?: string;
}

// Custom Tooltip with proper types
const CustomTooltip = ({ active, payload, label }: CustomTooltipProps) => {
  if (active && payload && payload.length) {
    return (
      <div className="bg-white dark:bg-slate-800 p-3 rounded-xl shadow-lg border border-slate-200 dark:border-slate-700">
        <p className="font-semibold text-slate-800 dark:text-white mb-2">{label}</p>
        {payload.map((entry: TooltipPayloadEntry, index: number) => (
          <p key={index} className="text-sm" style={{ color: entry.color }}>
            {entry.name}: {typeof entry.value === "number"
              ? entry.value.toLocaleString()
              : entry.value}
          </p>
        ))}
      </div>
    );
  }
  return null;
};

interface ChartCardProps {
  title: string;
  subtitle?: string;
  children: React.ReactNode;
  className?: string;
}

function ChartCard({ title, subtitle, children, className = "" }: ChartCardProps) {
  return (
    <div className={`bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6 ${className}`}>
      <div className="mb-4">
        <h3 className="text-lg font-semibold text-slate-900 dark:text-white">{title}</h3>
        {subtitle && <p className="text-sm text-slate-500 dark:text-slate-400">{subtitle}</p>}
      </div>
      {children}
    </div>
  );
}

export function SalesLineChart() {
  return (
    <ChartCard title="Sales Trend" subtitle="Last 7 months performance" className="lg:col-span-2">
      <ResponsiveContainer width="100%" height={280}>
        <AreaChart data={salesData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
          <defs>
            <linearGradient id="salesGradient" x1="0" y1="0" x2="0" y2="1">
              <stop offset="5%" stopColor={COLORS.primary} stopOpacity={0.3} />
              <stop offset="95%" stopColor={COLORS.primary} stopOpacity={0} />
            </linearGradient>
            <linearGradient id="revenueGradient" x1="0" y1="0" x2="0" y2="1">
              <stop offset="5%" stopColor={COLORS.secondary} stopOpacity={0.3} />
              <stop offset="95%" stopColor={COLORS.secondary} stopOpacity={0} />
            </linearGradient>
          </defs>
          <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
          <XAxis dataKey="name" stroke="#94a3b8" fontSize={12} />
          <YAxis stroke="#94a3b8" fontSize={12} tickFormatter={(v) => `${(v / 1000).toFixed(0)}k`} />
          <Tooltip content={<CustomTooltip />} />
          <Area
            type="monotone"
            dataKey="revenue"
            stroke={COLORS.secondary}
            fillOpacity={1}
            fill="url(#revenueGradient)"
            strokeWidth={2}
            name="Revenue ($)"
          />
          <Area
            type="monotone"
            dataKey="sales"
            stroke={COLORS.primary}
            fillOpacity={1}
            fill="url(#salesGradient)"
            strokeWidth={2}
            name="Sales (units)"
          />
        </AreaChart>
      </ResponsiveContainer>
    </ChartCard>
  );
}

export function RevenuePieChart() {
  return (
    <ChartCard title="Revenue by Category" subtitle="Current month breakdown">
      <ResponsiveContainer width="100%" height={220}>
        <PieChart>
          <Pie
            data={categoryData}
            cx="50%"
            cy="50%"
            innerRadius={50}
            outerRadius={80}
            paddingAngle={2}
            dataKey="value"
          >
            {categoryData.map((entry, index) => (
              <Cell key={`cell-${index}`} fill={entry.color} />
            ))}
          </Pie>
          <Tooltip content={<CustomTooltip />} />
          <Legend
            iconType="circle"
            iconSize={8}
            wrapperStyle={{ fontSize: "12px" }}
          />
        </PieChart>
      </ResponsiveContainer>
    </ChartCard>
  );
}

export function WeeklyComparisonChart() {
  return (
    <ChartCard title="Weekly Comparison" subtitle="Current vs Previous Month">
      <ResponsiveContainer width="100%" height={220}>
        <BarChart data={monthlyData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
          <XAxis dataKey="name" stroke="#94a3b8" fontSize={12} />
          <YAxis stroke="#94a3b8" fontSize={12} />
          <Tooltip content={<CustomTooltip />} />
          <Bar dataKey="current" fill={COLORS.primary} radius={[4, 4, 0, 0]} name="Current" />
          <Bar dataKey="previous" fill={COLORS.gray} radius={[4, 4, 0, 0]} name="Previous" />
        </BarChart>
      </ResponsiveContainer>
    </ChartCard>
  );
}

export function TrendLineChart() {
  return (
    <ChartCard title="Order Trends" subtitle="Daily order volume">
      <ResponsiveContainer width="100%" height={180}>
        <LineChart data={monthlyData} margin={{ top: 5, right: 10, left: -20, bottom: 0 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
          <XAxis dataKey="name" stroke="#94a3b8" fontSize={12} />
          <YAxis stroke="#94a3b8" fontSize={12} />
          <Tooltip content={<CustomTooltip />} />
          <Line
            type="monotone"
            dataKey="current"
            stroke={COLORS.primary}
            strokeWidth={2}
            dot={{ fill: COLORS.primary, strokeWidth: 0, r: 4 }}
            name="Orders"
          />
        </LineChart>
      </ResponsiveContainer>
    </ChartCard>
  );
}

export { salesData, categoryData, monthlyData };
