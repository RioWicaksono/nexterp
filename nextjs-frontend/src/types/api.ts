/**
 * Type definitions for API responses
 */

export interface ApiResponse<T = unknown> {
  success: boolean;
  data?: T;
  error?: string;
  message?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiError {
  code: string;
  message: string;
  details?: Record<string, string[]>;
}

export interface ApiResponse<T = unknown> {
  success: boolean;
  data?: T;
  error?: string;
  message?: string;
}

export interface PaginationParams {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface EmployeeDto {
  id: string;
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  department?: string;
  employeeNumber?: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface DepartmentDto {
  id: string;
  name: string;
  code: string;
  description?: string;
  employeeCount?: number;
}

export interface StockItemDto {
  id: string;
  code: string;
  name: string;
  barcode?: string;
  standardCost?: number;
  standardPrice?: number;
  reorderLevel?: number;
  isActive: boolean;
}

export interface WarehouseDto {
  id: string;
  name: string;
  code: string;
  address?: string;
  isActive: boolean;
}

export interface SupplierDto {
  id: string;
  name: string;
  code: string;
  email?: string;
  phone?: string;
  contactPerson?: string;
  isActive: boolean;
}

export interface PurchaseOrderDto {
  id: string;
  orderNumber: string;
  supplier?: string;
  orderDate?: string;
  expectedDeliveryDate?: string;
  status: 'Draft' | 'Submitted' | 'Approved' | 'Cancelled';
  totalAmount?: number;
  itemCount?: number;
}

export interface ProjectDto {
  id: string;
  name: string;
  code: string;
  client?: string;
  startDate?: string;
  endDate?: string;
  status: 'Planning' | 'In Progress' | 'Completed';
  budget?: number;
  spent?: number;
}

export interface AccountDto {
  id: string;
  accountNumber: string;
  name: string;
  type: string;
  balance?: number;
}

export interface JournalEntryDto {
  id: string;
  entryNumber: string;
  entryDate: string;
  description: string;
  debitTotal: number;
  creditTotal: number;
  status: 'Draft' | 'Posted';
}

export interface RoleDto {
  id: string;
  name: string;
  description?: string;
  permissions: string[];
  userCount?: number;
}

export interface PermissionDto {
  id: string;
  name: string;
  description?: string;
  category?: string;
}

export interface ModuleDto {
  id: string;
  name: string;
  code: string;
  description?: string;
  isEnabled: boolean;
  icon?: string;
}

export interface NotificationDto {
  id: string;
  type: 'info' | 'success' | 'warning' | 'error';
  title: string;
  message?: string;
  read: boolean;
  createdAt: string;
  link?: string;
}

export interface AuditLogDto {
  id: string;
  timestamp: string;
  userId: string;
  userName: string;
  action: 'create' | 'update' | 'delete' | 'view';
  entityType: string;
  entityId: string;
  entityName: string;
  changes?: Array<{ field: string; oldValue: string; newValue: string }>;
  ipAddress?: string;
}

export interface ActivityDto {
  id: string;
  userId: string;
  userName: string;
  action: string;
  entityType: string;
  entityId: string;
  entityName: string;
  details?: string;
  timestamp: string;
}

export interface DashboardStatsDto {
  totalEmployees: number;
  totalInventoryItems: number;
  totalPurchaseOrders: number;
  totalSuppliers: number;
  totalProjects: number;
  totalAccounts: number;
}
