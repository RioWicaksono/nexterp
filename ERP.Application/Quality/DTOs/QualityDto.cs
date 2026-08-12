namespace ERP.Application.Quality.DTOs;

public class InspectionDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string InspectionNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public DateTime InspectionDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Inspector { get; set; }
    public string Results { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NonConformanceDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? InspectionId { get; set; }
    public string Severity { get; set; } = "Low";
    public string Description { get; set; } = string.Empty;
    public string? RootCause { get; set; }
    public string? CorrectiveAction { get; set; }
    public string? PreventiveAction { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Command DTOs
public record CreateInspectionCommand(
    string Type,
    Guid? ReferenceId,
    string ReferenceType,
    DateTime InspectionDate,
    string? Inspector,
    string? Notes);

public record CompleteInspectionCommand(
    Guid Id,
    string Results,
    bool Passed,
    string? Notes);

public record CreateNonConformanceCommand(
    Guid? InspectionId,
    string Severity,
    string Description,
    string? RootCause,
    string? CorrectiveAction,
    string? PreventiveAction);

public record ResolveNonConformanceCommand(
    Guid Id,
    string? RootCause,
    string? CorrectiveAction,
    string? PreventiveAction);
