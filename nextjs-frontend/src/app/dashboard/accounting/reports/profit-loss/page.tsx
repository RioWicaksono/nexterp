'use client';

import { useState } from 'react';
import { PageHeader } from '@/components/PageHeader';
import { Download, TrendingUp, TrendingDown, ChevronDown } from 'lucide-react';
import { downloadCSV } from '@/lib/export';
import { cn } from '@/lib/utils';

// Mock data
const mockRevenue = [
  { id: '1', accountNumber: '4000', name: 'Sales Revenue', balance: 250000 },
  { id: '2', accountNumber: '4100', name: 'Service Revenue', balance: 75000 },
];

const mockExpenses = [
  { id: '3', accountNumber: '5000', name: 'Cost of Goods Sold', balance: 150000 },
  { id: '4', accountNumber: '5100', name: 'Salaries Expense', balance: 75000 },
  { id: '5', accountNumber: '5200', name: 'Rent Expense', balance: 24000 },
  { id: '6', accountNumber: '5300', name: 'Utilities Expense', balance: 8000 },
  { id: '7', accountNumber: '5400', name: 'Depreciation Expense', balance: 15000 },
];

export default function ProfitLossPage() {
  const [showDetails, setShowDetails] = useState(true);

  const totalRevenue = mockRevenue.reduce((sum, a) => sum + a.balance, 0);
  const totalExpenses = mockExpenses.reduce((sum, a) => sum + a.balance, 0);
  const grossProfit = totalRevenue;
  const netIncome = grossProfit - totalExpenses;

  const exportReport = () => {
    const data = [
      ...mockRevenue.map(a => ({ Account: a.accountNumber, Name: a.name, Type: 'Revenue', Balance: a.balance })),
      { Account: '', Name: 'Total Revenue', Type: '', Balance: totalRevenue },
      ...mockExpenses.map(a => ({ Account: a.accountNumber, Name: a.name, Type: 'Expense', Balance: a.balance })),
      { Account: '', Name: 'Total Expenses', Type: '', Balance: totalExpenses },
      { Account: '', Name: 'NET INCOME', Type: '', Balance: netIncome },
    ];
    downloadCSV(data as Record<string, unknown>[], [
      { key: 'Account', header: 'Account #' },
      { key: 'Name', header: 'Account Name' },
      { key: 'Type', header: 'Type' },
      { key: 'Balance', header: 'Balance' },
    ], { filename: 'profit-loss-statement' });
  };

  return (
    <div className="space-y-6">
      <PageHeader
        title="Profit & Loss Statement"
        subtitle="Income Statement"
        breadcrumbs={[
          { label: 'Dashboard', href: '/dashboard' },
          { label: 'Accounting', href: '/dashboard/accounting' },
          { label: 'Reports' },
          { label: 'Profit & Loss' },
        ]}
        actions={
          <button
            onClick={exportReport}
            className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 text-sm"
          >
            <Download className="w-4 h-4" />
            Export CSV
          </button>
        }
      />

      {/* Date Range */}
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-4">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="font-medium text-slate-900 dark:text-white">Period</h3>
            <p className="text-sm text-slate-500">
              January 1, 2024 - December 31, 2024
            </p>
          </div>
          <div className="text-right">
            <p className="text-sm text-slate-500">Currency: USD</p>
          </div>
        </div>
      </div>

      {/* Key Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <MetricCard
          label="Total Revenue"
          value={totalRevenue}
          icon={TrendingUp}
          color="bg-green-500"
          trend="+12%"
        />
        <MetricCard
          label="Total Expenses"
          value={totalExpenses}
          icon={TrendingDown}
          color="bg-red-500"
          trend="-5%"
          trendUp={false}
        />
        <MetricCard
          label="Gross Profit"
          value={grossProfit}
          icon={TrendingUp}
          color="bg-blue-500"
        />
        <MetricCard
          label="Net Income"
          value={netIncome}
          icon={netIncome >= 0 ? TrendingUp : TrendingDown}
          color={netIncome >= 0 ? 'bg-emerald-500' : 'bg-red-500'}
          trend={netIncome >= 0 ? '+Profit' : '-Loss'}
        />
      </div>

      {/* P&L Table */}
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 overflow-hidden">
        <div className="p-4 border-b border-slate-200 dark:border-slate-700">
          <h2 className="text-lg font-semibold text-slate-900 dark:text-white">
            Statement of Operations
          </h2>
        </div>

        <div className="divide-y divide-slate-200 dark:divide-slate-700">
          {/* Revenue Section */}
          <PLSection
            title="REVENUE"
            accounts={mockRevenue}
            total={totalRevenue}
            showDetails={showDetails}
            onToggle={() => setShowDetails(!showDetails)}
            accentColor="text-green-600"
            bgColor="bg-green-50 dark:bg-green-900/20"
          />

          {/* Cost of Goods Sold */}
          <div className="px-4 py-3 bg-slate-50 dark:bg-slate-900/50">
            <div className="flex justify-between items-center">
              <span className="font-semibold text-slate-700 dark:text-slate-300">
                Cost of Goods Sold
              </span>
              <span className="font-semibold text-red-600">
                -${mockExpenses[0].balance.toLocaleString('en-US', { minimumFractionDigits: 2 })}
              </span>
            </div>
          </div>

          <div className="px-4 py-3 bg-blue-50 dark:bg-blue-900/20 border-t border-b border-blue-200 dark:border-blue-800">
            <div className="flex justify-between items-center">
              <span className="font-bold text-blue-700 dark:text-blue-300">
                GROSS PROFIT
              </span>
              <span className="font-bold text-blue-700 dark:text-blue-300">
                ${grossProfit.toLocaleString('en-US', { minimumFractionDigits: 2 })}
              </span>
            </div>
          </div>

          {/* Expenses Section */}
          <PLSection
            title="OPERATING EXPENSES"
            accounts={mockExpenses.slice(1)}
            total={totalExpenses - mockExpenses[0].balance}
            showDetails={showDetails}
            onToggle={() => setShowDetails(!showDetails)}
            accentColor="text-red-600"
            bgColor="bg-red-50 dark:bg-red-900/20"
          />

          {/* Net Income */}
          <div className="px-4 py-4 bg-emerald-50 dark:bg-emerald-900/30 border-t-2 border-emerald-300 dark:border-emerald-700">
            <div className="flex justify-between items-center">
              <span className="text-lg font-bold text-emerald-700 dark:text-emerald-300">
                NET INCOME
              </span>
              <span className={cn(
                'text-lg font-bold',
                netIncome >= 0 ? 'text-emerald-600' : 'text-red-600'
              )}>
                {netIncome >= 0 ? '' : '-'}${Math.abs(netIncome).toLocaleString('en-US', { minimumFractionDigits: 2 })}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

function MetricCard({
  label,
  value,
  icon: Icon,
  color,
  trend,
  trendUp = true,
}: {
  label: string;
  value: number;
  icon: typeof TrendingUp;
  color: string;
  trend?: string;
  trendUp?: boolean;
}) {
  return (
    <div className="bg-white dark:bg-slate-800 rounded-xl p-5 border border-slate-200 dark:border-slate-700">
      <div className="flex items-center gap-3">
        <div className={cn('p-2.5 rounded-lg', color)}>
          <Icon className="w-5 h-5 text-white" />
        </div>
        <div>
          <p className="text-2xl font-bold text-slate-900 dark:text-white">
            ${value.toLocaleString('en-US', { minimumFractionDigits: 0 })}
          </p>
          <div className="flex items-center gap-2">
            <p className="text-sm text-slate-500">{label}</p>
            {trend && (
              <span className={cn(
                'text-xs px-1.5 py-0.5 rounded',
                trendUp ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
              )}>
                {trend}
              </span>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

function PLSection({
  title,
  accounts,
  total,
  showDetails,
  onToggle,
  accentColor,
  bgColor,
}: {
  title: string;
  accounts: { id: string; accountNumber: string; name: string; balance: number }[];
  total: number;
  showDetails: boolean;
  onToggle: () => void;
  accentColor: string;
  bgColor: string;
}) {
  return (
    <div className={cn('px-4 py-2', bgColor)}>
      <button
        onClick={onToggle}
        className="w-full flex items-center justify-between"
      >
        <span className={cn('font-semibold', accentColor)}>{title}</span>
        <div className="flex items-center gap-2">
          <span className={cn('font-semibold', accentColor)}>
            ${total.toLocaleString('en-US', { minimumFractionDigits: 2 })}
          </span>
          <ChevronDown className={cn('w-4 h-4 transition', showDetails && 'rotate-180')} />
        </div>
      </button>
      {showDetails && (
        <div className="mt-2 space-y-1 pl-4">
          {accounts.map((account) => (
            <div key={account.id} className="flex justify-between text-sm">
              <span className="text-slate-600 dark:text-slate-400">
                {account.accountNumber} - {account.name}
              </span>
              <span className="text-slate-700 dark:text-slate-300 font-mono">
                ${account.balance.toLocaleString('en-US', { minimumFractionDigits: 2 })}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
