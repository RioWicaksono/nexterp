namespace ERP.Domain.Accounting.Enums;

/// <summary>
/// Account types based on accounting classification
/// </summary>
public enum AccountType
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}

/// <summary>
/// Account class for debit/credit rules
/// </summary>
public enum AccountClass
{
    Debit = 1,    // Assets, Expenses (increase with debit)
    Credit = 2   // Liabilities, Equity, Revenue (increase with credit)
}

/// <summary>
/// Journal Entry status workflow
/// </summary>
public enum JournalEntryStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Posted = 4,
    Cancelled = 5,
    Reversed = 6
}
