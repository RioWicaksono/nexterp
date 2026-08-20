import type { Meta, StoryObj } from '@storybook/react';
import { AutoSaveIndicator } from './AutoSaveIndicator';

const meta = {
  title: 'Components/AutoSaveIndicator',
  component: AutoSaveIndicator,
  tags: ['autodocs'],
  parameters: {
    layout: 'padded',
  },
} satisfies Meta<typeof AutoSaveIndicator>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Idle: Story = {
  args: {
    status: 'idle',
    lastSavedAt: null,
    hasDraft: false,
  },
};

export const Saving: Story = {
  args: {
    status: 'saving',
    lastSavedAt: null,
    hasDraft: true,
  },
};

export const Saved: Story = {
  args: {
    status: 'saved',
    lastSavedAt: Date.now() - 5000,
    hasDraft: true,
    onRestore: () => console.log('Restore clicked'),
    onClear: () => console.log('Clear clicked'),
  },
};

export const Restored: Story = {
  args: {
    status: 'restored',
    lastSavedAt: Date.now() - 60000,
    hasDraft: true,
    onRestore: () => console.log('Restore clicked'),
    onClear: () => console.log('Clear clicked'),
  },
};

export const Error: Story = {
  args: {
    status: 'error',
    lastSavedAt: null,
    hasDraft: true,
  },
};
