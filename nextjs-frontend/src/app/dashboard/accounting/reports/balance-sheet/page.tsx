'use client';

import { useState } from 'react';
import { PageHeader } from '@/components/PageHeader';
import { Download, TrendingUp, TrendingDown, ChevronDown } from 'lucide-react';
import { downloadCSV } from '@/lib/export';
import { cn } from '@/lib/utils';
import { useRouter } from 'next/navigation';

// Mock financial data
const mockAccounts = [
  // Assets
  { id: '1', accountNumber: '1000', name: 'Cash', type: 'Asset', balance: 125000, normalBalance: 'Debit' },
  { id: '2', accountNumber: '1100', name: 'Accounts Receivable', type: 'Asset', balance: 45000, normalBalance: 'Debit' },
  { id: '3', accountNumber: '1200', name: 'Inventory', type: 'Asset', balance: 85000, normalBalance: 'Debit' },
  { id: '4', accountNumber: '1500', name: 'Equipment', type: 'Asset', balance: 150000, normalBalance: 'Debit' },
  { id: '5', accountNumber: '1600', name: 'Accumulated Depreciation', type: 'Asset', balance: -45000, normalBalance: 'Credit' },
  // Liabilities
  { id: '6', accountNumber: '2000', name: 'Accounts Payable', type: 'Liability', balance: 32000, normalBalance: 'Credit' },
  { id: '7', accountNumber: '2100', name: 'Salaries Payable', type: 'Liability', balance: 15000, normalBalance: 'Credit' },
  { id: '8', accountNumber: '2200', name: 'Notes Payable', type: 'Liability', balance: 50000, normalBalance: 'Credit' },
  // Equity
  { id: '9', accountNumber: '3000', name: 'Common Stock', type: 'Equity', balance: 100000, normalBalance: 'Credit' },
  { id: '10', accountNumber: '3100', name: 'Retained Earnings', type: 'Equity', balance: 73000, normalBalance: 'Credit' },
  // Revenue
  { id: '11', accountNumber: '4000', name: 'Sales Revenue', type: 'Revenue', balance: 250000, normalBalance: 'Credit' },
  { id: '12', accountNumber: '4100', name: 'Service Revenue', type: 'Revenue', balance: 75000, normalBalance: 'Credit' },
  // Expenses
  { id: '13', accountNumber: '5000', name: 'Cost of Goods Sold', type: 'Expense', balance: 150000, normalBalance: 'Debit' },
  { id: '14', accountNumber: '5100', name: 'Salaries Expense', type: 'Expense', balance: 75000, normalBalance: 'Debit' },
  { id: '15', accountNumber: '5200', name: 'Rent Expense', type: 'Expense', balance: 24000, normalBalance: 'Debit' },
  { id: '16', accountNumber: '5300', name: 'Utilities Expense', type: 'Expense', balance: 8000, normalBalance: 'Debit' },
  { id: '17', accountNumber: '5400', name: 'Depreciation Expense', type: 'Expense', balance: 15000, normalBalance: 'Debit' },
];

