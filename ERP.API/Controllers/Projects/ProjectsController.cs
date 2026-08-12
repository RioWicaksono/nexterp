using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Controllers.Base;
using ERP.Application.Projects.Commands;
using ERP.Application.Projects.DTOs;
using ERP.Application.Projects.Queries;

namespace ERP.API.Controllers.Projects;

/// <summary>
/// Project management endpoints
/// </summary>
[ApiController]
[Route("api/v1/projects")]
[Authorize]
public class ProjectsController : BaseApiController
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all projects
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProjectDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjects(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetProjectsQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get project by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProjectByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto request, CancellationToken cancellationToken)
    {
        var command = new CreateProjectCommand
        {
            OrganizationId = request.OrganizationId,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Budget = request.Budget,
            ProjectManagerId = request.ProjectManagerId
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Created($"api/v1/projects/{result.Value}", result);

        return HandleResult(result);
    }

    /// <summary>
    /// Update project
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateProjectDto request, CancellationToken cancellationToken)
    {
        // TODO: Implement update
        return Error("Not implemented yet", StatusCodes.Status501NotImplemented);
    }

    /// <summary>
    /// Start project
    /// </summary>
    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement
        return Error("Not implemented yet", StatusCodes.Status501NotImplemented);
    }

    /// <summary>
    /// Complete project
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement
        return Error("Not implemented yet", StatusCodes.Status501NotImplemented);
    }

    /// <summary>
    /// Cancel project
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement
        return Error("Not implemented yet", StatusCodes.Status501NotImplemented);
    }
}

/// <summary>
/// Project Tasks endpoints
/// </summary>
[ApiController]
[Route("api/v1/project-tasks")]
[Authorize]
public class ProjectTasksController : BaseApiController
{
    private readonly IMediator _mediator;

    public ProjectTasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get tasks by project
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProjectTaskDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks(
        [FromQuery] Guid? projectId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetProjectTasksQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateProjectTaskDto request, CancellationToken cancellationToken)
    {
        var command = new CreateProjectTaskCommand
        {
            OrganizationId = request.OrganizationId,
            ProjectId = request.ProjectId,
            ParentTaskId = request.ParentTaskId,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            EstimatedHours = request.EstimatedHours,
            AssignedToId = request.AssignedToId
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Created($"api/v1/project-tasks/{result.Value}", result);

        return HandleResult(result);
    }

    /// <summary>
    /// Update task status
    /// </summary>
    [HttpPost("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateTaskStatusCommand
        {
            TaskId = id,
            Status = request.Status
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Assign task
    /// </summary>
    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignTaskDto request, CancellationToken cancellationToken)
    {
        // TODO: Implement
        return Error("Not implemented yet", StatusCodes.Status501NotImplemented);
    }
}
