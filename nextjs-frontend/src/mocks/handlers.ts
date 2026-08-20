import { http, HttpResponse, delay } from 'msw';
import {
  mockEmployees,
  mockDepartments,
  mockStockItems,
  mockWarehouses,
  mockSuppliers,
  mockPurchaseOrders,
  mockProjects,
  mockChartOfAccounts,
  mockOrganizations,
  mockUsers,
  mockDashboardStats,
  mockActivities,
  mockRoles,
  mockPermissions,
  mockModules,
} from '@/lib/mockData';

const BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

function paginatedResponse<T>(items: T[], page = 1, pageSize = 10) {
  const start = (page - 1) * pageSize;
  const paged = items.slice(start, start + pageSize);
  return HttpResponse.json({
    success: true,
    data: {
      items: paged,
      totalCount: items.length,
      page,
      pageSize,
      totalPages: Math.ceil(items.length / pageSize),
    },
  });
}

export const handlers = [
  // Dashboard
  http.get(`${BASE_URL}/api/dashboard/stats`, async () => {
    await delay(300);
    return HttpResponse.json({ success: true, data: mockDashboardStats });
  }),

  // Employees
  http.get(`${BASE_URL}/api/employees`, async ({ request }) => {
    await delay(400);
    const url = new URL(request.url);
    const page = Number(url.searchParams.get('page')) || 1;
    const pageSize = Number(url.searchParams.get('pageSize')) || 10;
    const search = url.searchParams.get('search')?.toLowerCase() || '';

    let filtered = mockEmployees;
    if (search) {
      filtered = mockEmployees.filter(
        (e) =>
          e.firstName.toLowerCase().includes(search) ||
          e.lastName.toLowerCase().includes(search) ||
          e.email.toLowerCase().includes(search)
      );
    }

    return paginatedResponse(filtered, page, pageSize);
  }),

  http.post(`${BASE_URL}/api/employees`, async ({ request }) => {
    await delay(500);
    const body = await request.json() as Record<string, unknown>;
    const newEmployee = {
      id: String(mockEmployees.length + 1),
      ...body,
      employeeNumber: `EMP-${String(mockEmployees.length + 1).padStart(3, '0')}`,
      isActive: true,
    };
    return HttpResponse.json({ success: true, data: newEmployee }, { status: 201 });
  }),

  http.put(`${BASE_URL}/api/employees/:id`, async ({ params }) => {
    await delay(400);
    const { id } = params;
    const employee = mockEmployees.find((e) => e.id === id);
    if (!employee) {
      return HttpResponse.json({ success: false, error: 'Employee not found' }, { status: 404 });
    }
    return HttpResponse.json({ success: true, data: { ...employee } });
  }),

  http.delete(`${BASE_URL}/api/employees/:id`, async ({ params }) => {
    await delay(300);
    const { id } = params;
    if (!mockEmployees.find((e) => e.id === id)) {
      return HttpResponse.json({ success: false, error: 'Employee not found' }, { status: 404 });
    }
    return HttpResponse.json({ success: true });
  }),

  // Departments
  http.get(`${BASE_URL}/api/departments`, async ({ request }) => {
    await delay(300);
    const url = new URL(request.url);
    const pageSize = Number(url.searchParams.get('pageSize')) || 100;
    return paginatedResponse(mockDepartments, 1, pageSize);
  }),

  // Inventory / Stock Items
  http.get(`${BASE_URL}/api/stock-items`, async ({ request }) => {
    await delay(400);
    const url = new URL(request.url);
    const page = Number(url.searchParams.get('page')) || 1;
    const pageSize = Number(url.searchParams.get('pageSize')) || 10;
    const search = url.searchParams.get('search')?.toLowerCase() || '';

    let filtered = mockStockItems;
    if (search) {
      filtered = mockStockItems.filter(
        (i) => i.name.toLowerCase().includes(search) || i.code.toLowerCase().includes(search)
      );
    }

    return paginatedResponse(filtered, page, pageSize);
  }),

  http.post(`${BASE_URL}/api/stock-items`, async ({ request }) => {
    await delay(500);
    const body = await request.json() as Record<string, unknown>;
    const newItem = { id: String(mockStockItems.length + 1), ...body, isActive: true };
    return HttpResponse.json({ success: true, data: newItem }, { status: 201 });
  }),

  http.put(`${BASE_URL}/api/stock-items/:id`, async ({ params }) => {
    await delay(400);
    const { id } = params;
    const item = mockStockItems.find((i) => i.id === id);
    if (!item) {
      return HttpResponse.json({ success: false, error: 'Item not found' }, { status: 404 });
    }
    return HttpResponse.json({ success: true, data: { ...item } });
  }),

  http.delete(`${BASE_URL}/api/stock-items/:id`, async ({ params }) => {
    await delay(300);
    const { id } = params;
    if (!mockStockItems.find((i) => i.id === id)) {
      return HttpResponse.json({ success: false, error: 'Item not found' }, { status: 404 });
    }
    return HttpResponse.json({ success: true });
  }),

  // Warehouses
  http.get(`${BASE_URL}/api/warehouses`, async ({ request }) => {
    await delay(300);
    const url = new URL(request.url);
    const pageSize = Number(url.searchParams.get('pageSize')) || 100;
    return paginatedResponse(mockWarehouses, 1, pageSize);
  }),

  // Suppliers
  http.get(`${BASE_URL}/api/suppliers`, async ({ request }) => {
    await delay(400);
    const url = new URL(request.url);
    const page = Number(url.searchParams.get('page')) || 1;
    const pageSize = Number(url.searchParams.get('pageSize')) || 10;
    return paginatedResponse(mockSuppliers, page, pageSize);
  }),

  // Purchase Orders
  http.get(`${BASE_URL}/api/purchase-orders`, async ({ request }) => {
    await delay(400);
    const url = new URL(request.url);
    const page = Number(url.searchParams.get('page')) || 1;
    const pageSize = Number(url.searchParams.get('pageSize')) || 10;
    return paginatedResponse(mockPurchaseOrders, page, pageSize);
  }),

  // Projects
  http.get(`${BASE_URL}/api/projects`, async ({ request }) => {
    await delay(400);
    const url = new URL(request.url);
    const page = Number(url.searchParams.get('page')) || 1;
    const pageSize = Number(url.searchParams.get('pageSize')) || 10;
    return paginatedResponse(mockProjects, page, pageSize);
  }),

  // Chart of Accounts
  http.get(`${BASE_URL}/api/accounts`, async ({ request }) => {
    await delay(400);
    const url = new URL(request.url);
    const page = Number(url.searchParams.get('page')) || 1;
    const pageSize = Number(url.searchParams.get('pageSize')) || 10;
    return paginatedResponse(mockChartOfAccounts, page, pageSize);
  }),

  // Organizations
  http.get(`${BASE_URL}/api/organizations`, async () => {
    await delay(300);
    return HttpResponse.json({ success: true, data: { items: mockOrganizations, totalCount: mockOrganizations.length } });
  }),

  // Users
  http.get(`${BASE_URL}/api/users`, async ({ request }) => {
    await delay(400);
    const url = new URL(request.url);
    const page = Number(url.searchParams.get('page')) || 1;
    const pageSize = Number(url.searchParams.get('pageSize')) || 10;
    return paginatedResponse(mockUsers, page, pageSize);
  }),

  // Roles
  http.get(`${BASE_URL}/api/roles`, async () => {
    await delay(300);
    return HttpResponse.json({ success: true, data: { items: mockRoles, totalCount: mockRoles.length } });
  }),

  // Permissions
  http.get(`${BASE_URL}/api/permissions`, async () => {
    await delay(300);
    return HttpResponse.json({ success: true, data: { items: mockPermissions, totalCount: mockPermissions.length } });
  }),

  // Modules
  http.get(`${BASE_URL}/api/modules`, async () => {
    await delay(300);
    return HttpResponse.json({ success: true, data: { items: mockModules, totalCount: mockModules.length } });
  }),

  // Activities
  http.get(`${BASE_URL}/api/activities`, async () => {
    await delay(300);
    return HttpResponse.json({ success: true, data: mockActivities });
  }),

  // Auth - Login
  http.post(`${BASE_URL}/api/auth/login`, async ({ request }) => {
    await delay(600);
    const body = await request.json() as { username?: string; password?: string };
    if (body.username === 'admin' && body.password === 'DevPassword2024!') {
      return HttpResponse.json({
        success: true,
        data: {
          token: 'mock-jwt-token',
          user: {
            id: '1',
            username: 'admin',
            email: 'admin@nexterp.com',
            firstName: 'System',
            lastName: 'Administrator',
            fullName: 'System Administrator',
            organizationId: '1',
            isActive: true,
            isSuperAdmin: true,
            roles: ['SuperAdmin'],
          },
        },
      });
    }
    return HttpResponse.json({ success: false, error: 'Invalid credentials' }, { status: 401 });
  }),

  // Auth - Refresh
  http.post(`${BASE_URL}/api/auth/refresh`, async () => {
    await delay(200);
    return HttpResponse.json({
      success: true,
      data: { token: 'mock-refreshed-token' },
    });
  }),

  // Health
  http.get(`${BASE_URL}/health`, async () => {
    return HttpResponse.json({ status: 'healthy', timestamp: new Date().toISOString() });
  }),

  http.get(`${BASE_URL}/health/ready`, async () => {
    return HttpResponse.json({ status: 'ready', checks: { database: 'ok', cache: 'ok' } });
  }),
];
