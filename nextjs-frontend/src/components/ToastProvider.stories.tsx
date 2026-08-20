import type { Meta, StoryObj } from '@storybook/react';
import { ToastProvider, useToast } from './ToastProvider';

function ToastDemo() {
  const toast = useToast();
  return (
    <div style={{ padding: '2rem' }}>
      <h2 style={{ marginBottom: '1rem' }}>Toast Provider Demo</h2>
      <p style={{ marginBottom: '1rem' }}>Click buttons to see different toast types:</p>
      <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
        <button
          onClick={() => toast('success', 'Success!', 'Your changes have been saved.')}
          style={{ padding: '0.5rem 1rem', background: '#22c55e', color: 'white', border: 'none', borderRadius: '0.375rem', cursor: 'pointer' }}
        >
          Success
        </button>
        <button
          onClick={() => toast('error', 'Error!', 'Something went wrong.')}
          style={{ padding: '0.5rem 1rem', background: '#ef4444', color: 'white', border: 'none', borderRadius: '0.375rem', cursor: 'pointer' }}
        >
          Error
        </button>
        <button
          onClick={() => toast('warning', 'Warning!', 'Please review your input.')}
          style={{ padding: '0.5rem 1rem', background: '#f59e0b', color: 'white', border: 'none', borderRadius: '0.375rem', cursor: 'pointer' }}
        >
          Warning
        </button>
        <button
          onClick={() => toast('info', 'Info', 'New update available.')}
          style={{ padding: '0.5rem 1rem', background: '#3b82f6', color: 'white', border: 'none', borderRadius: '0.375rem', cursor: 'pointer' }}
        >
          Info
        </button>
      </div>
    </div>
  );
}

const meta: Meta = {
  title: 'Components/ToastProvider',
  component: ToastProvider,
  tags: ['autodocs'],
  parameters: {
    layout: 'fullscreen',
  },
};

export default meta;

export const Demo: StoryObj = {
  render: () => (
    <ToastProvider>
      <ToastDemo />
    </ToastProvider>
  ),
};
