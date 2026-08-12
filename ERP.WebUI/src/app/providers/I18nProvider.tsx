"use client";

import { createContext, useContext, useState, useCallback, ReactNode } from "react";

export type Locale = "en" | "id";
export type TranslationKey = string;

interface Translations {
  [key: string]: string | Translations;
}

const translations: Record<Locale, Translations> = {
  en: {
    // Common
    common: {
      save: "Save",
      cancel: "Cancel",
      delete: "Delete",
      edit: "Edit",
      add: "Add",
      search: "Search",
      filter: "Filter",
      export: "Export",
      import: "Import",
      loading: "Loading...",
      noData: "No data available",
      confirm: "Confirm",
      back: "Back",
      next: "Next",
      submit: "Submit",
      close: "Close",
    },
    // Navigation
    nav: {
      dashboard: "Dashboard",
      inventory: "Inventory",
      sales: "Sales",
      purchases: "Purchases",
      accounting: "Accounting",
      hr: "HR",
      projects: "Projects",
      analytics: "Analytics",
      assets: "Assets",
      quality: "Quality",
      settings: "Settings",
    },
    // Auth
    auth: {
      login: "Sign In",
      register: "Sign Up",
      logout: "Sign Out",
      email: "Email",
      password: "Password",
      forgotPassword: "Forgot Password?",
      rememberMe: "Remember me",
      createAccount: "Create Account",
      alreadyHaveAccount: "Already have an account?",
      dontHaveAccount: "Don't have an account?",
    },
    // Dashboard
    dashboard: {
      title: "Dashboard",
      welcome: "Welcome back",
      revenue: "Revenue",
      orders: "Orders",
      customers: "Customers",
      products: "Products",
      lowStock: "Low Stock",
      pendingTasks: "Pending Tasks",
      recentOrders: "Recent Orders",
      topProducts: "Top Products",
      salesOverview: "Sales Overview",
      inventory: "Inventory Status",
    },
    // Inventory
    inventory: {
      title: "Inventory",
      addProduct: "Add Product",
      productName: "Product Name",
      sku: "SKU",
      quantity: "Quantity",
      unitPrice: "Unit Price",
      reorderLevel: "Reorder Level",
      warehouse: "Warehouse",
      category: "Category",
      stock: "Stock",
      inStock: "In Stock",
      outOfStock: "Out of Stock",
      lowStock: "Low Stock",
    },
    // Sales
    sales: {
      title: "Sales",
      newOrder: "New Order",
      orderId: "Order ID",
      customer: "Customer",
      date: "Date",
      status: "Status",
      total: "Total",
      subtotal: "Subtotal",
      tax: "Tax",
      discount: "Discount",
      pending: "Pending",
      processing: "Processing",
      completed: "Completed",
      cancelled: "Cancelled",
    },
    // Errors
    errors: {
      required: "This field is required",
      invalidEmail: "Invalid email address",
      minLength: "Minimum {min} characters required",
      maxLength: "Maximum {max} characters allowed",
      passwordMatch: "Passwords do not match",
      networkError: "Network error. Please try again.",
      serverError: "Server error. Please contact support.",
      unauthorized: "Please sign in to continue.",
    },
    // Success
    success: {
      saved: "Saved successfully",
      deleted: "Deleted successfully",
      created: "Created successfully",
      updated: "Updated successfully",
    },
  },
  id: {
    // Common
    common: {
      save: "Simpan",
      cancel: "Batal",
      delete: "Hapus",
      edit: "Ubah",
      add: "Tambah",
      search: "Cari",
      filter: "Filter",
      export: "Ekspor",
      import: "Impor",
      loading: "Memuat...",
      noData: "Tidak ada data",
      confirm: "Konfirmasi",
      back: "Kembali",
      next: "Berikutnya",
      submit: "Kirim",
      close: "Tutup",
    },
    // Navigation
    nav: {
      dashboard: "Dasbor",
      inventory: "Inventori",
      sales: "Penjualan",
      purchases: "Pembelian",
      accounting: "Akuntansi",
      hr: "SDM",
      projects: "Proyek",
      analytics: "Analitik",
      assets: "Aset",
      quality: "Kualitas",
      settings: "Pengaturan",
    },
    // Auth
    auth: {
      login: "Masuk",
      register: "Daftar",
      logout: "Keluar",
      email: "Email",
      password: "Kata Sandi",
      forgotPassword: "Lupa Kata Sandi?",
      rememberMe: "Ingat saya",
      createAccount: "Buat Akun",
      alreadyHaveAccount: "Sudah punya akun?",
      dontHaveAccount: "Belum punya akun?",
    },
    // Dashboard
    dashboard: {
      title: "Dasbor",
      welcome: "Selamat datang",
      revenue: "Pendapatan",
      orders: "Pesanan",
      customers: "Pelanggan",
      products: "Produk",
      lowStock: "Stok Rendah",
      pendingTasks: "Tugas Tertunda",
      recentOrders: "Pesanan Terbaru",
      topProducts: "Produk Terlaris",
      salesOverview: "Ringkasan Penjualan",
      inventory: "Status Inventori",
    },
    // Inventory
    inventory: {
      title: "Inventori",
      addProduct: "Tambah Produk",
      productName: "Nama Produk",
      sku: "SKU",
      quantity: "Jumlah",
      unitPrice: "Harga Satuan",
      reorderLevel: "Level Pemesanan Ulang",
      warehouse: "Gudang",
      category: "Kategori",
      stock: "Stok",
      inStock: "Tersedia",
      outOfStock: "Habis",
      lowStock: "Stok Rendah",
    },
    // Sales
    sales: {
      title: "Penjualan",
      newOrder: "Pesanan Baru",
      orderId: "ID Pesanan",
      customer: "Pelanggan",
      date: "Tanggal",
      status: "Status",
      total: "Total",
      subtotal: "Subtotal",
      tax: "Pajak",
      discount: "Diskon",
      pending: "Tertunda",
      processing: "Diproses",
      completed: "Selesai",
      cancelled: "Dibatalkan",
    },
    // Errors
    errors: {
      required: "Kolom ini wajib diisi",
      invalidEmail: "Alamat email tidak valid",
      minLength: "Minimal {min} karakter diperlukan",
      maxLength: "Maksimal {max} karakter diizinkan",
      passwordMatch: "Kata sandi tidak cocok",
      networkError: "Kesalahan jaringan. Silakan coba lagi.",
      serverError: "Kesalahan server. Hubungi dukungan.",
      unauthorized: "Silakan masuk untuk melanjutkan.",
    },
    // Success
    success: {
      saved: "Berhasil disimpan",
      deleted: "Berhasil dihapus",
      created: "Berhasil dibuat",
      updated: "Berhasil diperbarui",
    },
  },
};

