using ERP.Application.Common.DTOs;

namespace ERP.Application.Accounting.DTOs;

/// <summary>
/// Account data transfer object
/// </summary>
public class AccountDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public string? ParentCode { get; set; }
    public string Type { get; set; } = string.Empty;
    public string AccountClass { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool AllowDirectPosting { get; set; }
    public bool IsBankAccount { get; set; }
    public bool IsCashAccount { get; set; }
    public decimal Balance { get; set; }
}

/// <summary>
/// Journal Entry data transfer object
/// </summary>
public class JournalEntryDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public DateTime PostingDate { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal Difference { get; set; }
    public bool IsBalanced => TotalDebit == TotalCredit;
    public List<JournalLineDto> Lines { get; set; } = new();
}

/// <summary>
/// Journal Line data transfer object
/// </summary>
public class JournalLineDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? Reference { get; set; }
    public DateTime? DueDate { get; set; }
}

/// <summary>
/// DTO for creating a journal entry
/// </summary>
public class CreateJournalEntryDto
{
    public DateTime EntryDate { get; set; }
    public DateTime PostingDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceNumber { get; set; }
    public List<CreateJournalLineDto> Lines { get; set; } = new();
}

/// <summary>
/// DTO for creating a journal line
/// </summary>
public class CreateJournalLineDto
{
    public Guid AccountId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? Reference { get; set; }
    public DateTime? DueDate { get; set; }
}
