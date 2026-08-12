using ERP.Domain.Common;
using ERP.Domain.Projects.Enums;

namespace ERP.Domain.Projects.Entities;

/// <summary>
/// Project entity
/// </summary>
public class Project : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public string? Description { get; private set; }
    public ProjectStatus Status { get; private set; } = ProjectStatus.Planning;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? ActualStartDate { get; private set; }
    public DateTime? ActualEndDate { get; private set; }
    public decimal? Budget { get; private set; }
    public Guid? ProjectManagerId { get; private set; }
    public bool IsTemplate { get; private set; }

    // Navigation properties
    private readonly List<ProjectTask> _tasks = new();
    public IReadOnlyCollection<ProjectTask> Tasks => _tasks.AsReadOnly();

    // Calculated properties
    public decimal Progress => Tasks.Count == 0 ? 0 : (decimal)Tasks.Count(t => t.Status == ProjectTaskStatus.Done) / Tasks.Count * 100;
    public int TotalTasks => Tasks.Count;
    public int CompletedTasks => Tasks.Count(t => t.Status == ProjectTaskStatus.Done);
    public int OverdueTasks => Tasks.Count(t => t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != ProjectTaskStatus.Done);

    // Factory method
    public static Project Create(
        Guid organizationId,
        string name,
        string? code = null,
        string? description = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        decimal? budget = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required", nameof(name));

        return new Project
        {
            OrganizationId = organizationId,
            Name = name.Trim(),
            Code = code?.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            Budget = budget
        };
    }

    public void Update(
        string? name = null,
        string? description = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        decimal? budget = null)
    {
        Name = name?.Trim() ?? Name;
        Description = description?.Trim() ?? Description;
        StartDate = startDate ?? StartDate;
        EndDate = endDate ?? EndDate;
        Budget = budget ?? Budget;
        UpdateTimestamp();
    }

    public void SetProjectManager(Guid employeeId)
    {
        ProjectManagerId = employeeId;
        UpdateTimestamp();
    }

    public void SetStatus(ProjectStatus status)
    {
        Status = status;

        if (status == ProjectStatus.Active && !ActualStartDate.HasValue)
            ActualStartDate = DateTime.UtcNow;

        if (status == ProjectStatus.Completed)
            ActualEndDate = DateTime.UtcNow;

        UpdateTimestamp();
    }

    public void Start()
    {
        if (Status != ProjectStatus.Planning && Status != ProjectStatus.OnHold)
            throw new InvalidOperationException("Can only start planning or on-hold projects");

        Status = ProjectStatus.Active;
        ActualStartDate = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void Complete()
    {
        Status = ProjectStatus.Completed;
        ActualEndDate = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void Cancel()
    {
        Status = ProjectStatus.Cancelled;
        UpdateTimestamp();
    }

    public void Hold()
    {
        Status = ProjectStatus.OnHold;
        UpdateTimestamp();
    }

    public void AddTask(ProjectTask task)
    {
        _tasks.Add(task);
        UpdateTimestamp();
    }

    public void RemoveTask(Guid taskId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task != null)
        {
            _tasks.Remove(task);
            UpdateTimestamp();
        }
    }
}

/// <summary>
/// Project task entity
/// </summary>
public class ProjectTask : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid? ParentTaskId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProjectTaskStatus Status { get; private set; } = ProjectTaskStatus.Todo;
    public TaskPriority Priority { get; private set; } = TaskPriority.Medium;
    public DateTime? StartDate { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public decimal EstimatedHours { get; private set; }
    public decimal ActualHours { get; private set; }
    public decimal Progress { get; private set; }
    public Guid? AssignedToId { get; private set; }
    public Guid? MilestoneId { get; private set; }
    public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.UtcNow && Status != ProjectTaskStatus.Done;

    // Navigation properties
    private readonly Project? _project;
    public Project? Project => _project;

    private readonly ProjectTask? _parentTask;
    public ProjectTask? ParentTask => _parentTask;

    private readonly List<ProjectTask> _subTasks = new();
    public IReadOnlyCollection<ProjectTask> SubTasks => _subTasks.AsReadOnly();

    // Factory method
    public static ProjectTask Create(
        Guid organizationId,
        Guid projectId,
        string title,
        string? description = null,
        TaskPriority priority = TaskPriority.Medium,
        DateTime? dueDate = null,
        decimal estimatedHours = 0)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title is required", nameof(title));

        return new ProjectTask
        {
            OrganizationId = organizationId,
            ProjectId = projectId,
            Title = title.Trim(),
            Description = description?.Trim(),
            Priority = priority,
            DueDate = dueDate,
            EstimatedHours = estimatedHours
        };
    }

    public void Update(
        string? title = null,
        string? description = null,
        TaskPriority? priority = null,
        DateTime? startDate = null,
        DateTime? dueDate = null,
        decimal? estimatedHours = null)
    {
        Title = title?.Trim() ?? Title;
        Description = description?.Trim() ?? Description;
        Priority = priority ?? Priority;
        StartDate = startDate ?? StartDate;
        DueDate = dueDate ?? DueDate;
        EstimatedHours = estimatedHours ?? EstimatedHours;
        UpdateTimestamp();
    }

    public void AssignTo(Guid employeeId)
    {
        AssignedToId = employeeId;
        UpdateTimestamp();
    }

    public void Unassign()
    {
        AssignedToId = null;
        UpdateTimestamp();
    }

    public void SetParent(Guid? parentTaskId)
    {
        ParentTaskId = parentTaskId;
        UpdateTimestamp();
    }

    public void SetMilestone(Guid? milestoneId)
    {
        MilestoneId = milestoneId;
        UpdateTimestamp();
    }

    public void Start()
    {
        if (Status != ProjectTaskStatus.Todo)
            throw new InvalidOperationException("Can only start todo tasks");

        Status = ProjectTaskStatus.InProgress;
        StartDate = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void MoveToReview()
    {
        if (Status != ProjectTaskStatus.InProgress)
            throw new InvalidOperationException("Can only review in-progress tasks");

        Status = ProjectTaskStatus.Review;
        UpdateTimestamp();
    }

    public void Complete()
    {
        Status = ProjectTaskStatus.Done;
        CompletedDate = DateTime.UtcNow;
        Progress = 100;
        UpdateTimestamp();
    }

    public void Cancel()
    {
        Status = ProjectTaskStatus.Cancelled;
        UpdateTimestamp();
    }

    public void SetProgress(decimal progress)
    {
        if (progress < 0 || progress > 100)
            throw new ArgumentException("Progress must be between 0 and 100");

        Progress = progress;
        if (progress == 100)
            Status = ProjectTaskStatus.Done;
        else if (progress > 0 && Status == ProjectTaskStatus.Todo)
            Status = ProjectTaskStatus.InProgress;
        UpdateTimestamp();
    }

    public void LogHours(decimal hours)
    {
        if (hours < 0)
            throw new ArgumentException("Hours cannot be negative");

        ActualHours += hours;
        UpdateTimestamp();
    }
}
