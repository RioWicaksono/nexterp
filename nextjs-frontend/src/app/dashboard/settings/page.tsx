'use client';

import { useState } from 'react';
import { User, Bell, Shield, Palette, Save, Loader2 } from 'lucide-react';

export default function SettingsPage() {
  const [tab, setTab] = useState<'profile' | 'notifications' | 'security' | 'appearance'>('profile');
  const [saving] = useState(false);

  const tabs = [
    { id: 'profile' as const, label: 'Profile', icon: User },
    { id: 'notifications' as const, label: 'Notifications', icon: Bell },
    { id: 'security' as const, label: 'Security', icon: Shield },
    { id: 'appearance' as const, label: 'Appearance', icon: Palette },
  ];

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Settings</h1>
        <p className="text-sm text-slate-500 mt-1">Manage your account and preferences</p>
      </div>
      <div className="flex gap-6">
        <div className="w-44 shrink-0">
          <nav className="space-y-0.5">
            {tabs.map(t => (
              <button
                key={t.id}
                onClick={() => setTab(t.id)}
                className={`w-full flex items-center gap-2 px-3 py-2 rounded-lg text-sm transition ${tab === t.id ? 'bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400 font-medium' : 'text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800'}`}
              >
                <t.icon className="w-4 h-4" />{t.label}
              </button>
            ))}
          </nav>
        </div>
        <div className="flex-1 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-6">
          {tab === 'profile' && (
            <div className="space-y-6">
              <h3 className="font-semibold text-slate-900 dark:text-white">Profile Information</h3>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">First Name</label>
                  <input type="text" defaultValue="System" className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-sm" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Last Name</label>
                  <input type="text" defaultValue="Administrator" className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-sm" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Email</label>
                  <input type="email" defaultValue="admin@nexterp.com" className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-sm" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Phone</label>
                  <input type="text" placeholder="+62 xxx xxxx xxxx" className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-sm" />
                </div>
              </div>
              <div className="flex justify-end pt-4 border-t border-slate-100 dark:border-slate-700">
                <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm">
                  <Save className="w-4 h-4" />Save Changes
                </button>
              </div>
            </div>
          )}
          {tab === 'notifications' && (
            <div className="space-y-4">
              <h3 className="font-semibold text-slate-900 dark:text-white">Notification Preferences</h3>
              {[
                { key: 'email', label: 'Email Notifications', desc: 'Receive notifications via email' },
                { key: 'push', label: 'Push Notifications', desc: 'Browser push notifications' },
                { key: 'weekly', label: 'Weekly Digest', desc: 'Summary of weekly activity' },
              ].map(item => (
                <div key={item.key} className="flex items-center justify-between py-3 border-b border-slate-100 dark:border-slate-700 last:border-0">
                  <div>
                    <p className="font-medium text-slate-900 dark:text-white">{item.label}</p>
                    <p className="text-sm text-slate-500">{item.desc}</p>
                  </div>
                  <div className="w-11 h-6 bg-blue-600 rounded-full relative cursor-pointer">
                    <span className="absolute top-0.5 left-5 w-5 h-5 bg-white rounded-full shadow" />
                  </div>
                </div>
              ))}
            </div>
          )}
          {tab === 'security' && (
            <div className="space-y-4">
              <h3 className="font-semibold text-slate-900 dark:text-white">Security Settings</h3>
              <div className="p-4 bg-slate-50 dark:bg-slate-700/50 rounded-lg">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium text-slate-900 dark:text-white">Password</p>
                    <p className="text-sm text-slate-500">Last changed 30 days ago</p>
                  </div>
                  <button className="px-3 py-1.5 border border-slate-300 dark:border-slate-600 rounded-lg text-sm hover:bg-slate-100 dark:hover:bg-slate-700">Change</button>
                </div>
              </div>
              <div className="p-4 bg-slate-50 dark:bg-slate-700/50 rounded-lg">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-green-100 dark:bg-green-900/30 flex items-center justify-center">
                      <Shield className="w-5 h-5 text-green-600" />
                    </div>
                    <div>
                      <p className="font-medium text-slate-900 dark:text-white flex items-center gap-2">
                        Two-Factor Auth <span className="px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded">Enabled</span>
                      </p>
                      <p className="text-sm text-slate-500">Extra layer of security</p>
                    </div>
                  </div>
                  <button className="px-3 py-1.5 border border-slate-300 text-red-600 rounded-lg text-sm hover:bg-red-50">Disable</button>
                </div>
              </div>
            </div>
          )}
          {tab === 'appearance' && (
            <div className="space-y-6">
              <h3 className="font-semibold text-slate-900 dark:text-white">Appearance</h3>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-3">Theme</label>
                <div className="flex gap-3">
                  {[
                    { value: 'light', label: 'Light', bg: 'bg-white', border: 'border-slate-300' },
                    { value: 'dark', label: 'Dark', bg: 'bg-slate-800', border: 'border-slate-600' },
                    { value: 'system', label: 'System', bg: 'bg-gradient-to-r from-white to-slate-800', border: 'border-slate-300' },
                  ].map(theme => (
                    <button
                      key={theme.value}
                      className={`flex-1 p-4 rounded-xl border-2 transition ${theme.border}`}
                    >
                      <div className={`w-full h-12 rounded-lg mb-2 ${theme.bg} border ${theme.border}`} />
                      <p className="text-sm font-medium text-slate-700 dark:text-slate-300">{theme.label}</p>
                    </button>
                  ))}
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