interface I18nContextType {
  locale: Locale;
  setLocale: (locale: Locale) => void;
  t: (key: string, params?: Record<string, string | number>) => string;
  translations: Translations;
}

const I18nContext = createContext<I18nContextType | undefined>(undefined);

export function I18nProvider({ children }: { children: ReactNode }) {
  const [locale, setLocale] = useState<Locale>(() => {
    if (typeof window !== "undefined") {
      const stored = localStorage.getItem("nexterp-locale") as Locale;
      if (stored && (stored === "en" || stored === "id")) return stored;
    }
    return "en";
  });

  const handleSetLocale = useCallback((newLocale: Locale) => {
    setLocale(newLocale);
    localStorage.setItem("nexterp-locale", newLocale);
    document.documentElement.lang = newLocale;
  }, []);

  const t = useCallback(
    (key: string, params?: Record<string, string | number>): string => {
      const keys = key.split(".");
      let value: string | Translations | undefined = translations[locale];

      for (const k of keys) {
        if (value && typeof value === "object") {
          value = value[k];
        } else {
          value = undefined;
          break;
        }
      }

      if (typeof value !== "string") {
        console.warn(`Translation missing for key: ${key}`);
        return key;
      }

      if (params) {
        return Object.entries(params).reduce(
          (str, [k, v]) => str.replace(new RegExp(`\\{${k}\\}`, "g"), String(v)),
          value
        );
      }

      return value;
    },
    [locale]
  );

  return (
    <I18nContext.Provider
      value={{
        locale,
        setLocale: handleSetLocale,
        t,
        translations: translations[locale],
      }}
    >
      {children}
    </I18nContext.Provider>
  );
}

export function useI18n() {
  const context = useContext(I18nContext);
  if (!context) {
    throw new Error("useI18n must be used within I18nProvider");
  }
  return context;
}
