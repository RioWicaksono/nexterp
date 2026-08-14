using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Accounting.DTOs;
using ERP.Domain.Accounting.Entities;

namespace ERP.Application.Accounting.Commands.JournalEntries;

/// <summary>
/// Command to create a new journal entry
/// </summary>
public class CreateJournalEntryCommand : ICommand<Guid>
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
/// Validator for CreateJournalEntryCommand
/// </summary>
public class CreateJournalEntryCommandValidator : AbstractValidator<CreateJournalEntryCommand>
{
    public CreateJournalEntryCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(500).WithMessage("Title cannot exceed 500 characters");

        RuleFor(x => x.EntryDate)
            .NotEmpty().WithMessage("Entry date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Entry date cannot be in the future");

        RuleFor(x => x.PostingDate)
            .NotEmpty().WithMessage("Posting date is required");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Journal entry must have at least one line")
            .Must(lines => lines.Count >= 2).WithMessage("Journal entry must have at least 2 lines");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.AccountId)
                .NotEmpty().WithMessage("Account is required for each line");

            line.RuleFor(l => l.Description)
                .NotEmpty().WithMessage("Description is required for each line")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            line.RuleFor(l => new { l.DebitAmount, l.CreditAmount })
                .Must(x => x.DebitAmount > 0 || x.CreditAmount > 0)
                .WithMessage("Each line must have either a debit or credit amount");

            line.RuleFor(l => new { l.DebitAmount, l.CreditAmount })
                .Must(x => !(x.DebitAmount > 0 && x.CreditAmount > 0))
                .WithMessage("A line cannot have both debit and credit amounts");
        });
    }
}

/// <summary>
/// Handler for CreateJournalEntryCommand
/// </summary>
public class CreateJournalEntryCommandHandler : IRequestHandler<CreateJournalEntryCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateJournalEntryCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        // Return failure if user is not associated with an organization
        if (_currentUser.OrganizationId == null)
            return Result<Guid>.Failure("User is not associated with an organization");

        var organizationId = _currentUser.OrganizationId.Value;

        // Validate all accounts exist
        var accountIds = request.Lines.Select(l => l.AccountId).Distinct().ToList();
        var accounts = await _context.Accounts
            .Where(a => accountIds.Contains(a.Id) && !a.IsDeleted && a.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        if (accounts.Count != accountIds.Count)
            return Result<Guid>.Failure("One or more accounts are invalid or inactive");

        // Validate that all accounts allow direct posting
        var nonPostingAccounts = accounts.Where(a => !a.AllowDirectPosting).ToList();
        if (nonPostingAccounts.Any())
        {
            var codes = string.Join(", ", nonPostingAccounts.Select(a => a.AccountCode));
            return Result<Guid>.Failure($"These accounts do not allow direct posting: {codes}");
        }

        // Calculate totals
        var totalDebit = request.Lines.Sum(l => l.DebitAmount);
        var totalCredit = request.Lines.Sum(l => l.CreditAmount);

        // Check balance
        if (Math.Abs(totalDebit - totalCredit) > 0.01m)
        {
            return Result<Guid>.Failure($"Journal entry is not balanced. Debit: {totalDebit}, Credit: {totalCredit}, Difference: {totalCredit - totalDebit}");
        }

        // Generate entry number
        var entryNumber = await GenerateEntryNumberAsync(organizationId, cancellationToken);

        // Create journal entry
        var entry = JournalEntry.Create(
            organizationId,
            entryNumber,
            request.EntryDate,
            request.PostingDate,
            request.Title,
            request.Notes,
            request.ReferenceId,
            request.ReferenceType,
            request.ReferenceNumber);

        // Add lines
        foreach (var lineDto in request.Lines)
        {
            var line = JournalLine.Create(
                lineDto.AccountId,
                lineDto.Description,
                lineDto.DebitAmount,
                lineDto.CreditAmount,
                lineDto.CostCenterId,
                lineDto.ProjectId,
                lineDto.Reference,
                lineDto.DueDate);

            entry.AddLine(line);
        }

        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entry.Id);
    }

    private async Task<string> GenerateEntryNumberAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var lastEntry = await _context.JournalEntries
            .Where(j => j.OrganizationId == organizationId)
            .OrderByDescending(j => j.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var lastNumber = 0;
        if (lastEntry != null)
        {
            var parts = lastEntry.EntryNumber.Split('-');
            if (parts.Length == 3 && parts[0] == $"JE{year}")
            {
                int.TryParse(parts[2], out lastNumber);
            }
        }

        return $"JE{year}-{(lastNumber + 1):D5}";
    }
}
