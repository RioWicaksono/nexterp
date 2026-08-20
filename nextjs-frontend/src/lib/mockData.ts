/**
 * Mock data for development and testing
 */

export const mockEmployees = [
  { id: '1', firstName: 'Sarah', lastName: 'Chen', email: 'sarah.chen@company.com', phone: '+1-555-0101', department: 'Engineering', employeeNumber: 'EMP-001', isActive: true },
  { id: '2', firstName: 'James', lastName: 'Wilson', email: 'james.wilson@company.com', phone: '+1-555-0102', department: 'Marketing', employeeNumber: 'EMP-002', isActive: true },
  { id: '3', firstName: 'Maria', lastName: 'Garcia', email: 'maria.garcia@company.com', phone: '+1-555-0103', department: 'Finance', employeeNumber: 'EMP-003', isActive: true },
  { id: '4', firstName: 'David', lastName: 'Kim', email: 'david.kim@company.com', phone: '+1-555-0104', department: 'Engineering', employeeNumber: 'EMP-004', isActive: true },
  { id: '5', firstName: 'Emily', lastName: 'Johnson', email: 'emily.johnson@company.com', phone: '+1-555-0105', department: 'HR', employeeNumber: 'EMP-005', isActive: false },
];

export const mockDepartments = [
  { id: '1', name: 'Engineering', code: 'ENG', description: 'Software Development', employeeCount: 45 },
  { id: '2', name: 'Marketing', code: 'MKT', description: 'Marketing & Sales', employeeCount: 12 },
  { id: '3', name: 'Finance', code: 'FIN', description: 'Finance & Accounting', employeeCount: 8 },
  { id: '4', name: 'HR', code: 'HR', description: 'Human Resources', employeeCount: 5 },
  { id: '5', name: 'Operations', code: 'OPS', description: 'Operations & Logistics', employeeCount: 15 },
];

export const mockStockItems = [
  { id: '1', code: 'ITM-001', name: 'Laptop Dell XPS 15', barcode: '8901234567890', standardCost: 1299.99, standardPrice: 1799.99, reorderLevel: 10, isActive: true },
  { id: '2', code: 'ITM-002', name: 'Monitor LG 27"', barcode: '8901234567891', standardCost: 299.99, standardPrice: 449.99, reorderLevel: 25, isActive: true },
  { id: '3', code: 'ITM-003', name: 'Keyboard Mechanical', barcode: '8901234567892', standardCost: 89.99, standardPrice: 149.99, reorderLevel: 50, isActive: true },
  { id: '4', code: 'ITM-004', name: 'USB-C Hub', barcode: '8901234567893', standardCost: 29.99, standardPrice: 59.99, reorderLevel: 100, isActive: true },
  { id: '5', code: 'ITM-005', name: 'Webcam HD 1080p', barcode: '8901234567894', standardCost: 49.99, standardPrice: 89.99, reorderLevel: 30, isActive: false },
];

export const mockWarehouses = [
  { id: '1', name: 'Main Warehouse', code: 'WH-001', address: '123 Industrial Ave, City', isActive: true },
  { id: '2', name: 'Distribution Center', code: 'WH-002', address: '456 Logistics Blvd, Town', isActive: true },
  { id: '3', name: 'Backup Storage', code: 'WH-003', address: '789 Storage Lane, Village', isActive: false },
];

export const mockSuppliers = [
  { id: '1', name: 'TechParts Inc.', code: 'SUP-001', email: 'orders@techparts.com', phone: '+1-555-1001', contactPerson: 'John Smith', isActive: true },
  { id: '2', name: 'Global Supply Co.', code: 'SUP-002', email: 'sales@globalsupply.com', phone: '+1-555-1002', contactPerson: 'Lisa Wong', isActive: true },
  { id: '3', name: 'Office Essentials', code: 'SUP-003', email: 'support@officeess.com', phone: '+1-555-1003', contactPerson: 'Mike Brown', isActive: true },
  { id: '4', name: 'LogiTech Solutions', code: 'SUP-004', email: 'info@logitechsol.com', phone: '+1-555-1004', contactPerson: 'Anna Lee', isActive: false },
];

export const mockPurchaseOrders = [
  { id: '1', orderNumber: 'PO-2024-001', supplier: 'TechParts Inc.', orderDate: '2024-01-15', expectedDate: '2024-01-25', status: 'Approved', totalAmount: 2599.98, itemCount: 2 },
  { id: '2', orderNumber: 'PO-2024-002', supplier: 'Global Supply Co.', orderDate: '2024-01-18', expectedDate: '2024-01-28', status: 'Pending', totalAmount: 899.97, itemCount: 3 },
  { id: '3', orderNumber: 'PO-2024-003', supplier: 'Office Essentials', orderDate: '2024-01-20', expectedDate: '2024-01-30', status: 'Draft', totalAmount: 449.95, itemCount: 5 },
];

