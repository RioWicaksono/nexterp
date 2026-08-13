'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import {
  Package, DollarSign, ShoppingCart, FileText, Users, BarChart3,
  Building2, CheckCircle2, Play, ArrowRight, Sparkles, Shield, Zap,
  Globe, Clock, Headphones, ChevronRight, Menu, X, Star, Sun, Moon, Monitor
} from 'lucide-react'
import { useTheme } from './providers/ThemeProvider'

const features = [
  { icon: Package, title: 'Inventory Management', desc: 'Multi-warehouse stock tracking with batch/serial number support for complete traceability', color: 'blue' },
  { icon: DollarSign, title: 'Accounting', desc: 'Double-entry bookkeeping with automated financial reports and tax compliance', color: 'emerald' },
  { icon: ShoppingCart, title: 'Sales', desc: 'End-to-end order management from quotes to invoices and payment tracking', color: 'violet' },
  { icon: FileText, title: 'Purchasing', desc: 'Streamlined supplier management, purchase orders, and goods receipt processing', color: 'amber' },
  { icon: Users, title: 'Human Resources', desc: 'Complete HRM with attendance tracking, leave management, and payroll integration', color: 'rose' },
  { icon: BarChart3, title: 'Analytics', desc: 'Real-time dashboards and KPI tracking for data-driven business decisions', color: 'cyan' },
  { icon: Building2, title: 'Fixed Assets', desc: 'Asset lifecycle management with depreciation scheduling and maintenance tracking', color: 'indigo' },
  { icon: CheckCircle2, title: 'Quality Control', desc: 'Inspection workflows, NCR management, and CAPA tracking for compliance', color: 'teal' },
]

const modules = [
  { name: 'Foundation', items: ['Organization & Users', 'Inventory & Warehouses', 'Chart of Accounts', 'Tax Configuration'], icon: '01' },
  { name: 'Operations', items: ['Sales & Customers', 'Purchasing & Suppliers', 'Projects & Tasks', 'HRM & Payroll'], icon: '02' },
  { name: 'Excellence', items: ['Financial Reports', 'Asset Management', 'Quality Control', 'Advanced Analytics'], icon: '03' },
]

const stats = [
  { value: '500+', label: 'Enterprise Clients', icon: Globe },
  { value: '99.9%', label: 'Uptime Guarantee', icon: Shield },
  { value: '10K+', label: 'Daily Active Users', icon: Users },
  { value: '24/7', label: 'Expert Support', icon: Headphones },
]

const plans = [
  {
    name: 'Starter',
    price: '$99',
    users: 'Up to 10 users',
    features: ['Core Modules', 'Email Support', '50GB Storage', 'Basic Reports'],
    popular: false
  },
  {
    name: 'Professional',
    price: '$299',
    users: 'Up to 50 users',
    features: ['All Modules', 'Priority Support', '200GB Storage', 'API Access', 'Custom Reports'],
    popular: true
  },
  {
    name: 'Enterprise',
    price: 'Custom',
    users: 'Unlimited users',
    features: ['All Features', 'Dedicated Support', 'Unlimited Storage', 'SSO Integration', 'On-Premise Option', 'Custom Development'],
    popular: false
  },
]

const colorMap: Record<string, { bg: string, icon: string, border: string, hover: string }> = {
  blue: { bg: 'bg-blue-500/10', icon: 'text-blue-400', border: 'border-blue-500/20', hover: 'group-hover:bg-blue-500/20' },
  emerald: { bg: 'bg-emerald-500/10', icon: 'text-emerald-400', border: 'border-emerald-500/20', hover: 'group-hover:bg-emerald-500/20' },
  violet: { bg: 'bg-violet-500/10', icon: 'text-violet-400', border: 'border-violet-500/20', hover: 'group-hover:bg-violet-500/20' },
  amber: { bg: 'bg-amber-500/10', icon: 'text-amber-400', border: 'border-amber-500/20', hover: 'group-hover:bg-amber-500/20' },
  rose: { bg: 'bg-rose-500/10', icon: 'text-rose-400', border: 'border-rose-500/20', hover: 'group-hover:bg-rose-500/20' },
  cyan: { bg: 'bg-cyan-500/10', icon: 'text-cyan-400', border: 'border-cyan-500/20', hover: 'group-hover:bg-cyan-500/20' },
  indigo: { bg: 'bg-indigo-500/10', icon: 'text-indigo-400', border: 'border-indigo-500/20', hover: 'group-hover:bg-indigo-500/20' },
  teal: { bg: 'bg-teal-500/10', icon: 'text-teal-400', border: 'border-teal-500/20', hover: 'group-hover:bg-teal-500/20' },
}

