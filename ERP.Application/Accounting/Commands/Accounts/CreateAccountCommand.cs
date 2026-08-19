using FluentValidation;
using MediatR;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Accounting.Enums;

namespace ERP.Application.Accounting.Commands.Accounts;

/// <summary>
/// Command to create a new account
/// </summary>
public class CreateAccountCommand : ICommand<Guid>
{
    public string AccountCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AccountType AccountType { get; set; }
    public AccountClass Class { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsBankAccount { get; set; }
    public bool IsCashAccount { get; set; }
    public decimal? OpeningBalance { get; set; }
    public DateTime? OpeningBalanceDate { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
}

/// <summary>
/// Validator for CreateAccountCommand
/// </summary>
public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
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

        RuleFor(x => x.OpeningBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Opening balance cannot be negative")
            .When(x => x.OpeningBalance.HasValue);
    }
}
