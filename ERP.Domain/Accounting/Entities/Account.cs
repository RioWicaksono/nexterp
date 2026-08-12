using ERP.Domain.Common;
using ERP.Domain.Accounting.Enums;

namespace ERP.Domain.Accounting.Entities;

/// <summary>
/// Account entity representing a chart of accounts entry
/// </summary>
public class Account : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string AccountCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentId { get; private set; }
    public AccountType Type { get; private set; }
    public AccountClass Class { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool AllowDirectPosting { get; private set; } = true;
    public bool IsBankAccount { get; private set; }
    public bool IsCashAccount { get; private set; }
    public decimal? OpeningBalance { get; private set; }
    public DateTime? OpeningBalanceDate { get; private set; }
    public string? BankAccountNumber { get; private set; }
    public string? BankName { get; private set; }

    // Navigation properties
    private readonly Account? _parent;
    public Account? Parent => _parent;

    private readonly List<Account> _children = new();
    public IReadOnlyCollection<Account> Children => _children.AsReadOnly();

    private readonly List<JournalLine> _journalLines = new();
    public IReadOnlyCollection<JournalLine> JournalLines => _journalLines.AsReadOnly();

    // Balance calculated from journal lines
    public decimal Balance => _journalLines
        .Where(j => j.IsActive && !j.JournalEntry!.IsDeleted)
        .Sum(j => Class == AccountClass.Credit
            ? -j.DebitAmount + j.CreditAmount
            : j.DebitAmount - j.CreditAmount);

    // Factory method
    public static Account Create(
        Guid organizationId,
        string accountCode,
        string name,
        AccountType type,
        AccountClass accountClass,
        string? description = null,
        Guid? parentId = null,
        bool allowDirectPosting = true,
        bool isBankAccount = false,
        bool isCashAccount = false)
    {
        if (string.IsNullOrWhiteSpace(accountCode))
            throw new ArgumentException("Account code is required", nameof(accountCode));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name is required", nameof(name));

        return new Account
        {
            OrganizationId = organizationId,
            AccountCode = accountCode.Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
            ParentId = parentId,
            Type = type,
            Class = accountClass,
            AllowDirectPosting = allowDirectPosting,
            IsBankAccount = isBankAccount,
            IsCashAccount = isCashAccount
        };
    }

    public void Update(
        string? name = null,
        string? description = null,
        bool? allowDirectPosting = null)
    {
        Name = name?.Trim() ?? Name;
        Description = description?.Trim() ?? Description;
        AllowDirectPosting = allowDirectPosting ?? AllowDirectPosting;
        UpdateTimestamp();
    }

    public void SetParent(Guid? parentId)
    {
        ParentId = parentId;
        UpdateTimestamp();
    }

    public void SetAsBankAccount(string? bankAccountNumber = null, string? bankName = null)
    {
        IsBankAccount = true;
        BankAccountNumber = bankAccountNumber?.Trim();
        BankName = bankName?.Trim();
        UpdateTimestamp();
    }

    public void SetAsCashAccount()
    {
        IsCashAccount = true;
        IsBankAccount = false;
        UpdateTimestamp();
    }

    public void SetOpeningBalance(decimal balance, DateTime date)
    {
        OpeningBalance = balance;
        OpeningBalanceDate = date;
        UpdateTimestamp();
    }

    public void Activate() { IsActive = true; UpdateTimestamp(); }
    public void Deactivate() { IsActive = false; UpdateTimestamp(); }
}

