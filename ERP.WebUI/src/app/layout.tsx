import type { Metadata, Viewport } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import { Providers } from "./providers-wrapper";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  metadataBase: new URL(
    process.env.NEXT_PUBLIC_APP_URL || "http://localhost:3000"
  ),
  title: {
    default: "NEXTERP - Enterprise Resource Planning System",
    template: "%s | NEXTERP",
  },
  description: "Streamline your business operations with NEXTERP - the all-in-one ERP solution for modern enterprises.",
  keywords: ["ERP", "Enterprise Resource Planning", "Inventory Management", "Accounting", "HRM", "CRM", "Business Software"],
  authors: [{ name: "SeVeN-" }],
  creator: "SeVeN-",
  publisher: "SeVeN-",
  robots: {
    index: true,
    follow: true,
    googleBot: {
      index: true,
      follow: true,
      "max-video-preview": -1,
      "max-image-preview": "large",
      "max-snippet": -1,
    },
  },
  openGraph: {
    type: "website",
    locale: "en_US",
    alternateLocale: "id_ID",
    siteName: "NEXTERP",
    title: "NEXTERP - Enterprise Resource Planning System",
    description: "Transform your business with powerful, intuitive ERP software",
    images: [
      {
        url: "/og-image.png",
        width: 1200,
        height: 630,
        alt: "NEXTERP Dashboard",
      },
    ],
  },
  twitter: {
    card: "summary_large_image",
    title: "NEXTERP - Enterprise Resource Planning System",
    description: "Transform your business with powerful, intuitive ERP software",
    images: ["/og-image.png"],
  },
  icons: {
    icon: "/favicon.svg",
    apple: "/apple-touch-icon.png",
    shortcut: "/favicon.svg",
  },
  manifest: "/manifest.json",
  appleWebApp: {
    capable: true,
    statusBarStyle: "default",
    title: "NEXTERP",
  },
};

export const viewport: Viewport = {
  themeColor: [
    { media: "(prefers-color-scheme: light)", color: "#ffffff" },
    { media: "(prefers-color-scheme: dark)", color: "#0f172a" },
  ],
  width: "device-width",
  initialScale: 1,
  maximumScale: 5,
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className={`${inter.className} min-h-screen flex flex-col antialiased`}>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