export const mockProjects = [
  { id: '1', name: 'ERP Implementation', code: 'PRJ-001', client: 'Acme Corp', startDate: '2024-01-01', endDate: '2024-06-30', status: 'In Progress', budget: 150000, spent: 45000 },
  { id: '2', name: 'Website Redesign', code: 'PRJ-002', client: 'TechStart Inc', startDate: '2024-02-01', endDate: '2024-04-30', status: 'Planning', budget: 25000, spent: 0 },
  { id: '3', name: 'Mobile App Development', code: 'PRJ-003', client: 'RetailPlus', startDate: '2023-10-01', endDate: '2024-03-31', status: 'In Progress', budget: 80000, spent: 65000 },
];

export const mockChartOfAccounts = [
  { id: '1', accountNumber: '1000', name: 'Cash', type: 'Asset', balance: 125000.00 },
  { id: '2', accountNumber: '1100', name: 'Accounts Receivable', type: 'Asset', balance: 45000.00 },
  { id: '3', accountNumber: '1200', name: 'Inventory', type: 'Asset', balance: 85000.00 },
  { id: '4', accountNumber: '2000', name: 'Accounts Payable', type: 'Liability', balance: 32000.00 },
  { id: '5', accountNumber: '3000', name: 'Common Stock', type: 'Equity', balance: 100000.00 },
  { id: '6', accountNumber: '4000', name: 'Sales Revenue', type: 'Revenue', balance: 250000.00 },
  { id: '7', accountNumber: '5000', name: 'Cost of Goods Sold', type: 'Expense', balance: 150000.00 },
  { id: '8', accountNumber: '5100', name: 'Salaries Expense', type: 'Expense', balance: 75000.00 },
];

export const mockOrganizations = [
  { id: '1', name: 'Nexterp Solutions', code: 'NXT', taxId: '12-3456789', address: '100 Business Park, Suite 500', phone: '+1-555-5000', isActive: true },
];

export const mockUsers = [
  { id: '1', username: 'admin', email: 'admin@nexterp.com', firstName: 'System', lastName: 'Administrator', roles: ['SuperAdmin'], isActive: true },
  { id: '2', username: 'sarah.chen', email: 'sarah.chen@company.com', firstName: 'Sarah', lastName: 'Chen', roles: ['Manager'], isActive: true },
  { id: '3', username: 'james.wilson', email: 'james.wilson@company.com', firstName: 'James', lastName: 'Wilson', roles: ['User'], isActive: true },
];

export const mockDashboardStats = {
  totalEmployees: 85,
  totalInventoryItems: 245,
  totalPurchaseOrders: 42,
  totalSuppliers: 18,
  totalProjects: 7,
  totalAccounts: 56,
};

export const mockActivities = [
  { id: '1', type: 'order', message: 'Purchase Order #PO-2024-001 approved', timestamp: '5 min ago', user: 'Sarah Chen' },
  { id: '2', type: 'user', message: 'New employee John Doe added to HRM', timestamp: '15 min ago', user: 'HR Admin' },
  { id: '3', type: 'document', message: 'Invoice #INV-2024-045 created', timestamp: '30 min ago', user: 'Finance' },
  { id: '4', type: 'approval', message: 'Budget request BR-2024-012 approved', timestamp: '1 hour ago', user: 'Manager' },
  { id: '5', type: 'order', message: 'New supplier contract signed: TechParts Inc', timestamp: '2 hours ago', user: 'Procurement' },
  { id: '6', type: 'alert', message: 'Low stock alert: Item ITM-001 below threshold', timestamp: '3 hours ago' },
];

export const mockRoles = [
  { id: '1', name: 'SuperAdmin', description: 'Full system access', userCount: 1, permissions: ['*'] },
  { id: '2', name: 'Manager', description: 'Department management', userCount: 5, permissions: ['read', 'write', 'approve'] },
  { id: '3', name: 'User', description: 'Standard user access', userCount: 20, permissions: ['read'] },
];

export const mockPermissions = [
  { id: '1', name: 'employees.read', description: 'View employees', category: 'HRM' },
  { id: '2', name: 'employees.write', description: 'Create/edit employees', category: 'HRM' },
  { id: '3', name: 'inventory.read', description: 'View inventory', category: 'Inventory' },
  { id: '4', name: 'inventory.write', description: 'Manage inventory', category: 'Inventory' },
  { id: '5', name: 'orders.read', description: 'View purchase orders', category: 'Purchasing' },
  { id: '6', name: 'orders.write', description: 'Create orders', category: 'Purchasing' },
  { id: '7', name: 'accounting.read', description: 'View accounts', category: 'Accounting' },
  { id: '8', name: 'reports.export', description: 'Export reports', category: 'Reports' },
];

export const mockModules = [
  { id: '1', name: 'HRM', code: 'hrm', description: 'Human Resource Management', isEnabled: true, icon: 'Users' },
  { id: '2', name: 'Inventory', code: 'inventory', description: 'Inventory Management', isEnabled: true, icon: 'Package' },
  { id: '3', name: 'Purchasing', code: 'purchasing', description: 'Purchase Management', isEnabled: true, icon: 'ShoppingCart' },
  { id: '4', name: 'Accounting', code: 'accounting', description: 'Financial Accounting', isEnabled: true, icon: 'DollarSign' },
  { id: '5', name: 'Projects', code: 'projects', description: 'Project Management', isEnabled: false, icon: 'FolderKanban' },
];