export default function BalanceSheetPage() {
  const [showDetails, setShowDetails] = useState(false);
  const router = useRouter();

  // Calculate totals
  const assets = mockAccounts.filter(a => a.type === 'Asset');
  const liabilities = mockAccounts.filter(a => a.type === 'Liability');
  const equity = mockAccounts.filter(a => a.type === 'Equity');

  const totalAssets = assets.reduce((sum, a) => sum + a.balance, 0);
  const totalLiabilities = liabilities.reduce((sum, a) => sum + a.balance, 0);
  const totalEquity = equity.reduce((sum, a) => sum + a.balance, 0);

  const exportReport = () => {
    const data = [
      ...assets.map(a => ({ Account: a.accountNumber, Name: a.name, Type: 'Asset', Balance: a.balance })),
      { Account: '', Name: `Total Assets: ${totalAssets.toFixed(2)}`, Type: '', Balance: totalAssets },
      ...liabilities.map(a => ({ Account: a.accountNumber, Name: a.name, Type: 'Liability', Balance: a.balance })),
      { Account: '', Name: `Total Liabilities: ${totalLiabilities.toFixed(2)}`, Type: '', Balance: totalLiabilities },
      ...equity.map(a => ({ Account: a.accountNumber, Name: a.name, Type: 'Equity', Balance: a.balance })),
      { Account: '', Name: `Total Equity: ${totalEquity.toFixed(2)}`, Type: '', Balance: totalEquity },
    ];
    downloadCSV(data as Record<string, unknown>[], [
      { key: 'Account', header: 'Account #' },
      { key: 'Name', header: 'Account Name' },
      { key: 'Type', header: 'Type' },
      { key: 'Balance', header: 'Balance' },
    ], { filename: 'balance-sheet' });
  };

  return (
    <div className="space-y-6">
      <PageHeader
        title="Balance Sheet"
        subtitle="Statement of Financial Position"
        breadcrumbs={[
          { label: 'Dashboard', href: '/dashboard' },
          { label: 'Accounting', href: '/dashboard/accounting' },
          { label: 'Reports' },
          { label: 'Balance Sheet' },
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

      {/* Report Period */}
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-4">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="font-medium text-slate-900 dark:text-white">As of Date</h3>
            <p className="text-sm text-slate-500">{new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })}</p>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-sm text-slate-500">Currency: USD</span>
          </div>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <SummaryCard
          label="Total Assets"
          value={totalAssets}
          icon={TrendingUp}
          color="bg-blue-500"
          percentage={100}
        />
        <SummaryCard
          label="Total Liabilities"
          value={totalLiabilities}
          icon={TrendingDown}
          color="bg-red-500"
          percentage={(totalLiabilities / totalAssets * 100).toFixed(1) + '%'}
        />
        <SummaryCard
          label="Total Equity"
          value={totalEquity}
          icon={TrendingUp}
          color="bg-green-500"
          percentage={(totalEquity / totalAssets * 100).toFixed(1) + '%'}
        />
      </div>

      {/* Balance Sheet Table */}
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 overflow-hidden">
        <div className="p-4 border-b border-slate-200 dark:border-slate-700">
          <h2 className="text-lg font-semibold text-slate-900 dark:text-white">Statement of Financial Position</h2>
        </div>

        <div className="divide-y divide-slate-200 dark:divide-slate-700">
          {/* Assets Section */}
          <Section
            title="ASSETS"
            accounts={assets}
            total={totalAssets}
            showDetails={showDetails}
            onToggleDetails={() => setShowDetails(!showDetails)}
          />

          {/* Liabilities Section */}
          <Section
            title="LIABILITIES"
            accounts={liabilities}
            total={totalLiabilities}
            showDetails={showDetails}
            onToggleDetails={() => setShowDetails(!showDetails)}
          />

          {/* Equity Section */}
          <Section
            title="EQUITY"
            accounts={equity}
            total={totalEquity}
            showDetails={showDetails}
            onToggleDetails={() => setShowDetails(!showDetails)}
          />
        </div>

        {/* Grand Total */}
        <div className="p-4 bg-slate-50 dark:bg-slate-900/50 border-t-2 border-slate-300 dark:border-slate-600">
          <div className="flex justify-between items-center">
            <span className="text-lg font-bold text-slate-900 dark:text-white">
              TOTAL LIABILITIES + EQUITY
            </span>
            <span className="text-xl font-bold text-slate-900 dark:text-white">
              ${(totalLiabilities + totalEquity).toLocaleString('en-US', { minimumFractionDigits: 2 })}
            </span>
          </div>
          <div className="flex justify-end mt-1">
            <span className={cn(
              'text-sm font-medium',
              totalAssets === totalLiabilities + totalEquity
                ? 'text-green-600'
                : 'text-red-600'
            )}>
              {totalAssets === totalLiabilities + totalEquity ? '✓ Balanced' : '⚠ Unbalanced'}
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}

function SummaryCard({
  label,
  value,
  icon: Icon,
  color,
  percentage,
}: {
  label: string;
  value: number;
  icon: typeof TrendingUp;
  color: string;
  percentage: string | number;
}) {
  return (
    <div className="bg-white dark:bg-slate-800 rounded-xl p-5 border border-slate-200 dark:border-slate-700">
      <div className="flex items-center gap-3">
        <div className={cn('p-2.5 rounded-lg', color)}>
          <Icon className="w-5 h-5 text-white" />
        </div>
        <div>
          <p className="text-2xl font-bold text-slate-900 dark:text-white">
            ${value.toLocaleString('en-US', { minimumFractionDigits: 2 })}
          </p>
          <div className="flex items-center gap-2">
            <p className="text-sm text-slate-500">{label}</p>
            <span className="text-xs px-1.5 py-0.5 bg-slate-100 dark:bg-slate-700 rounded text-slate-500">
              {percentage}
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}

function Section({
  title,
  accounts,
  total,
  showDetails,
  onToggleDetails,
}: {
  title: string;
  accounts: typeof mockAccounts;
  total: number;
  showDetails: boolean;
  onToggleDetails: () => void;
}) {
  return (
    <div>
      <button
        onClick={onToggleDetails}
        className="w-full px-4 py-3 flex items-center justify-between hover:bg-slate-50 dark:hover:bg-slate-700/30 transition"
      >
        <span className="font-semibold text-slate-700 dark:text-slate-300">{title}</span>
        <div className="flex items-center gap-2">
          <span className="font-semibold text-slate-900 dark:text-white">
            ${total.toLocaleString('en-US', { minimumFractionDigits: 2 })}
          </span>
          <ChevronDown className={cn('w-4 h-4 text-slate-400 transition', showDetails && 'rotate-180')} />
        </div>
      </button>
      {showDetails && (
        <div className="px-4 pb-2">
          {accounts.map((account) => (
            <div key={account.id} className="flex justify-between py-2 text-sm border-b border-slate-100 dark:border-slate-800 last:border-0">
              <span className="text-slate-600 dark:text-slate-400">
                {account.accountNumber} - {account.name}
              </span>
              <span className={cn(
                'font-mono',
                account.balance >= 0 ? 'text-slate-700 dark:text-slate-300' : 'text-red-600'
              )}>
                ${account.balance.toLocaleString('en-US', { minimumFractionDigits: 2 })}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
