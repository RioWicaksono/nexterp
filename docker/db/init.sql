-- ERP Database Initialization Script
-- This script runs automatically on first container start

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Create custom types for ERP
DO $$
BEGIN
    -- Stock Transaction Type Enum
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'stock_transaction_type') THEN
        CREATE TYPE stock_transaction_type AS ENUM (
            'PurchaseReceipt',
            'PurchaseReturn',
            'SalesDelivery',
            'SalesReturn',
            'StockTransfer',
            'StockAdjustment',
            'StockOpening',
            'StockDamage',
            'StockExpired'
        );
    END IF;

    -- Valuation Method Enum
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'valuation_method') THEN
        CREATE TYPE valuation_method AS ENUM (
            'AverageCost',
            'FIFO',
            'LIFO',
            'StandardCost'
        );
    END IF;

    -- UOM Type Enum
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'uom_type') THEN
        CREATE TYPE uom_type AS ENUM (
            'Quantity',
            'Weight',
            'Volume',
            'Length',
            'Area',
            'Time',
            'Currency'
        );
    END IF;

    -- Account Type Enum
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'account_type') THEN
        CREATE TYPE account_type AS ENUM (
            'Asset',
            'Liability',
            'Equity',
            'Revenue',
            'Expense'
        );
    END IF;

    -- Account Class Enum
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'account_class') THEN
        CREATE TYPE account_class AS ENUM (
            'Debit',
            'Credit'
        );
    END IF;

    -- Journal Entry Status Enum
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'journal_entry_status') THEN
        CREATE TYPE journal_entry_status AS ENUM (
            'Draft',
            'Submitted',
            'Approved',
            'Posted',
            'Cancelled',
            'Reversed'
        );
    END IF;

    -- Customer Type Enum
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'customer_type') THEN
        CREATE TYPE customer_type AS ENUM (
            'Individual',
            'Company',
            'Government'
        );
    END IF;

    -- Supplier Type Enum
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'supplier_type') THEN
        CREATE TYPE supplier_type AS ENUM (
            'Individual',
            'Company',
            'Government'
        );
    END IF;

    -- Sales Order Status Enum
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'sales_order_status') THEN
        CREATE TYPE sales_order_status AS ENUM (
            'Draft',
            'Submitted',
            'Approved',
            'Rejected',
            'Cancelled',
            'Delivered',
            'Invoiced'
        );
    END IF;

    -- Purchase Order Status Enum
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'purchase_order_status') THEN
        CREATE TYPE purchase_order_status AS ENUM (
            'Draft',
            'Submitted',
            'Approved',
            'Rejected',
            'Cancelled',
            'Received',
            'Invoiced'
        );
    END IF;

    -- Stock Transaction Status Enum
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'stock_transaction_status') THEN
        CREATE TYPE stock_transaction_status AS ENUM (
            'Pending',
            'Approved',
            'Rejected',
            'Completed',
            'Cancelled'
        );
    END IF;

    -- Sales Invoice Status Enum
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'sales_invoice_status') THEN
        CREATE TYPE sales_invoice_status AS ENUM (
            'Draft',
            'Submitted',
            'Posted',
            'Cancelled',
            'Paid'
        );
    END IF;

    -- Invoice Type Enum
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'invoice_type') THEN
        CREATE TYPE invoice_type AS ENUM (
            'Invoice',
            'CreditNote',
            'DebitNote'
        );
    END IF;
END $$;

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_organizations_code ON organizations(code);
CREATE INDEX IF NOT EXISTS idx_organizations_name ON organizations(name);
CREATE INDEX IF NOT EXISTS idx_users_username ON users(username);
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_org_id ON users(organization_id);

CREATE INDEX IF NOT EXISTS idx_stock_items_code ON stock_items(code);
CREATE INDEX IF NOT EXISTS idx_stock_items_barcode ON stock_items(barcode);
CREATE INDEX IF NOT EXISTS idx_stock_items_org_id ON stock_items(organization_id);
CREATE INDEX IF NOT EXISTS idx_warehouses_code ON warehouses(code);
CREATE INDEX IF NOT EXISTS idx_warehouses_org_id ON warehouses(organization_id);

