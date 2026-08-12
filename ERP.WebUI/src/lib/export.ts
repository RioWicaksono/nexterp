/**
 * Data Export Utilities
 * Supports CSV and PDF export functionality
 */

// Type-safe export interfaces
interface ExportColumn<T = unknown> {
  key: string;
  header: string;
  formatter?: (value: unknown, row: T) => string;
}

interface ExportOptions<T extends object = object> {
  filename?: string;
  columns: ExportColumn<T>[];
  data: T[];
  delimiter?: string;
}

/**
 * Convert data to CSV format
 */
export function toCSV<T extends object>(options: ExportOptions<T>): string {
  const { columns, data, delimiter = "," } = options;

  // Create header row
  const headers = columns.map((col) => escapeCSVValue(col.header)).join(delimiter);

  // Create data rows
  const rows = data.map((row) => {
    return columns
      .map((col) => {
        const value = getNestedValue(row, col.key);
        const formatted = col.formatter ? col.formatter(value, row) : value;
        return escapeCSVValue(formatted);
      })
      .join(delimiter);
  });

  return [headers, ...rows].join("\n");
}

/**
 * Escape CSV value (handle quotes and commas)
 */
function escapeCSVValue(value: unknown): string {
  if (value === null || value === undefined) return "";
  const str = String(value);
  if (str.includes(",") || str.includes('"') || str.includes("\n")) {
    return `"${str.replace(/"/g, '""')}"`;
  }
  return str;
}

/**
 * Get nested value from object using dot notation
 */
function getNestedValue<T extends object>(obj: T, path: string): unknown {
  return path.split(".").reduce((current, key) => (current as Record<string, unknown>)?.[key], obj);
}

/**
 * Download CSV file
 */
export function downloadCSV<T extends object>(options: ExportOptions<T>): void {
  const { filename = "export", data } = options;

  if (data.length === 0) {
    throw new Error("No data to export");
  }

  const csv = toCSV(options);
  const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
  const url = URL.createObjectURL(blob);

  const link = document.createElement("a");
  link.href = url;
  link.download = `${sanitizeFilename(filename)}_${formatDateForFilename(new Date())}.csv`;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}

/**
 * Sanitize filename
 */
function sanitizeFilename(name: string): string {
  return name.replace(/[^a-z0-9_-]/gi, "_").toLowerCase();
}

/**
 * Format date for filename
 */
function formatDateForFilename(date: Date): string {
  return date.toISOString().split("T")[0];
}

/**
 * Export table to CSV (from DOM element)
 */
export function exportTableToCSV(
  tableId: string,
  filename: string = "export"
): void {
  const table = document.getElementById(tableId);
  if (!table) throw new Error(`Table with id "${tableId}" not found`);

  const rows = table.querySelectorAll("tr");
  const csv: string[] = [];

  rows.forEach((row) => {
    const cells = row.querySelectorAll("td, th");
    const rowData: string[] = [];

    cells.forEach((cell) => {
      // Skip action columns (last column with buttons)
      if (!cell.classList.contains("actions")) {
        let text = cell.textContent?.trim() || "";
        rowData.push(escapeCSVValue(text));
      }
    });

    if (rowData.length > 0) {
      csv.push(rowData.join(","));
    }
  });

  const blob = new Blob([csv.join("\n")], { type: "text/csv;charset=utf-8;" });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = `${sanitizeFilename(filename)}_${formatDateForFilename(new Date())}.csv`;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}

/**
 * Escape HTML to prevent XSS
 */
