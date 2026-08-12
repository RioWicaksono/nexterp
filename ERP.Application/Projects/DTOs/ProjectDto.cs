using ERP.Application.Common.DTOs;
using ERP.Domain.Projects.Enums;

namespace ERP.Application.Projects.DTOs;

/// <summary>
/// Project DTO
/// </summary>
public class ProjectDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public Guid? ProjectManagerId { get; set; }
    public string? ProjectManagerName { get; set; }
    public decimal Progress { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
}

/// <summary>
/// Project Task DTO
/// </summary>
public class ProjectTaskDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public Guid? ParentTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public decimal Progress { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public bool IsOverdue { get; set; }
    public int SubTaskCount { get; set; }
}

/// <summary>
/// DTO for creating a project
/// </summary>
public class CreateProjectDto
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public Guid? ProjectManagerId { get; set; }
}

/// <summary>
/// DTO for creating a task
/// </summary>
public class CreateProjectTaskDto
{
    public Guid OrganizationId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? ParentTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal EstimatedHours { get; set; }
    public Guid? AssignedToId { get; set; }
}

/// <summary>
/// DTO for updating task status
/// </summary>
public class UpdateTaskStatusDto
{
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// DTO for logging hours
/// </summary>
public class LogTaskHoursDto
{
    public decimal Hours { get; set; }
}

/// <summary>
/// DTO for assigning task
/// </summary>
public class AssignTaskDto
{
    public Guid? EmployeeId { get; set; }
}
