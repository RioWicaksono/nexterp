using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Accounting.Entities;

namespace ERP.Application.Accounting.Commands.JournalEntries;

/// <summary>
/// Command to submit a journal entry for approval
/// </summary>
public class SubmitJournalEntryCommand : ICommand<bool>
{
    public Guid Id { get; set; }
}

/// <summary>
/// Handler for SubmitJournalEntryCommand
/// </summary>
public class SubmitJournalEntryCommandHandler : IRequestHandler<SubmitJournalEntryCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SubmitJournalEntryCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(SubmitJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedAccessException("User is not associated with an organization");

        var entry = await _context.JournalEntries
            .FirstOrDefaultAsync(j => j.Id == request.Id && !j.IsDeleted && j.OrganizationId == organizationId, cancellationToken);

        if (entry == null)
            return Result<bool>.Failure("Journal entry not found");

        try
        {
            entry.Submit();
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
