import type { Meta, StoryObj } from '@storybook/react';
import { ChartWidget } from './ChartWidget';

const meta = {
  title: 'Dashboard/ChartWidget',
  component: ChartWidget,
  tags: ['autodocs'],
  parameters: {
    layout: 'padded',
  },
} satisfies Meta<typeof ChartWidget>;

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

export const WithData: Story = {
  args: {
    data: [
      { name: 'Week 1', employees: 45, orders: 89 },
      { name: 'Week 2', employees: 52, orders: 102 },
      { name: 'Week 3', employees: 58, orders: 115 },
      { name: 'Week 4', employees: 62, orders: 128 },
    ],
  },
};
