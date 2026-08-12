import '@testing-library/jest-dom';

// Mock next/navigation
jest.mock('next/navigation', () => ({
  useRouter: () => ({
    push: jest.fn(),
    replace: jest.fn(),
    prefetch: jest.fn(),
    back: jest.fn(),
    forward: jest.fn(),
  }),
  usePathname: () => '/',
  useSearchParams: () => new URLSearchParams(),
}));

// Mock next/image
jest.mock('next/image', () => ({
  __esModule: true,
  default: (props) => {
    return <img {...props} />;
  },
}));

// Mock lucide-react icons
jest.mock('lucide-react', () => {
  const icons = {};
  const LucideIcon = (props) => <span data-testid={props['data-testid'] || 'icon'} {...props} />;

  return {
    ...LucideIcon,
    Package: LucideIcon,
    Users: LucideIcon,
    ShoppingCart: LucideIcon,
    DollarSign: LucideIcon,
    TrendingUp: LucideIcon,
    TrendingDown: LucideIcon,
    ArrowUpRight: LucideIcon,
    ArrowDownRight: LucideIcon,
    Bell: LucideIcon,
    Search: LucideIcon,
    Globe: LucideIcon,
    Download: LucideIcon,
    Filter: LucideIcon,
    Menu: LucideIcon,
    Settings: LucideIcon,
    LayoutDashboard: LucideIcon,
    FileText: LucideIcon,
    Sun: LucideIcon,
    Moon: LucideIcon,
    Monitor: LucideIcon,
    Loader2: LucideIcon,
    Check: LucideIcon,
    X: LucideIcon,
    Eye: LucideIcon,
    EyeOff: LucideIcon,
    AlertCircle: LucideIcon,
    AlertTriangle: LucideIcon,
    Info: LucideIcon,
    CheckCircle: LucideIcon,
    RefreshCw: LucideIcon,
    WifiOff: LucideIcon,
    Wifi: LucideIcon,
    Home: LucideIcon,
    MoreHorizontal: LucideIcon,
    FileSpreadsheet: LucideIcon,
  };
});

// Mock localStorage
const localStorageMock = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
  clear: jest.fn(),
};
global.localStorage = localStorageMock;

// Mock window.matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: jest.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: jest.fn(),
    removeListener: jest.fn(),
    addEventListener: jest.fn(),
    removeEventListener: jest.fn(),
    dispatchEvent: jest.fn(),
  })),
});

// Mock Service Worker
Object.defineProperty(navigator, 'serviceWorker', {
  writable: true,
  value: {
    register: jest.fn().mockResolvedValue({}),
    ready: Promise.resolve({
      update: jest.fn(),
      installing: null,
      waiting: null,
    }),
    addEventListener: jest.fn(),
    removeEventListener: jest.fn(),
    controller: null,
  },
});

// Mock beforeinstallprompt event
const mockBeforeInstallPromptEvent = {
  prompt: jest.fn(),
  userChoice: Promise.resolve({ outcome: 'dismissed' }),
  preventDefault: jest.fn(),
};

global.window.addEventListener = global.window.addEventListener || jest.fn();
global.dispatchEvent = global.dispatchEvent || jest.fn();
