using MediatR;
using ERP.Application.Common.Base;

namespace ERP.Application.Accounting.Commands.Accounts;

/// <summary>
/// Command to delete an account (soft delete)
/// </summary>
public class DeleteAccountCommand : ICommand<bool>
{
    public Guid Id { get; set; }
}
