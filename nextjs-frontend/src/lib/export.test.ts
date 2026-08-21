import { downloadCSV, downloadExcel, toCSV } from './export';

describe('export utilities', () => {
  describe('toCSV', () => {
    it('should convert array of objects to CSV string', () => {
      const data = [
        { name: 'John', age: 30 },
        { name: 'Jane', age: 25 },
      ];
      const columns = [
        { key: 'name', header: 'Name' },
        { key: 'age', header: 'Age' },
      ];

      const result = toCSV(data, columns);

      expect(result).toBe('Name,Age\nJohn,30\nJane,25');
    });

    it('should escape values with commas', () => {
      const data = [{ name: 'Doe, John', age: 30 }];
      const columns = [
        { key: 'name', header: 'Name' },
        { key: 'age', header: 'Age' },
      ];

      const result = toCSV(data, columns);

      expect(result).toBe('Name,Age\n"Doe, John",30');
    });

    it('should escape values with quotes', () => {
      const data = [{ name: 'John "Jack" Doe', age: 30 }];
      const columns = [
        { key: 'name', header: 'Name' },
        { key: 'age', header: 'Age' },
      ];

      const result = toCSV(data, columns);

      expect(result).toBe('Name,Age\n"John ""Jack"" Doe",30');
    });

    it('should handle null and undefined values', () => {
      const data = [{ name: null, age: undefined }];
      const columns = [
        { key: 'name', header: 'Name' },
        { key: 'age', header: 'Age' },
      ];

      const result = toCSV(data, columns);

      expect(result).toBe('Name,Age\n,');
    });

    it('should use custom formatter when provided', () => {
      const data = [{ status: true, name: 'John' }];
      const columns = [
        { key: 'status', header: 'Status', formatter: (v: unknown) => (v ? 'Active' : 'Inactive') },
        { key: 'name', header: 'Name' },
      ];

      const result = toCSV(data, columns);

      expect(result).toBe('Status,Name\nActive,John');
    });

    it('should handle nested keys', () => {
      const data = [{ user: { name: 'John', age: 30 } }];
      const columns = [
        { key: 'user.name', header: 'Name' },
        { key: 'user.age', header: 'Age' },
      ];

      const result = toCSV(data, columns);

      expect(result).toBe('Name,Age\nJohn,30');
    });

    it('should handle empty array', () => {
      const data: Record<string, unknown>[] = [];
      const columns = [{ key: 'name', header: 'Name' }];

      const result = toCSV(data, columns);

      expect(result).toBe('Name');
    });
  });
});
