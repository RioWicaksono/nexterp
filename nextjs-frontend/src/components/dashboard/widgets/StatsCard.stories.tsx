import type { Meta, StoryObj } from '@storybook/react';
import { StatsCard } from './StatsCard';
import { Users } from 'lucide-react';

const meta = {
  title: 'Dashboard/StatsCard',
  component: StatsCard,
  tags: ['autodocs'],
  parameters: {
    layout: 'padded',
  },
} satisfies Meta<typeof StatsCard>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {
    label: 'Total Employees',
    value: '85',
    icon: Users,
    bgClass: 'bg-blue-500',
    href: '/dashboard/hrm',
  },
};

export const Loading: Story = {
  args: {
    label: 'Total Employees',
    value: '85',
    icon: Users,
    bgClass: 'bg-blue-500',
    href: '/dashboard/hrm',
    isLoading: true,
  },
};

export const ZeroValue: Story = {
  args: {
    label: 'Pending Orders',
    value: '0',
    icon: Users,
    bgClass: 'bg-orange-500',
    href: '/dashboard/purchasing',
  },
};
