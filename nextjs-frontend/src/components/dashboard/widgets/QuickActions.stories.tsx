import type { Meta, StoryObj } from '@storybook/react';
import { QuickActions } from './QuickActions';

const meta = {
  title: 'Dashboard/QuickActions',
  component: QuickActions,
  tags: ['autodocs'],
  parameters: {
    layout: 'padded',
  },
} satisfies Meta<typeof QuickActions>;

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