function FeatureCard({ icon: Icon, title, desc, color }: typeof features[0]) {
  const colors = colorMap[color]
  return (
    <div className="group relative bg-slate-800/50 dark:bg-slate-800/50 rounded-2xl p-6 border border-slate-700/50 hover:border-slate-600 hover:shadow-xl hover:shadow-slate-900/50 transition-all duration-300 overflow-hidden backdrop-blur-sm">
      <div className={`absolute inset-0 ${colors.bg} opacity-0 group-hover:opacity-100 transition-opacity duration-300`} />
      <div className="relative z-10">
        <div className={`inline-flex items-center justify-center w-12 h-12 rounded-xl ${colors.bg} mb-4 group-hover:scale-110 transition-transform duration-300 border ${colors.border}`}>
          <Icon className={`w-6 h-6 ${colors.icon}`} />
        </div>
        <h3 className="text-lg font-semibold text-white mb-2">{title}</h3>
        <p className="text-slate-400 text-sm leading-relaxed">{desc}</p>
      </div>
    </div>
  )
}

function ModuleCard({ name, items, icon }: typeof modules[0]) {
  return (
    <div className="relative bg-slate-800/50 rounded-2xl p-8 border border-slate-700/50 hover:border-slate-600 hover:shadow-xl hover:shadow-slate-900/50 transition-all duration-300 backdrop-blur-sm">
      <div className="flex items-center gap-4 mb-6">
        <span className="text-5xl font-bold bg-gradient-to-br from-blue-400 to-blue-600 bg-clip-text text-transparent opacity-30">{icon}</span>
        <h3 className="text-xl font-semibold text-white">{name}</h3>
      </div>
      <ul className="space-y-3">
        {items.map((item) => (
          <li key={item} className="flex items-center gap-3 text-slate-400">
            <span className="w-1.5 h-1.5 rounded-full bg-blue-500" />
            {item}
          </li>
        ))}
      </ul>
    </div>
  )
}