CREATE INDEX IF NOT EXISTS idx_accounts_code ON accounts(account_code);
CREATE INDEX IF NOT EXISTS idx_accounts_org_id ON accounts(organization_id);
CREATE INDEX IF NOT EXISTS idx_accounts_type ON accounts(type);
CREATE INDEX IF NOT EXISTS idx_journal_entries_number ON journal_entries(entry_number);
CREATE INDEX IF NOT EXISTS idx_journal_entries_org_id ON journal_entries(organization_id);
CREATE INDEX IF NOT EXISTS idx_journal_entries_date ON journal_entries(entry_date);
CREATE INDEX IF NOT EXISTS idx_journal_entries_status ON journal_entries(status);

CREATE INDEX IF NOT EXISTS idx_customers_code ON customers(customer_code);
CREATE INDEX IF NOT EXISTS idx_customers_org_id ON customers(organization_id);
CREATE INDEX IF NOT EXISTS idx_sales_orders_number ON sales_orders(order_number);
CREATE INDEX IF NOT EXISTS idx_sales_orders_org_id ON sales_orders(organization_id);
CREATE INDEX IF NOT EXISTS idx_sales_orders_customer_id ON sales_orders(customer_id);
CREATE INDEX IF NOT EXISTS idx_sales_invoices_number ON sales_invoices(invoice_number);
CREATE INDEX IF NOT EXISTS idx_sales_invoices_org_id ON sales_invoices(organization_id);

CREATE INDEX IF NOT EXISTS idx_suppliers_code ON suppliers(supplier_code);
CREATE INDEX IF NOT EXISTS idx_suppliers_org_id ON suppliers(organization_id);
CREATE INDEX IF NOT EXISTS idx_purchase_orders_number ON purchase_orders(order_number);
CREATE INDEX IF NOT EXISTS idx_purchase_orders_org_id ON purchase_orders(organization_id);
CREATE INDEX IF NOT EXISTS idx_purchase_orders_supplier_id ON purchase_orders(supplier_id);

CREATE INDEX IF NOT EXISTS idx_stock_transactions_number ON stock_transactions(transaction_number);
CREATE INDEX IF NOT EXISTS idx_stock_transactions_org_id ON stock_transactions(organization_id);
CREATE INDEX IF NOT EXISTS idx_stock_transactions_item_id ON stock_transactions(stock_item_id);
CREATE INDEX IF NOT EXISTS idx_stock_transactions_date ON stock_transactions(transaction_date);

-- Create GIN indexes for full-text search
CREATE INDEX IF NOT EXISTS idx_stock_items_name_search ON stock_items USING gin(name gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_customers_name_search ON customers USING gin(customer_name gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_suppliers_name_search ON suppliers USING gin(supplier_name gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_accounts_name_search ON accounts USING gin(name gin_trgm_ops);

-- Create function for updated_at trigger
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Apply updated_at trigger to all tables
DROP TRIGGER IF EXISTS update_organizations_updated_at ON organizations;
CREATE TRIGGER update_organizations_updated_at BEFORE UPDATE ON organizations
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_users_updated_at ON users;
CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_roles_updated_at ON roles;
CREATE TRIGGER update_roles_updated_at BEFORE UPDATE ON roles
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_stock_items_updated_at ON stock_items;
CREATE TRIGGER update_stock_items_updated_at BEFORE UPDATE ON stock_items
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_warehouses_updated_at ON warehouses;
CREATE TRIGGER update_warehouses_updated_at BEFORE UPDATE ON warehouses
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_accounts_updated_at ON accounts;
CREATE TRIGGER update_accounts_updated_at BEFORE UPDATE ON accounts
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_journal_entries_updated_at ON journal_entries;
CREATE TRIGGER update_journal_entries_updated_at BEFORE UPDATE ON journal_entries
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_customers_updated_at ON customers;
CREATE TRIGGER update_customers_updated_at BEFORE UPDATE ON customers
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_sales_orders_updated_at ON sales_orders;
CREATE TRIGGER update_sales_orders_updated_at BEFORE UPDATE ON sales_orders
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_suppliers_updated_at ON suppliers;
CREATE TRIGGER update_suppliers_updated_at BEFORE UPDATE ON suppliers
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_purchase_orders_updated_at ON purchase_orders;
CREATE TRIGGER update_purchase_orders_updated_at BEFORE UPDATE ON purchase_orders
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Grant permissions
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO postgres;

-- Log completion
DO $$
BEGIN
    RAISE NOTICE 'ERP Database initialization completed successfully!';
END $$;
