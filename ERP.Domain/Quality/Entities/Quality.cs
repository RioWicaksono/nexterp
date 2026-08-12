namespace ERP.Domain.Quality.Entities;

/// <summary>
/// Quality inspection entity
/// </summary>
public class Inspection
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Non-conformance report
/// </summary>
public class NonConformance
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? InspectionId { get; set; }
    public string Severity { get; set; } = "Low";
    public string Description { get; set; } = string.Empty;
    public string RootCause { get; set; } = string.Empty;
    public string CorrectiveAction { get; set; } = string.Empty;
    public string PreventiveAction { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
