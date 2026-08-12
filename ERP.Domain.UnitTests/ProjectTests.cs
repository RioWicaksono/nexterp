using Xunit;
using ERP.Domain.Projects.Entities;
using ERP.Domain.Projects.Enums;

namespace ERP.Domain.UnitTests;

/// <summary>
/// Unit tests for Project entity
/// </summary>
public class ProjectTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateProject()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var name = "Test Project";

        // Act
        var project = Project.Create(orgId, name, "PRJ001");

        // Assert
        Assert.Equal(orgId, project.OrganizationId);
        Assert.Equal(name, project.Name);
        Assert.Equal("PRJ001", project.Code);
        Assert.Equal(ProjectStatus.Planning, project.Status);
        Assert.False(project.IsDeleted);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Project.Create(Guid.NewGuid(), ""));
    }

    [Fact]
    public void Start_ShouldSetActiveStatus()
    {
        // Arrange
        var project = Project.Create(Guid.NewGuid(), "Test Project");

        // Act
        project.Start();

        // Assert
        Assert.Equal(ProjectStatus.Active, project.Status);
        Assert.NotNull(project.ActualStartDate);
    }

    [Fact]
    public void Start_AlreadyActive_ShouldThrowException()
    {
        // Arrange
        var project = Project.Create(Guid.NewGuid(), "Test Project");
        project.Start();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => project.Start());
    }

    [Fact]
    public void Complete_ShouldSetCompletedStatus()
    {
        // Arrange
        var project = Project.Create(Guid.NewGuid(), "Test Project");
        project.Start();

        // Act
        project.Complete();

        // Assert
        Assert.Equal(ProjectStatus.Completed, project.Status);
        Assert.NotNull(project.ActualEndDate);
    }

    [Fact]
    public void Cancel_ShouldSetCancelledStatus()
    {
        // Arrange
        var project = Project.Create(Guid.NewGuid(), "Test Project");

        // Act
        project.Cancel();

        // Assert
        Assert.Equal(ProjectStatus.Cancelled, project.Status);
    }

    [Fact]
    public void AddTask_ShouldIncrementTotalTasks()
    {
        // Arrange
        var project = Project.Create(Guid.NewGuid(), "Test Project");
        var task = ProjectTask.Create(
            Guid.NewGuid(), project.Id, "Test Task");

        // Act
        project.AddTask(task);

        // Assert
        Assert.Equal(1, project.TotalTasks);
    }

    [Fact]
    public void Progress_ShouldCalculateCorrectly()
    {
        // Arrange
        var project = Project.Create(Guid.NewGuid(), "Test Project");
        var task1 = ProjectTask.Create(Guid.NewGuid(), project.Id, "Task 1");
        var task2 = ProjectTask.Create(Guid.NewGuid(), project.Id, "Task 2");
        task1.Complete();
        project.AddTask(task1);
        project.AddTask(task2);

        // Assert
        Assert.Equal(2, project.TotalTasks);
        Assert.Equal(1, project.CompletedTasks);
    }
}

/// <summary>
/// Unit tests for ProjectTask entity
/// </summary>
public class ProjectTaskTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateTask()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        // Act
        var task = ProjectTask.Create(orgId, projectId, "Test Task");

        // Assert
        Assert.Equal("Test Task", task.Title);
        Assert.Equal(ProjectTaskStatus.Todo, task.Status);
        Assert.Equal(TaskPriority.Medium, task.Priority);
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), ""));
    }

    [Fact]
    public void Start_ShouldSetInProgressStatus()
    {
        // Arrange
        var task = ProjectTask.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Test Task");

        // Act
        task.Start();

        // Assert
        Assert.Equal(ProjectTaskStatus.InProgress, task.Status);
        Assert.NotNull(task.StartDate);
    }

    [Fact]
    public void Start_AlreadyInProgress_ShouldThrowException()
    {
        // Arrange
        var task = ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Task");
        task.Start();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => task.Start());
    }

    [Fact]
    public void Complete_ShouldSetDoneStatus()
    {
        // Arrange
        var task = ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Task");
        task.Start();

        // Act
        task.Complete();

        // Assert
        Assert.Equal(ProjectTaskStatus.Done, task.Status);
        Assert.Equal(100, task.Progress);
        Assert.NotNull(task.CompletedDate);
    }

    [Fact]
    public void MoveToReview_ShouldSetReviewStatus()
    {
        // Arrange
        var task = ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Task");
        task.Start();

        // Act
        task.MoveToReview();

        // Assert
        Assert.Equal(ProjectTaskStatus.Review, task.Status);
    }

    [Fact]
    public void AssignTo_ShouldSetAssignee()
    {
        // Arrange
        var task = ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Task");
        var employeeId = Guid.NewGuid();

        // Act
        task.AssignTo(employeeId);

        // Assert
        Assert.Equal(employeeId, task.AssignedToId);
    }

    [Fact]
    public void Unassign_ShouldClearAssignee()
    {
        // Arrange
        var task = ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Task");
        task.AssignTo(Guid.NewGuid());

        // Act
        task.Unassign();

        // Assert
        Assert.Null(task.AssignedToId);
    }

    [Fact]
    public void SetProgress_ShouldUpdateProgress()
    {
        // Arrange
        var task = ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Task");

        // Act
        task.SetProgress(50);

        // Assert
        Assert.Equal(50, task.Progress);
        Assert.Equal(ProjectTaskStatus.InProgress, task.Status);
    }

    [Fact]
    public void SetProgress_100_ShouldSetDone()
    {
        // Arrange
        var task = ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Task");

        // Act
        task.SetProgress(100);

        // Assert
        Assert.Equal(100, task.Progress);
        Assert.Equal(ProjectTaskStatus.Done, task.Status);
    }

    [Fact]
    public void LogHours_ShouldAddHours()
    {
        // Arrange
        var task = ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Task");

        // Act
        task.LogHours(4.5m);
        task.LogHours(2.5m);

        // Assert
        Assert.Equal(7, task.ActualHours);
    }

    [Fact]
    public void LogHours_Negative_ShouldThrowException()
    {
        // Arrange
        var task = ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Task");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => task.LogHours(-5));
    }

    [Fact]
    public void IsOverdue_WhenPastDueDate_ShouldReturnTrue()
    {
        // Arrange
        var task = ProjectTask.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Test Task",
            dueDate: DateTime.UtcNow.AddDays(-1));

        // Assert
        Assert.True(task.IsOverdue);
    }

    [Fact]
    public void IsOverdue_WhenDone_ShouldReturnFalse()
    {
        // Arrange
        var task = ProjectTask.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Test Task",
            dueDate: DateTime.UtcNow.AddDays(-1));
        task.Complete();

        // Assert
        Assert.False(task.IsOverdue);
    }
}
