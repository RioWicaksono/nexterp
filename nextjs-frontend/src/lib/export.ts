/**
 * Export utilities for CSV and Excel files
 */

export interface ExportColumn<T> {
  key: keyof T | string;
  header: string;
  formatter?: (value: unknown, row: T) => string;
}

export interface ExportOptions {
  filename?: string;
  dateFormat?: string;
  includeTimestamp?: boolean;
}

/**
 * Convert data array to CSV string
 */
export function toCSV<T extends Record<string, unknown>>(
  data: T[],
  columns: ExportColumn<T>[],
  options: ExportOptions = {}
): string {
  const { dateFormat = 'yyyy-MM-dd' } = options;

  // Header row
  const headers = columns.map((col) => escapeCSV(col.header));
  const rows: string[] = [headers.join(',')];

  // Data rows
  for (const item of data) {
    const row = columns.map((col) => {
      let value: unknown;
      if (typeof col.key === 'string' && col.key.includes('.')) {
        // Nested key like "user.name"
        value = col.key.split('.').reduce((obj, key) => {
          if (obj && typeof obj === 'object') {
            return (obj as Record<string, unknown>)[key];
          }
          return undefined;
        }, item as unknown);
      } else {
        value = item[col.key as keyof T];
      }

      if (col.formatter) {
        value = col.formatter(value, item);
      } else if (value instanceof Date) {
        value = formatDate(value, dateFormat);
      } else if (value === null || value === undefined) {
        value = '';
      } else if (typeof value === 'object') {
        value = JSON.stringify(value);
      }

      return escapeCSV(String(value));
    });
    rows.push(row.join(','));
  }

  return rows.join('\n');
}

/**
 * Escape CSV special characters
 */
function escapeCSV(value: string): string {
  if (value.includes(',') || value.includes('"') || value.includes('\n')) {
    return `"${value.replace(/"/g, '""')}"`;
  }
  return value;
}

/**
 * Format date to string
 */
function formatDate(date: Date, format: string): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');

  return format
    .replace('yyyy', String(year))
    .replace('MM', month)
    .replace('dd', day)
    .replace('HH', hours)
    .replace('mm', minutes);
}

/**
 * Download data as CSV file
 */
export function downloadCSV<T extends Record<string, unknown>>(
  data: T[],
  columns: ExportColumn<T>[],
  options: ExportOptions = {}
): void {
  const {
    filename = 'export',
    includeTimestamp = true,
  } = options;

  const csv = toCSV(data, columns, options);
  const timestamp = includeTimestamp
    ? `_${new Date().toISOString().slice(0, 10)}`
    : '';
  const fullFilename = `${filename}${timestamp}.csv`;

  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.setAttribute('href', url);
  link.setAttribute('download', fullFilename);
  link.style.visibility = 'hidden';
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}

/**
 * Download data as Excel-compatible XML (for simple cases)
 */
export function downloadExcel<T extends Record<string, unknown>>(
  data: T[],
  columns: ExportColumn<T>[],
  options: ExportOptions = {}
): void {
  const {
    filename = 'export',
    includeTimestamp = true,
    dateFormat = 'yyyy-MM-dd',
  } = options;

  const headers = columns.map((col) => `<Cell><Data ss:Type="String">${escapeXML(col.header)}</Data></Cell>`);
  const headerRow = `<Row>${headers.join('')}</Row>`;

  const dataRows = data.map((item) => {
    const cells = columns.map((col) => {
      let value: unknown;
      if (typeof col.key === 'string' && col.key.includes('.')) {
        value = col.key.split('.').reduce((obj, key) => {
          if (obj && typeof obj === 'object') {
            return (obj as Record<string, unknown>)[key];
          }
          return undefined;
        }, item as unknown);
      } else {
        value = item[col.key as keyof T];
      }

      if (col.formatter) {
        value = col.formatter(value, item);
      } else if (value instanceof Date) {
        value = formatDate(value, dateFormat);
      }

      const cellValue = value === null || value === undefined ? '' : String(value);
      return `<Cell><Data ss:Type="String">${escapeXML(cellValue)}</Data></Cell>`;
    });
    return `<Row>${cells.join('')}</Row>`;
  });

  const xml = `<?xml version="1.0" encoding="UTF-8"?>
<?mso-application progid="Excel.Sheet"?>
<Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet"
          xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
  <Worksheet ss:Name="Sheet1">
    <Table>${headerRow}${dataRows.join('')}</Table>
  </Worksheet>
</Workbook>`;

  const timestamp = includeTimestamp
    ? `_${new Date().toISOString().slice(0, 10)}`
    : '';
  const fullFilename = `${filename}${timestamp}.xls`;

  const blob = new Blob([xml], { type: 'application/vnd.ms-excel;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.setAttribute('href', url);
  link.setAttribute('download', fullFilename);
  link.style.visibility = 'hidden';
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}

function escapeXML(str: string): string {
  return str
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&apos;');
}
