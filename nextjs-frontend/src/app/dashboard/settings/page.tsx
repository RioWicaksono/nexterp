'use client';

import { User, Shield, Bell, Database } from 'lucide-react';

const sections = [
  { id: 'profile', icon: User, title: 'Profile', description: 'Update your profile information' },
  { id: 'security', icon: Shield, title: 'Security', description: 'Password & 2FA settings' },
  { id: 'notifications', icon: Bell, title: 'Notifications', description: 'Email & push preferences' },
  { id: 'api', icon: Database, title: 'API Keys', description: 'Manage API access' },
];

export default function SettingsPage() {
  return (
    <div className="p-6 space-y-6">
      <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Settings</h1>
      <div className="grid gap-4">
        {sections.map((section) => (
          <button
            key={section.id}
            type="button"
            className="flex items-center gap-4 p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 text-left w-full hover:border-blue-500 dark:hover:border-blue-500 transition-colors"
          >
            <div className="p-3 bg-slate-100 dark:bg-slate-900 rounded-lg">
              <section.icon className="w-6 h-6 text-slate-600 dark:text-slate-400" />
            </div>
            <div>
              <h3 className="font-medium text-slate-900 dark:text-white">{section.title}</h3>
              <p className="text-sm text-slate-500">{section.description}</p>
            </div>
          </button>
        ))}
      </div>
    </div>
  );
}
