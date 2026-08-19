'use client';

import { useState } from 'react';
import { Breadcrumbs } from '@/components/Breadcrumbs';
import { PageHeader } from '@/components/PageHeader';
import { useToast } from '@/hooks/useToast';
import { useAuthStore } from '@/lib/store';
import { User, Bell, Shield, Palette, Globe, Save, Loader2, Check } from 'lucide-react';

export default function SettingsPage() {
  const toast = useToast();
  const { user } = useAuthStore();
  const [saving, setSaving] = useState(false);
  const [activeTab, setActiveTab] = useState<'profile' | 'notifications' | 'security' | 'appearance'>('profile');

  const [profile, setProfile] = useState({
    firstName: user?.firstName || '',
    lastName: user?.lastName || '',
    email: user?.email || '',
    phone: '',
    language: 'en',
    timezone: 'Asia/Jakarta',
  });

  const [notifications, setNotifications] = useState({
    email: true,
    push: true,
    weekly: false,
    marketing: false,
  });

  const [appearance, setAppearance] = useState({
    theme: 'system',
    compact: false,
  });

  const handleSave = async (section: string) => {
    setSaving(true);
    await new Promise(r => setTimeout(r, 800));
    toast('success', 'Saved!', `${section} settings updated`);
    setSaving(false);
  };

  const tabs = [
    { id: 'profile', label: 'Profile', icon: User },
    { id: 'notifications', label: 'Notifications', icon: Bell },
    { id: 'security', label: 'Security', icon: Shield },
    { id: 'appearance', label: 'Appearance', icon: Palette },
  ] as const;

  return (
    <div>
      <Breadcrumbs items={[{ label: 'Dashboard', href: '/dashboard' }, { label: 'Settings' }]} />

      <PageHeader title="Settings" subtitle="Manage your account and application preferences" />

      <div className="flex gap-6">
        {/* Settings Nav */}
        <div className="w-48 shrink-0">
          <nav className="space-y-0.5">
            {tabs.map(tab => (
              <button key={tab.id} onClick={() => setActiveTab(tab.id)} className={`w-full flex items-center gap-2 px-3 py-2 rounded-lg text-sm transition ${activeTab === tab.id ? 'bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400 font-medium' : 'text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800'}`}>
                <tab.icon className="w-4 h-4" />
                {tab.label}
              </button>
            ))}
          </nav>
        </div>

        {/* Content */}
        <div className="flex-1 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-6">
          {activeTab === 'profile' && (
            <div className="space-y-6">
              <div>
                <h3 className="font-semibold text-slate-900 dark:text-white mb-4">Profile Information</h3>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">First Name</label>
                    <input type="text" value={profile.firstName} onChange={e => setProfile({ ...profile, firstName: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg dark:bg-slate-700" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Last Name</label>
                    <input type="text" value={profile.lastName} onChange={e => setProfile({ ...profile, lastName: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg dark:bg-slate-700" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Email</label>
                    <input type="email" value={profile.email} onChange={e => setProfile({ ...profile, email: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg dark:bg-slate-700" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Phone</label>
                    <input type="text" value={profile.phone} onChange={e => setProfile({ ...profile, phone: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg dark:bg-slate-700" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Language</label>
                    <select value={profile.language} onChange={e => setProfile({ ...profile, language: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg dark:bg-slate-700">
                      <option value="en">English</option>
                      <option value="id">Bahasa Indonesia</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Timezone</label>
                    <select value={profile.timezone} onChange={e => setProfile({ ...profile, timezone: e.target.value })} className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg dark:bg-slate-700">
                      <option value="Asia/Jakarta">Asia/Jakarta (WIB)</option>
                      <option value="Asia/Ujung_Pandang">Asia/Makassar (WITA)</option>
                      <option value="Asia/Jayapura">Asia/Jayapura (WIT)</option>
                    </select>
                  </div>
                </div>
              </div>
              <div className="flex justify-end pt-4 border-t border-slate-100 dark:border-slate-700">
                <button onClick={() => handleSave('Profile')} disabled={saving} className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm disabled:opacity-50">
                  {saving ? <Loader2 className="w-4 h-4 animate-spin" /> : <Save className="w-4 h-4" />}
                  Save Changes
                </button>
              </div>
            </div>
          )}

          {activeTab === 'notifications' && (
            <div className="space-y-6">
              <h3 className="font-semibold text-slate-900 dark:text-white">Notification Preferences</h3>
              {[
                { key: 'email', label: 'Email Notifications', desc: 'Receive notifications via email' },
                { key: 'push', label: 'Push Notifications', desc: 'Browser push notifications' },
                { key: 'weekly', label: 'Weekly Digest', desc: 'Summary of weekly activity' },
                { key: 'marketing', label: 'Marketing', desc: 'Product updates and announcements' },
              ].map(item => (
                <div key={item.key} className="flex items-center justify-between py-3 border-b border-slate-100 dark:border-slate-700 last:border-0">
                  <div>
                    <p className="font-medium text-slate-900 dark:text-white">{item.label}</p>
                    <p className="text-sm text-slate-500">{item.desc}</p>
                  </div>
                  <button onClick={() => setNotifications(n => ({ ...n, [item.key]: !n[item.key as keyof typeof notifications] })} className={`relative w-11 h-6 rounded-full transition ${notifications[item.key as keyof typeof notifications] ? 'bg-blue-600' : 'bg-slate-300 dark:bg-slate-600'}`}>
                    <span className={`absolute top-0.5 w-5 h-5 bg-white rounded-full shadow transition-transform ${notifications[item.key as keyof typeof notifications] ? 'translate-x-5' : 'translate-x-0.5'}`} />
                  </button>
                </div>
              ))}
              <div className="flex justify-end pt-4">
                <button onClick={() => handleSave('Notification')} className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm">
                  <Save className="w-4 h-4" /> Save
                </button>
              </div>
            </div>
          )}

          {activeTab === 'security' && (
            <div className="space-y-6">
              <h3 className="font-semibold text-slate-900 dark:text-white">Security Settings</h3>
              <div className="space-y-4">
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
                        <p className="font-medium text-slate-900 dark:text-white flex items-center gap-2">Two-Factor Auth <span className="px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded">Enabled</span></p>
                        <p className="text-sm text-slate-500">Extra layer of security</p>
                      </div>
                    </div>
                    <button className="px-3 py-1.5 border border-slate-300 dark:border-slate-600 rounded-lg text-sm text-red-600 hover:bg-red-50">Disable</button>
                  </div>
                </div>
                <div className="p-4 bg-slate-50 dark:bg-slate-700/50 rounded-lg">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium text-slate-900 dark:text-white">Active Sessions</p>
                      <p className="text-sm text-slate-500">1 active session</p>
                    </div>
                    <button className="px-3 py-1.5 border border-red-200 text-red-600 rounded-lg text-sm hover:bg-red-50">Sign out all</button>
                  </div>
                </div>
              </div>
            </div>
          )}

          {activeTab === 'appearance' && (
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
                    <button key={theme.value} onClick={() => setAppearance(a => ({ ...a, theme: theme.value }))} className={`flex-1 p-4 rounded-xl border-2 transition ${appearance.theme === theme.value ? 'border-blue-500' : 'border-transparent ${theme.border}`}>
                      <div className={`w-full h-12 rounded-lg ${theme.bg} mb-2 border ${theme.border}`} />
                      <p className="text-sm font-medium text-slate-700 dark:text-slate-300">{theme.label}</p>
                    </button>
                  ))}
                </div>
              </div>
              <div className="flex items-center justify-between py-3">
                <div>
                  <p className="font-medium text-slate-900 dark:text-white">Compact Mode</p>
                  <p className="text-sm text-slate-500">Reduce spacing for denser UI</p>
                </div>
                <button onClick={() => setAppearance(a => ({ ...a, compact: !a.compact }))} className={`relative w-11 h-6 rounded-full transition ${appearance.compact ? 'bg-blue-600' : 'bg-slate-300 dark:bg-slate-600'}`}>
                  <span className={`absolute top-0.5 w-5 h-5 bg-white rounded-full shadow transition-transform ${appearance.compact ? 'translate-x-5' : 'translate-x-0.5'}`} />
                </button>
              </div>
              <div className="flex justify-end pt-4 border-t border-slate-100 dark:border-slate-700">
                <button onClick={() => handleSave('Appearance')} className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm">
                  <Save className="w-4 h-4" /> Save
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
