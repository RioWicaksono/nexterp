/**
 * Unit Tests for Export Utilities
 */

import {
  toCSV,
  formatNumber,
  formatCurrency,
  formatDate,
  ExportPresets,
} from '@/lib/export';

describe('Export Utilities', () => {
  describe('toCSV', () => {
    it('should convert array of objects to CSV string', () => {
      const data = [
        { id: 1, name: 'John', email: 'john@test.com' },
        { id: 2, name: 'Jane', email: 'jane@test.com' },
      ];

      const columns = [
        { key: 'id', header: 'ID' },
        { key: 'name', header: 'Name' },
        { key: 'email', header: 'Email' },
      ];

      const result = toCSV({ columns, data });

      expect(result).toContain('ID,Name,Email');
      expect(result).toContain('1,John,john@test.com');
      expect(result).toContain('2,Jane,jane@test.com');
    });

    it('should handle nested values with dot notation', () => {
      const data = [
        { id: 1, profile: { name: 'John', address: { city: 'Jakarta' } } },
      ];

      const columns = [
        { key: 'id', header: 'ID' },
        { key: 'profile.name', header: 'Name' },
        { key: 'profile.address.city', header: 'City' },
      ];

      const result = toCSV({ columns, data });

      expect(result).toContain('ID,Name,City');
      expect(result).toContain('1,John,Jakarta');
    });

    it('should handle custom formatters', () => {
      const data = [{ amount: 1000 }, { amount: 2500 }];

      const columns = [
        { key: 'amount', header: 'Amount', formatter: (v: number) => `$${v}` },
      ];

      const result = toCSV({ columns, data });

      expect(result).toContain('$1000');
      expect(result).toContain('$2500');
    });

    it('should escape values with commas and quotes', () => {
      const data = [{ name: 'John, Jr.', company: 'Test "Corp"' }];

      const columns = [
        { key: 'name', header: 'Name' },
        { key: 'company', header: 'Company' },
      ];

      const result = toCSV({ columns, data });

      expect(result).toContain('"John, Jr."');
      expect(result).toContain('"Test ""Corp"""');
    });

    it('should handle empty data array', () => {
      const columns = [{ key: 'id', header: 'ID' }];
      const data: unknown[] = [];

      const result = toCSV({ columns, data });

      expect(result).toBe('ID');
    });
  });

  describe('formatNumber', () => {
    it('should format numbers with commas', () => {
      expect(formatNumber(1000)).toBe('1,000');
      expect(formatNumber(1000000)).toBe('1,000,000');
      expect(formatNumber(100)).toBe('100');
    });

    it('should handle decimal numbers', () => {
      expect(formatNumber(1234.56)).toBe('1,234.56');
    });
  });

  describe('formatCurrency', () => {
    it('should format currency with default USD', () => {
      const result = formatCurrency(1000);
      expect(result).toContain('1,000');
      expect(result).toContain('$');
    });

    it('should format currency with custom currency', () => {
      const result = formatCurrency(1000, 'USD');
      expect(result).toContain('1,000');
      expect(result).toContain('$');
    });
  });

  describe('formatDate', () => {
    it('should format date string', () => {
      const result = formatDate('2024-01-15');
      expect(result).toContain('Jan');
      expect(result).toContain('15');
      expect(result).toContain('2024');
    });

    it('should format Date object', () => {
      const date = new Date('2024-06-20');
      const result = formatDate(date);
      expect(result).toContain('Jun');
      expect(result).toContain('20');
      expect(result).toContain('2024');
    });
  });

  describe('ExportPresets', () => {
    it('should generate orders export config', () => {
      const orders = [
        { id: 'ORD-001', customer: 'John', date: '2024-01-15', status: 'completed', total: 1000 },
      ];

      const config = ExportPresets.orders(orders);

      expect(config.filename).toBe('orders_export');
      expect(config.columns).toHaveLength(5);
      expect(config.columns[0].key).toBe('id');
      expect(config.data).toEqual(orders);
    });

    it('should generate products export config', () => {
      const products = [
        { sku: 'SKU-001', name: 'Product 1', category: 'Electronics', price: 100, stock: 50, status: 'active' },
      ];

      const config = ExportPresets.products(products);

      expect(config.filename).toBe('products_export');
      expect(config.columns).toHaveLength(6);
      expect(config.data).toEqual(products);
    });

    it('should generate customers export config', () => {
      const customers = [
        { id: 1, name: 'John', email: 'john@test.com', phone: '123', totalOrders: 5, totalSpent: 5000 },
      ];

      const config = ExportPresets.customers(customers);

      expect(config.filename).toBe('customers_export');
      expect(config.columns).toHaveLength(6);
    });

    it('should generate inventory export config', () => {
      const inventory = [
        { sku: 'SKU-001', name: 'Product', warehouse: 'Main', quantity: 100, reorderLevel: 20, status: 'in_stock' },
      ];

      const config = ExportPresets.inventory(inventory);

      expect(config.filename).toBe('inventory_export');
      expect(config.columns).toHaveLength(6);
    });
  });
});