export default function Home() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)
  const [mounted, setMounted] = useState(false)
  const { resolvedTheme, theme, setTheme } = useTheme()

  useEffect(() => {
    setMounted(true)
  }, [])

  const ThemeIcon = theme === 'light' ? Sun : theme === 'dark' ? Moon : Monitor

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900">
      {/* Header */}
      <header className="fixed top-0 left-0 right-0 z-50 bg-slate-900/80 backdrop-blur-xl border-b border-slate-800">
        <nav className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-20">
            <Link href="/" className="flex items-center gap-3">
              <div className="w-11 h-11 bg-gradient-to-br from-blue-600 to-indigo-600 rounded-xl flex items-center justify-center shadow-lg shadow-blue-600/25">
                <span className="text-white font-bold text-lg">N</span>
              </div>
              <div>
                <span className="text-2xl font-bold text-white tracking-tight">NEXTERP</span>
                <span className="hidden sm:block text-xs text-slate-500 -mt-1">Enterprise Suite</span>
              </div>
            </Link>

            <div className="hidden lg:flex items-center gap-10">
              <a href="#features" className="text-slate-400 hover:text-white transition-colors font-medium">Features</a>
              <a href="#modules" className="text-slate-400 hover:text-white transition-colors font-medium">Modules</a>
              <a href="#pricing" className="text-slate-400 hover:text-white transition-colors font-medium">Pricing</a>
              <a href="#contact" className="text-slate-400 hover:text-white transition-colors font-medium">Contact</a>
            </div>

            <div className="hidden lg:flex items-center gap-4">
              <button
                onClick={() => {
                  const next = theme === 'light' ? 'dark' : theme === 'dark' ? 'system' : 'light'
                  setTheme(next)
                }}
                className="p-2.5 rounded-xl bg-slate-800/80 border border-slate-700 hover:bg-slate-700 transition-all"
                aria-label="Toggle theme"
              >
                <ThemeIcon className="w-5 h-5 text-slate-300" />
              </button>
              <Link href="/login" className="text-slate-300 hover:text-white font-medium transition-colors">Sign In</Link>
              <Link href="/register" className="bg-gradient-to-r from-blue-600 to-indigo-600 text-white px-6 py-2.5 rounded-xl font-semibold hover:shadow-lg hover:shadow-blue-600/25 transition-all hover:-translate-y-0.5">
                Get Started
              </Link>
            </div>

            <button onClick={() => setMobileMenuOpen(!mobileMenuOpen)} className="lg:hidden p-2 text-slate-400 hover:text-white transition-colors">
              {mobileMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
            </button>
          </div>
        </nav>

        {/* Mobile Menu */}
        {mobileMenuOpen && (
          <div className="lg:hidden bg-slate-900 border-t border-slate-800 px-4 py-6 space-y-4">
            <a href="#features" className="block text-slate-400 hover:text-white font-medium">Features</a>
            <a href="#modules" className="block text-slate-400 hover:text-white font-medium">Modules</a>
            <a href="#pricing" className="block text-slate-400 hover:text-white font-medium">Pricing</a>
            <Link href="/login" className="block text-slate-400 hover:text-white font-medium">Sign In</Link>
            <Link href="/register" className="block bg-gradient-to-r from-blue-600 to-indigo-600 text-white text-center py-3 rounded-xl font-semibold">Get Started</Link>
          </div>
        )}
      </header>

      {/* Hero Section */}
      <section className="relative pt-32 pb-24 px-4 overflow-hidden">
        <div className="absolute inset-0 bg-gradient-to-b from-blue-900/20 via-transparent to-transparent" />
        <div className="absolute top-20 left-1/2 -translate-x-1/2 w-[800px] h-[800px] bg-blue-600/10 rounded-full blur-3xl" />
        <div className="absolute top-40 right-0 w-[600px] h-[600px] bg-indigo-600/10 rounded-full blur-3xl" />

        <div className="relative max-w-7xl mx-auto">
          <div className="text-center max-w-4xl mx-auto">
            <div className={`inline-flex items-center gap-2 bg-blue-500/10 border border-blue-500/20 text-blue-400 px-4 py-2 rounded-full text-sm font-medium mb-8 transition-all duration-700 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>
              <Sparkles className="w-4 h-4" />
              <span>Powered by Modern Technology Stack</span>
            </div>

            <h1 className={`text-5xl md:text-6xl lg:text-7xl font-bold text-white mb-8 leading-[1.1] tracking-tight transition-all duration-700 delay-100 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>
              The Complete ERP<br />
              <span className="bg-gradient-to-r from-blue-400 via-indigo-400 to-violet-400 bg-clip-text text-transparent">Solution</span>
            </h1>

            <p className={`text-xl md:text-2xl text-slate-400 max-w-2xl mx-auto mb-12 leading-relaxed transition-all duration-700 delay-200 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>
              Streamline your business operations with an integrated suite of powerful modules designed for modern enterprises.
            </p>

            <div className={`flex flex-col sm:flex-row gap-4 justify-center transition-all duration-700 delay-300 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>
              <Link href="/register" className="group bg-gradient-to-r from-blue-600 to-indigo-600 text-white px-8 py-4 rounded-2xl font-semibold text-lg hover:shadow-xl hover:shadow-blue-600/25 transition-all hover:-translate-y-1 flex items-center justify-center gap-2">
                Start Free Trial
                <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
              </Link>
              <Link href="/demo" className="group bg-slate-800 text-white px-8 py-4 rounded-2xl font-semibold text-lg border border-slate-700 hover:border-slate-600 hover:shadow-lg transition-all flex items-center justify-center gap-3">
                <div className="w-10 h-10 rounded-full bg-slate-700 flex items-center justify-center group-hover:bg-blue-500/20 group-hover:text-blue-400 transition-colors">
                  <Play className="w-4 h-4 ml-0.5" />
                </div>
                Watch Demo
              </Link>
            </div>

            <div className={`mt-16 flex flex-wrap items-center justify-center gap-x-10 gap-y-4 text-sm text-slate-500 transition-all duration-700 delay-400 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>
              <div className="flex items-center gap-2">
                <div className="w-5 h-5 rounded-full bg-green-500/10 border border-green-500/20 flex items-center justify-center">
                  <Shield className="w-3 h-3 text-green-400" />
                </div>
                <span>No credit card required</span>
              </div>
              <div className="flex items-center gap-2">
                <div className="w-5 h-5 rounded-full bg-green-500/10 border border-green-500/20 flex items-center justify-center">
                  <Clock className="w-3 h-3 text-green-400" />
                </div>
                <span>14-day free trial</span>
              </div>
              <div className="flex items-center gap-2">
                <div className="w-5 h-5 rounded-full bg-green-500/10 border border-green-500/20 flex items-center justify-center">
                  <Zap className="w-3 h-3 text-green-400" />
                </div>
                <span>Cancel anytime</span>
              </div>
            </div>
          </div>

          {/* Dashboard Preview */}
          <div className={`mt-20 transition-all duration-1000 delay-500 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-12'}`}>
            <div className="relative max-w-5xl mx-auto">
              <div className="absolute -inset-1 bg-gradient-to-r from-blue-600/20 via-indigo-600/20 to-violet-600/20 rounded-3xl blur-xl opacity-50" />
              <div className="relative bg-slate-800 rounded-2xl p-3 shadow-2xl border border-slate-700">
                <div className="flex items-center gap-3 px-4 py-2.5">
                  <div className="flex gap-2">
                    <div className="w-3.5 h-3.5 rounded-full bg-red-500/80" />
                    <div className="w-3.5 h-3.5 rounded-full bg-yellow-500/80" />
                    <div className="w-3.5 h-3.5 rounded-full bg-green-500/80" />
                  </div>
                  <div className="flex-1 bg-slate-900 rounded-lg px-4 py-1.5 text-slate-500 text-sm flex items-center gap-2">
                    <Globe className="w-4 h-4" />
                    <span>app.nexterp.com/dashboard</span>
                  </div>
                </div>
                <div className="bg-gradient-to-br from-slate-700 to-slate-800 rounded-xl p-6 min-h-[420px] border border-slate-700">
                  <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
                    {[
                      { label: 'Total Revenue', value: '$124,500', change: '+18.2%', positive: true },
                      { label: 'Active Orders', value: '89', change: '+12.5%', positive: true },
                      { label: 'Inventory Items', value: '1,234', change: '+5.3%', positive: true },
                      { label: 'Employees', value: '45', change: '+2.1%', positive: true },
                    ].map((item) => (
                      <div key={item.label} className="bg-slate-800/80 rounded-xl p-4 border border-slate-700 backdrop-blur-sm">
                        <div className="text-slate-500 text-xs font-medium uppercase tracking-wide">{item.label}</div>
                        <div className="text-2xl font-bold text-white mt-1">{item.value}</div>
                        <div className={`text-xs font-medium mt-1 ${item.positive ? 'text-green-400' : 'text-red-400'}`}>{item.change}</div>
                      </div>
                    ))}
                  </div>
                  <div className="bg-slate-800/80 rounded-xl h-56 border border-slate-700 backdrop-blur-sm flex items-center justify-center">
                    <div className="text-center">
                      <BarChart3 className="w-12 h-12 text-slate-600 mx-auto mb-2" />
                      <div className="text-slate-500 text-sm">Real-time Analytics Dashboard</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section id="features" className="py-24 px-4 bg-slate-900/50">
        <div className="max-w-7xl mx-auto">
          <div className="text-center mb-16">
            <span className="text-blue-400 font-semibold text-sm uppercase tracking-wider">Powerful Features</span>
            <h2 className="text-4xl md:text-5xl font-bold text-white mt-3 mb-4">Everything You Need</h2>
            <p className="text-xl text-slate-400 max-w-2xl mx-auto">
              A complete suite of integrated business modules designed to work together seamlessly
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-6">
            {features.map((feature, index) => (
              <div
                key={feature.title}
                className={`transition-all duration-500 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`}
                style={{ transitionDelay: `${index * 100}ms` }}
              >
                <FeatureCard {...feature} />
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Modules Section */}
      <section id="modules" className="py-24 px-4 bg-slate-900/50">
        <div className="max-w-7xl mx-auto">
          <div className="text-center mb-16">
            <span className="text-blue-400 font-semibold text-sm uppercase tracking-wider">Business Modules</span>
            <h2 className="text-4xl md:text-5xl font-bold text-white mt-3 mb-4">10+ Integrated Modules</h2>
            <p className="text-xl text-slate-400 max-w-2xl mx-auto">
              From core operations to enterprise-grade features, all seamlessly connected
            </p>
          </div>

          <div className="grid md:grid-cols-3 gap-8">
            {modules.map((group, index) => (
              <div
                key={group.name}
                className={`transition-all duration-500 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`}
                style={{ transitionDelay: `${index * 150}ms` }}
              >
                <ModuleCard {...group} />
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Stats Section */}
      <section className="py-24 px-4 bg-gradient-to-r from-blue-600 via-indigo-600 to-violet-600 text-white relative overflow-hidden">
        <div className="absolute inset-0 opacity-10">
          <div className="absolute inset-0" style={{ backgroundImage: 'url("data:image/svg+xml,%3Csvg width=\"60\" height=\"60\" viewBox=\"0 0 60 60\" xmlns=\"http://www.w3.org/2000/svg\"%3E%3Cg fill=\"none\" fill-rule=\"evenodd\"%3E%3Cg fill=\"%23ffffff\" fill-opacity=\"0.4\"%3E%3Cpath d=\"M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z\"/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")' }} />
        </div>
        <div className="max-w-7xl mx-auto relative">
          <div className="grid md:grid-cols-4 gap-8">
            {stats.map((stat, index) => {
              const Icon = stat.icon
              return (
                <div
                  key={stat.label}
                  className={`text-center transition-all duration-500 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`}
                  style={{ transitionDelay: `${index * 100}ms` }}
                >
                  <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-white/10 backdrop-blur-sm mb-4">
                    <Icon className="w-7 h-7" />
                  </div>
                  <div className="text-4xl md:text-5xl font-bold mb-2">{stat.value}</div>
                  <div className="text-blue-200 font-medium">{stat.label}</div>
                </div>
              )
            })}
          </div>
        </div>
      </section>

      {/* Pricing Section */}
      <section id="pricing" className="py-24 px-4 bg-slate-900/50">
        <div className="max-w-7xl mx-auto">
          <div className="text-center mb-16">
            <span className="text-blue-400 font-semibold text-sm uppercase tracking-wider">Pricing</span>
            <h2 className="text-4xl md:text-5xl font-bold text-white mt-3 mb-4">Simple, Transparent Pricing</h2>
            <p className="text-xl text-slate-400 max-w-2xl mx-auto">
              Choose the plan that fits your business needs
            </p>
          </div>

          <div className="grid md:grid-cols-3 gap-8 max-w-6xl mx-auto">
            {plans.map((plan, index) => (
              <div
                key={plan.name}
                className={`relative rounded-3xl p-8 transition-all duration-500 ${plan.popular
                  ? 'bg-gradient-to-br from-blue-600 to-indigo-700 text-white scale-105 shadow-2xl shadow-blue-600/25 border border-blue-500/30'
                  : 'bg-slate-800/50 border border-slate-700/50 hover:border-slate-600 hover:shadow-xl'}`}
                style={{ transitionDelay: `${index * 100}ms` }}
              >
                {plan.popular && (
                  <div className="absolute -top-4 left-1/2 -translate-x-1/2 bg-gradient-to-r from-amber-400 to-orange-400 text-gray-900 px-4 py-1 rounded-full text-sm font-semibold flex items-center gap-1">
                    <Star className="w-3.5 h-3.5" fill="currentColor" />
                    Most Popular
                  </div>
                )}

                <div className="text-center mb-8">
                  <h3 className={`text-xl font-semibold mb-2 ${!plan.popular && 'text-white'}`}>{plan.name}</h3>
                  <div className="text-4xl font-bold mb-1">{plan.price}</div>
                  <div className={`text-sm ${plan.popular ? 'text-blue-200' : 'text-slate-500'}`}>{plan.users}</div>
                </div>

                <ul className="space-y-4 mb-8">
                  {plan.features.map((feature) => (
                    <li key={feature} className="flex items-start gap-3">
                      <CheckCircle2 className={`w-5 h-5 mt-0.5 flex-shrink-0 ${plan.popular ? 'text-blue-200' : 'text-green-400'}`} />
                      <span className={`text-sm ${plan.popular ? 'text-blue-100' : 'text-slate-400'}`}>{feature}</span>
                    </li>
                  ))}
                </ul>

                <Link href="/register" className={`block text-center py-4 rounded-2xl font-semibold transition-all ${
                  plan.popular
                    ? 'bg-white text-blue-600 hover:bg-gray-100'
                    : 'bg-slate-700 text-white hover:bg-slate-600'
                }`}>
                  Get Started
                </Link>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section id="contact" className="py-24 px-4 bg-slate-900 text-white relative overflow-hidden">
        <div className="absolute inset-0">
          <div className="absolute top-0 left-1/4 w-96 h-96 bg-blue-600/20 rounded-full blur-3xl" />
          <div className="absolute bottom-0 right-1/4 w-96 h-96 bg-violet-600/20 rounded-full blur-3xl" />
        </div>

        <div className="max-w-4xl mx-auto text-center relative">
          <h2 className="text-4xl md:text-5xl font-bold mb-6">Ready to Transform Your Business?</h2>
          <p className="text-xl text-slate-400 mb-10 max-w-2xl mx-auto">
            Join hundreds of companies that have streamlined their operations with NEXTERP
          </p>

          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link href="/register" className="group bg-gradient-to-r from-blue-600 to-indigo-600 text-white px-8 py-4 rounded-2xl font-semibold text-lg hover:shadow-xl hover:shadow-blue-600/25 transition-all hover:-translate-y-1 flex items-center justify-center gap-2">
              Start Free Trial
              <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
            </Link>
            <Link href="/contact" className="bg-slate-800 text-white px-8 py-4 rounded-2xl font-semibold text-lg hover:bg-slate-700 transition-all border border-slate-700 flex items-center justify-center gap-2">
              Contact Sales
              <ChevronRight className="w-5 h-5" />
            </Link>
          </div>

          <div className="mt-12 text-slate-400">
            <span>Have questions? </span>
            <a href="mailto:sales@nexterp.com" className="text-blue-400 hover:text-blue-300 transition-colors font-medium">sales@nexterp.com</a>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-slate-950 py-16 px-4 border-t border-slate-800">
        <div className="max-w-7xl mx-auto">
          <div className="grid md:grid-cols-4 gap-12 mb-12">
            <div>
              <div className="flex items-center gap-3 mb-4">
                <div className="w-10 h-10 bg-gradient-to-br from-blue-600 to-indigo-600 rounded-xl flex items-center justify-center">
                  <span className="text-white font-bold">N</span>
                </div>
                <span className="text-xl font-bold text-white">NEXTERP</span>
              </div>
              <p className="text-slate-500 text-sm leading-relaxed">
                Enterprise Resource Planning for modern businesses. Streamline operations, reduce costs, and scale with confidence.
              </p>
            </div>

            <div>
              <h4 className="font-semibold text-white mb-4">Product</h4>
              <ul className="space-y-3 text-sm">
                <li><a href="#features" className="text-slate-400 hover:text-white transition-colors">Features</a></li>
                <li><a href="#pricing" className="text-slate-400 hover:text-white transition-colors">Pricing</a></li>
                <li><a href="/demo" className="text-slate-400 hover:text-white transition-colors">Demo</a></li>
                <li><a href="/changelog" className="text-slate-400 hover:text-white transition-colors">Changelog</a></li>
              </ul>
            </div>

            <div>
              <h4 className="font-semibold text-white mb-4">Company</h4>
              <ul className="space-y-3 text-sm">
                <li><a href="/about" className="text-slate-400 hover:text-white transition-colors">About Us</a></li>
                <li><a href="/blog" className="text-slate-400 hover:text-white transition-colors">Blog</a></li>
                <li><a href="/careers" className="text-slate-400 hover:text-white transition-colors">Careers</a></li>
                <li><a href="/press" className="text-slate-400 hover:text-white transition-colors">Press</a></li>
              </ul>
            </div>

            <div>
              <h4 className="font-semibold text-white mb-4">Support</h4>
              <ul className="space-y-3 text-sm">
                <li><a href="/docs" className="text-slate-400 hover:text-white transition-colors">Documentation</a></li>
                <li><a href="/help" className="text-slate-400 hover:text-white transition-colors">Help Center</a></li>
                <li><a href="/contact" className="text-slate-400 hover:text-white transition-colors">Contact</a></li>
                <li><a href="/status" className="text-slate-400 hover:text-white transition-colors">System Status</a></li>
              </ul>
            </div>
          </div>

          <div className="border-t border-slate-800 pt-8 flex flex-col md:flex-row justify-between items-center gap-4 text-slate-500 text-sm">
            <div>© 2026 NEXTERP by SeVeN-. All rights reserved.</div>
            <div className="flex items-center gap-6">
              <a href="/privacy" className="hover:text-white transition-colors">Privacy Policy</a>
              <a href="/terms" className="hover:text-white transition-colors">Terms of Service</a>
            </div>
          </div>
        </div>
      </footer>
    </div>
  )
}
