namespace ERP.Domain.Projects.Enums;

/// <summary>
/// Project status
/// </summary>
public enum ProjectStatus
{
    Planning = 1,
    Active = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5
}

/// <summary>
/// Task status (renamed to avoid Task/Task<T> ambiguity)
/// </summary>
public enum ProjectTaskStatus
{
    Todo = 1,
    InProgress = 2,
    Review = 3,
    Done = 4,
    Cancelled = 5
}

/// <summary>
/// Task priority
/// </summary>
public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
