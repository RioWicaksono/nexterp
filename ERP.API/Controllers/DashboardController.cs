using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Controllers.Base;
using ERP.Application.Common.Queries.Dashboard;
using Asp.Versioning;

namespace ERP.API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public class DashboardController : BaseApiController
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get dashboard statistics.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var query = new GetDashboardStatsQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}