function escapeHtml(text: string): string {
  const map: Record<string, string> = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#039;'
  };
  return text.replace(/[&<>"']/g, m => map[m]);
}

/**
 * Generate PDF using browser print (most reliable method)
 * With proper XSS protection using text extraction instead of innerHTML
 */
export function downloadPDF(elementId: string, filename: string = "export"): void {
  const element = document.getElementById(elementId);
  if (!element) throw new Error(`Element with id "${elementId}" not found`);

  // Store original styles
  const originalBg = element.style.background;
  const originalColor = element.style.color;

  // Ensure white background for PDF
  element.style.background = "white";
  element.style.color = "black";

  // Open print dialog
  const printWindow = window.open("", "_blank");
  if (!printWindow) throw new Error("Failed to open print window");

  // Extract safe text content instead of using innerHTML directly
  // This prevents XSS attacks from malicious content in the element
  const safeTableHtml = extractTableAsSafeHtml(element);

  printWindow.document.write(`
    <!DOCTYPE html>
    <html>
    <head>
      <title>${escapeHtml(filename)}</title>
      <style>
        @page { size: A4; margin: 20px; }
        body { font-family: 'Segoe UI', system-ui, sans-serif; padding: 20px; }
        h1 { font-size: 24px; margin-bottom: 20px; color: #0f172a; }
        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        th, td { border: 1px solid #e2e8f0; padding: 10px; text-align: left; font-size: 12px; }
        th { background: #f8fafc; font-weight: 600; }
        .badge { padding: 4px 8px; border-radius: 4px; font-size: 10px; }
        .badge-success { background: #dcfce7; color: #166534; }
        .badge-warning { background: #fef3c7; color: #92400e; }
        .badge-danger { background: #fee2e2; color: #991b1b; }
        .badge-info { background: #dbeafe; color: #1e40af; }
        .footer { margin-top: 40px; font-size: 10px; color: #94a3b8; text-align: center; }
      </style>
    </head>
    <body>
      <h1>${escapeHtml(filename)}</h1>
      <p>Generated: ${new Date().toLocaleString()}</p>
      ${safeTableHtml}
      <div class="footer">
        NEXTERP - Enterprise Resource Planning System | Generated on ${new Date().toISOString()}
      </div>
      <script>
        window.onload = function() {
          window.print();
          window.close();
        };
      </script>
    </body>
    </html>
  `);

  printWindow.document.close();

  // Restore original styles
  element.style.background = originalBg;
  element.style.color = originalColor;
}

/**
 * Extract table content as safe HTML (escapes user data)
 * Prevents XSS by escaping all cell text content
 */
function extractTableAsSafeHtml(element: HTMLElement): string {
  const table = element.querySelector('table');
  if (!table) return '<p>No table found</p>';

  let html = '<table>';

  // Process header rows
  const headers = table.querySelectorAll('thead th, thead td');
  if (headers.length > 0) {
    html += '<thead><tr>';
    headers.forEach(th => {
      html += `<th>${escapeHtml(th.textContent || '')}</th>`;
    });
    html += '</tr></thead>';
  }

  // Process body rows
  const rows = table.querySelectorAll('tbody tr');
  if (rows.length > 0) {
    html += '<tbody>';
    rows.forEach(tr => {
      html += '<tr>';
      const cells = tr.querySelectorAll('td');
      cells.forEach(td => {
        // Get text content only (no HTML)
        const text = td.textContent || '';
        // Preserve badge classes but escape the text
        const hasBadge = td.classList.contains('badge') ||
                        td.classList.contains('badge-success') ||
                        td.classList.contains('badge-warning') ||
                        td.classList.contains('badge-danger') ||
                        td.classList.contains('badge-info');

        if (hasBadge) {
          const badgeClass = Array.from(td.classList)
            .filter(c => c.startsWith('badge'))
            .join(' ');
          html += `<td><span class="${badgeClass}">${escapeHtml(text)}</span></td>`;
        } else {
          html += `<td>${escapeHtml(text)}</td>`;
        }
      });
      html += '</tr>';
    });
    html += '</tbody>';
  }

  html += '</table>';
  return html;
}

/**
 * Format number for display
 */
export function formatNumber(num: number, locale: string = "en-US"): string {
  return new Intl.NumberFormat(locale).format(num);
}

/**
 * Format currency
 */
export function formatCurrency(
  amount: number,
  currency: string = "USD",
  locale: string = "en-US"
): string {
  return new Intl.NumberFormat(locale, {
    style: "currency",
    currency,
  }).format(amount);
}

/**
 * Format date
 */
export function formatDate(date: Date | string, locale: string = "en-US"): string {
  const d = typeof date === "string" ? new Date(date) : date;
  return new Intl.DateTimeFormat(locale, {
    year: "numeric",
    month: "short",
    day: "numeric",
  }).format(d);
}

/**
 * Export hooks for common data types
 */
export const ExportPresets = {
  orders: <T extends Record<string, unknown>>(data: T[]) => ({
    filename: "orders_export",
    columns: [
      { key: "id", header: "Order ID" },
      { key: "customer", header: "Customer" },
      { key: "date", header: "Date", formatter: (_v: unknown) => formatDate(_v as Date | string) },
      { key: "status", header: "Status" },
      { key: "total", header: "Total", formatter: (v: unknown) => formatCurrency(v as number) },
    ] as ExportColumn<T>[],
    data,
  }),

  products: <T extends Record<string, unknown>>(data: T[]) => ({
    filename: "products_export",
    columns: [
      { key: "sku", header: "SKU" },
      { key: "name", header: "Product Name" },
      { key: "category", header: "Category" },
      { key: "price", header: "Price", formatter: (v: unknown) => formatCurrency(v as number) },
      { key: "stock", header: "Stock" },
      { key: "status", header: "Status" },
    ] as ExportColumn<T>[],
    data,
  }),

  customers: <T extends Record<string, unknown>>(data: T[]) => ({
    filename: "customers_export",
    columns: [
      { key: "id", header: "Customer ID" },
      { key: "name", header: "Name" },
      { key: "email", header: "Email" },
      { key: "phone", header: "Phone" },
      { key: "totalOrders", header: "Total Orders" },
      { key: "totalSpent", header: "Total Spent", formatter: (v: unknown) => formatCurrency(v as number) },
    ] as ExportColumn<T>[],
    data,
  }),

  inventory: <T extends Record<string, unknown>>(data: T[]) => ({
    filename: "inventory_export",
    columns: [
      { key: "sku", header: "SKU" },
      { key: "name", header: "Product" },
      { key: "warehouse", header: "Warehouse" },
      { key: "quantity", header: "Quantity", formatter: (v: unknown) => formatNumber(v as number) },
      { key: "reorderLevel", header: "Reorder Level", formatter: (v: unknown) => formatNumber(v as number) },
      { key: "status", header: "Status" },
    ] as ExportColumn<T>[],
    data,
  }),
};
