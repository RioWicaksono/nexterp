import type { Meta, StoryObj } from '@storybook/react';
import { RecentActivity } from './RecentActivity';

const meta = {
  title: 'Dashboard/RecentActivity',
  component: RecentActivity,
  tags: ['autodocs'],
  parameters: {
    layout: 'padded',
  },
} satisfies Meta<typeof RecentActivity>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {},
};

export const Loading: Story = {
  args: {
    isLoading: true,
  },
};

export const WithActivities: Story = {
  args: {
    activities: [
      { id: '1', type: 'order', message: 'Purchase Order #PO-2024-001 approved', timestamp: '5 min ago', user: 'Sarah Chen' },
      { id: '2', type: 'user', message: 'New employee John Doe added to HRM', timestamp: '15 min ago', user: 'HR Admin' },
      { id: '3', type: 'alert', message: 'Low stock alert: Item ITM-001 below threshold', timestamp: '3 hours ago' },
      { id: '4', type: 'approval', message: 'Budget request BR-2024-012 approved', timestamp: '1 hour ago', user: 'Manager' },
    ],
  },
};
