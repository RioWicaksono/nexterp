using FluentValidation;
using MediatR;
using ERP.Application.Common.Base;
using ERP.Domain.Accounting.Enums;

namespace ERP.Application.Accounting.Commands.Accounts;

/// <summary>
/// Command to update an existing account
/// </summary>
public class UpdateAccountCommand : ICommand<bool>
{
    public Guid Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AccountType AccountType { get; set; }
    public AccountClass Class { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsBankAccount { get; set; }
    public bool IsCashAccount { get; set; }
    public bool IsActive { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
}

/// <summary>
/// Validator for UpdateAccountCommand
/// </summary>
public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Account ID is required");

        RuleFor(x => x.AccountCode)
            .NotEmpty().WithMessage("Account code is required")
            .MaximumLength(50).WithMessage("Account code cannot exceed 50 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Account name is required")
            .MaximumLength(200).WithMessage("Account name cannot exceed 200 characters");

        RuleFor(x => x.AccountType)
            .IsInEnum().WithMessage("Invalid account type");

        RuleFor(x => x.Class)
            .IsInEnum().WithMessage("Invalid account class");
    }
}
