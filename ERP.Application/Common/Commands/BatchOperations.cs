using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Behaviors;

namespace ERP.Application.Common.Commands;

/// <summary>
/// Command to delete multiple entities by IDs (generic version)
/// Note: For specific entity types, use typed commands instead
/// </summary>
public class BatchDeleteGenericCommand : MediatR.IRequest<BatchDeleteResult>
{
    public string EntityType { get; set; } = string.Empty;
    public IEnumerable<Guid> Ids { get; set; } = Array.Empty<Guid>();
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Result of batch delete operation
/// </summary>
public class BatchDeleteResult
{
    public int TotalRequested { get; set; }
    public int DeletedCount { get; set; }
    public int NotFoundCount { get; set; }
    public int FailedCount { get; set; }
    public List<Guid> FailedIds { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool IsSuccess => FailedCount == 0;
}

/// <summary>
/// Validator for BatchDeleteGenericCommand
/// </summary>
public class BatchDeleteGenericCommandValidator : AbstractValidator<BatchDeleteGenericCommand>
{
    public BatchDeleteGenericCommandValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type is required");

        RuleFor(x => x.Ids)
            .NotEmpty().WithMessage("At least one ID is required")
            .Must(ids => ids.Count() <= 100).WithMessage("Maximum 100 items can be deleted at once");
    }
}
