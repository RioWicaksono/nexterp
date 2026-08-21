import type { Meta, StoryObj } from '@storybook/react';
import { within, userEvent } from '@storybook/testing-library';

const meta = { title: 'Forms/Button', component: null };

export default meta;

export const Primary: StoryObj = {
  args: { label: 'Button' },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await userEvent.click(canvas.getByRole('button'));
    await expect(canvas.getByRole('button')).toHaveFocus();
  },
};

export const Secondary: StoryObj = { args: { label: 'Cancel', variant: 'secondary' };
export const Danger: StoryObj = { args: { label: 'Delete', variant: 'danger' };
export const Loading: StoryObj = { args: { label: 'Loading', loading: true };
export const Disabled: StoryObj = { args: { label: 'Disabled', disabled: true };