/// <summary>
/// Journal Entry header - represents a financial transaction
/// </summary>
public class JournalEntry : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string EntryNumber { get; private set; } = string.Empty;
    public DateTime EntryDate { get; private set; }
    public DateTime PostingDate { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public JournalEntryStatus Status { get; private set; } = JournalEntryStatus.Draft;
    public bool IsAutoEntry { get; private set; }
    public Guid? ReversedEntryId { get; private set; }

    // Navigation properties
    private readonly List<JournalLine> _lines = new();
    public IReadOnlyCollection<JournalLine> Lines => _lines.AsReadOnly();

    // Totals
    public decimal TotalDebit => _lines.Sum(l => l.DebitAmount);
    public decimal TotalCredit => _lines.Sum(l => l.CreditAmount);
    public bool IsBalanced => TotalDebit == TotalCredit;
    public decimal Difference => TotalDebit - TotalCredit;

    // Factory method
    public static JournalEntry Create(
        Guid organizationId,
        string entryNumber,
        DateTime entryDate,
        DateTime postingDate,
        string title,
        string? notes = null,
        Guid? referenceId = null,
        string? referenceType = null,
        string? referenceNumber = null,
        bool isAutoEntry = false)
    {
        if (string.IsNullOrWhiteSpace(entryNumber))
            throw new ArgumentException("Entry number is required", nameof(entryNumber));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Entry title is required", nameof(title));

        return new JournalEntry
        {
            OrganizationId = organizationId,
            EntryNumber = entryNumber.Trim(),
            EntryDate = entryDate,
            PostingDate = postingDate,
            Title = title.Trim(),
            Notes = notes?.Trim(),
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            ReferenceNumber = referenceNumber?.Trim(),
            IsAutoEntry = isAutoEntry
        };
    }

    public void Update(string? title = null, DateTime? postingDate = null, string? notes = null)
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException("Can only update draft entries");

        Title = title?.Trim() ?? Title;
        PostingDate = postingDate ?? PostingDate;
        Notes = notes?.Trim() ?? Notes;
        UpdateTimestamp();
    }

    public void AddLine(JournalLine line)
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException("Can only modify draft entries");

        if (!_lines.Any(l => l.AccountId == line.AccountId && l.Id == line.Id))
        {
            _lines.Add(line);
            UpdateTimestamp();
        }
    }

    public void RemoveLine(Guid lineId)
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException("Can only modify draft entries");

        var line = _lines.FirstOrDefault(l => l.Id == lineId);
        if (line != null)
        {
            _lines.Remove(line);
            UpdateTimestamp();
        }
    }

    public void ClearLines()
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException("Can only modify draft entries");

        _lines.Clear();
        UpdateTimestamp();
    }

    public void Submit()
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException("Entry already submitted");

        if (!_lines.Any())
            throw new InvalidOperationException("Entry must have at least one line");

        if (!IsBalanced)
            throw new InvalidOperationException($"Entry is not balanced. Difference: {Difference}");

        Status = JournalEntryStatus.Submitted;
        UpdateTimestamp();
    }

    public void Approve()
    {
        if (Status != JournalEntryStatus.Submitted)
            throw new InvalidOperationException("Entry must be submitted first");

        Status = JournalEntryStatus.Approved;
        foreach (var line in _lines)
        {
            line.MarkAsPosted();
        }
        UpdateTimestamp();
    }

    public void Post()
    {
        if (Status != JournalEntryStatus.Approved)
            throw new InvalidOperationException("Entry must be approved first");

        Status = JournalEntryStatus.Posted;
        UpdateTimestamp();
    }

    public void Cancel()
    {
        if (Status == JournalEntryStatus.Posted)
            throw new InvalidOperationException("Posted entries cannot be cancelled. Use reversal instead.");

        if (Status == JournalEntryStatus.Cancelled)
            throw new InvalidOperationException("Entry is already cancelled");

        Status = JournalEntryStatus.Cancelled;
        UpdateTimestamp();
    }

    public void Reverse()
    {
        if (Status != JournalEntryStatus.Posted)
            throw new InvalidOperationException("Only posted entries can be reversed");

        IsDeleted = true;
        Status = JournalEntryStatus.Reversed;
        UpdateTimestamp();
    }
}

/// <summary>
/// Journal Line - individual debit/credit entry
/// </summary>
public class JournalLine : BaseEntity
{
    public Guid JournalEntryId { get; private set; }
    public Guid AccountId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal DebitAmount { get; private set; }
    public decimal CreditAmount { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public Guid? ProjectId { get; private set; }
    public string? Reference { get; private set; }
    public DateTime? DueDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? PostedAt { get; private set; }

    // Navigation properties
    private readonly JournalEntry? _journalEntry;
    public JournalEntry? JournalEntry => _journalEntry;

    private readonly Account? _account;
    public Account? Account => _account;

    public bool IsDebit => DebitAmount > 0;
    public bool IsCredit => CreditAmount > 0;
    public decimal NetAmount => DebitAmount - CreditAmount;

    public static JournalLine Create(
        Guid accountId,
        string description,
        decimal debitAmount = 0,
        decimal creditAmount = 0,
        Guid? costCenterId = null,
        Guid? projectId = null,
        string? reference = null,
        DateTime? dueDate = null)
    {
        if (debitAmount < 0 || creditAmount < 0)
            throw new ArgumentException("Amounts cannot be negative");

        if (debitAmount > 0 && creditAmount > 0)
            throw new ArgumentException("Line cannot have both debit and credit");

        if (debitAmount == 0 && creditAmount == 0)
            throw new ArgumentException("At least one of debit or credit amount is required");

        return new JournalLine
        {
            AccountId = accountId,
            Description = description.Trim(),
            DebitAmount = Math.Round(debitAmount, 2),
            CreditAmount = Math.Round(creditAmount, 2),
            CostCenterId = costCenterId,
            ProjectId = projectId,
            Reference = reference?.Trim(),
            DueDate = dueDate
        };
    }

    public void Update(decimal debitAmount, decimal creditAmount)
    {
        if (IsActive)
            throw new InvalidOperationException("Cannot update posted line");

        if (debitAmount < 0 || creditAmount < 0)
            throw new ArgumentException("Amounts cannot be negative");

        DebitAmount = Math.Round(debitAmount, 2);
        CreditAmount = Math.Round(creditAmount, 2);
        UpdateTimestamp();
    }

    public void MarkAsPosted()
    {
        IsActive = false;
        PostedAt = DateTime.UtcNow;
    }
}
