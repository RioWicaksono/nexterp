import '@testing-library/jest-dom';

// Mock next/navigation
jest.mock('next/navigation', () => ({
  useRouter: () => ({
    push: jest.fn(),
    replace: jest.fn(),
    refresh: jest.fn(),
    back: jest.fn(),
    forward: jest.fn(),
  }),
  usePathname: () => '/dashboard',
}));

// Mock lucide-react
jest.mock('lucide-react', () => {
  const icons = {
    Search: 'SearchIcon',
    Plus: 'PlusIcon',
    Edit2: 'EditIcon',
    Trash2: 'TrashIcon',
    Check: 'CheckIcon',
    X: 'XIcon',
    ChevronDown: 'ChevronDownIcon',
    ChevronLeft: 'ChevronLeftIcon',
    ChevronRight: 'ChevronRightIcon',
    Menu: 'MenuIcon',
    Settings: 'SettingsIcon',
    Users: 'UsersIcon',
    Package: 'PackageIcon',
    ShoppingCart: 'ShoppingCartIcon',
    DollarSign: 'DollarSignIcon',
    Briefcase: 'BriefcaseIcon',
    Layers: 'LayersIcon',
    Shield: 'ShieldIcon',
    Key: 'KeyIcon',
    Building2: 'Building2Icon',
    LogOut: 'LogOutIcon',
    Clock: 'ClockIcon',
    FileText: 'FileTextIcon',
    LogIn: 'LogInIcon',
    CheckCircle: 'CheckCircleIcon',
    XCircle: 'XCircleIcon',
    CheckSquare: 'CheckSquareIcon',
    Square: 'SquareIcon',
    Download: 'DownloadIcon',
    Filter: 'FilterIcon',
    Eye: 'EyeIcon',
    AlertCircle: 'AlertCircleIcon',
    AlertTriangle: 'AlertTriangleIcon',
    Info: 'InfoIcon',
    Bell: 'BellIcon',
    Sun: 'SunIcon',
    Moon: 'MoonIcon',
    Monitor: 'MonitorIcon',
    LayoutDashboard: 'LayoutDashboardIcon',
    Save: 'SaveIcon',
    Loader2: 'Loader2Icon',
    ArrowUpRight: 'ArrowUpRightIcon',
    RefreshCw: 'RefreshCwIcon',
    Activity: 'ActivityIcon',
    User: 'UserIcon',
    GripVertical: 'GripVerticalIcon',
    Maximize2: 'Maximize2Icon',
    Minimize2: 'Minimize2Icon',
    Lock: 'LockIcon',
    Unlock: 'UnlockIcon',
    RotateCcw: 'RotateCcwIcon',
    Cloud: 'CloudIcon',
    CheckCheck: 'CheckCheckIcon',
    PlusCircle: 'PlusCircleIcon',
  };

  return Object.keys(icons).reduce((acc, name) => {
    acc[name] = icons[name];
    return acc;
  }, { __esModule: true, default: () => null });
});

// Mock localStorage
const localStorageMock = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
  clear: jest.fn(),
};
Object.defineProperty(global, 'localStorage', { value: localStorageMock });

// Mock sessionStorage
const sessionStorageMock = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
  clear: jest.fn(),
};
Object.defineProperty(global, 'sessionStorage', { value: sessionStorageMock });

// Mock URLSearchParams
global.URLSearchParams = URLSearchParams;

// Mock IntersectionObserver
class MockIntersectionObserver {
  root = null;
  rootMargin = '';
  thresholds = [];
  disconnect = jest.fn();
  observe = jest.fn();
  takeRecords = () => [];
  unobserve = jest.fn();
}
Object.defineProperty(window, 'IntersectionObserver', {
  value: MockIntersectionObserver,
});

// Mock ResizeObserver
class MockResizeObserver {
  disconnect = jest.fn();
  observe = jest.fn();
  unobserve = jest.fn();
  reportAllChanges = false;
}
Object.defineProperty(window, 'ResizeObserver', {
  value: MockResizeObserver,
});

// Clear mocks before each test
beforeEach(() => {
  jest.clearAllMocks();
  localStorageMock.getItem.mockReturnValue(null);
  localStorageMock.setItem.mockReturnValue(undefined);
  localStorageMock.removeItem.mockReturnValue(undefined);
  sessionStorageMock.getItem.mockReturnValue(null);
  sessionStorageMock.setItem.mockReturnValue(undefined);
  sessionStorageMock.removeItem.mockReturnValue(undefined);
});
