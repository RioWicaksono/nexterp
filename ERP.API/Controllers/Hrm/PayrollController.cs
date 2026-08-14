using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Controllers.Base;
using ERP.Application.Common.Models;
using ERP.Application.Common.Behaviors;
using ERP.Application.Hrm.Commands.Payroll;
using ERP.Application.Hrm.Queries.Payroll;
using ERP.Domain.Hrm.Enums;
using Asp.Versioning;

namespace ERP.API.Controllers.Hrm;

/// <summary>
/// Payroll management endpoints.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/payroll")]
[Authorize]
[RequiresModule("HRM")]
public class PayrollController : BaseApiController
{
    private readonly IMediator _mediator;

    public PayrollController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get paginated payroll list.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPayrollList(
        [FromQuery] Guid organizationId,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] PayrollStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetPayrollListQuery(organizationId, year, month, employeeId, status, page, pageSize);
        var result = await _mediator.Send(query);
        return Success(result);
    }

    /// <summary>
    /// Get payroll by ID.
    /// </summary>
    [HttpGet("{payrollId}")]
    public async Task<IActionResult> GetPayrollById(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid payrollId)
    {
        var query = new GetPayrollByIdQuery(organizationId, payrollId);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFoundError("Payroll not found");

        return Success(result);
    }

    /// <summary>
    /// Get payroll summary for a period.
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetPayrollSummary(
        [FromQuery] Guid organizationId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var query = new GetPayrollSummaryQuery(organizationId, year, month);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFoundError("No payroll found for the specified period");

        return Success(result);
    }

    /// <summary>
    /// Calculate payroll preview for an employee.
    /// </summary>
    [HttpPost("preview")]
    public async Task<IActionResult> CalculatePreview([FromBody] CalculatePayrollPreviewCommand command)
    {
        var result = await _mediator.Send(command);
        return Success(result);
    }

    /// <summary>
    /// Create payroll for a single employee.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePayroll([FromBody] CreatePayrollCommand command)
    {
        try
        {
            var payrollId = await _mediator.Send(command);
            return Created($"/api/v1/payroll/{payrollId}", payrollId);
        }
        catch (InvalidOperationException ex)
        {
            return ValidationError(ex.Message);
        }
    }

    /// <summary>
    /// Create batch payroll for department/organization.
    /// </summary>
    [HttpPost("batch")]
    public async Task<IActionResult> CreateBatchPayroll([FromBody] CreateBatchPayrollCommand command)
    {
        var result = await _mediator.Send(command);
        return Success(result);
    }

    /// <summary>
    /// Approve payroll for payment.
    /// </summary>
    [HttpPost("{payrollId}/approve")]
    public async Task<IActionResult> ApprovePayroll(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid payrollId,
        [FromBody] ApprovePayrollRequest request)
    {
        try
        {
            var command = new ApprovePayrollCommand(organizationId, payrollId, request.ApprovedBy);
            var result = await _mediator.Send(command);
            return Success(result);
        }
        catch (InvalidOperationException ex)
        {
            return ValidationError(ex.Message);
        }
    }

    /// <summary>
    /// Mark payroll as paid.
    /// </summary>
    [HttpPost("{payrollId}/pay")]
    public async Task<IActionResult> MarkPayrollPaid(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid payrollId,
        [FromBody] MarkPayrollPaidRequest request)
    {
        try
        {
            var command = new MarkPayrollPaidCommand(organizationId, payrollId, request.PaymentDate, request.PaidBy);
            var result = await _mediator.Send(command);
            return Success(result);
        }
        catch (InvalidOperationException ex)
        {
            return ValidationError(ex.Message);
        }
    }

    /// <summary>
    /// Delete draft payroll.
    /// </summary>
    [HttpDelete("{payrollId}")]
    public async Task<IActionResult> DeletePayroll(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid payrollId,
        [FromBody] DeletePayrollRequest request)
    {
        try
        {
            var command = new DeletePayrollCommand(organizationId, payrollId, request.DeletedBy);
            var result = await _mediator.Send(command);
            return Success(result);
        }
        catch (InvalidOperationException ex)
        {
            return ValidationError(ex.Message);
        }
    }

    /// <summary>
    /// Get payslip for employee.
    /// </summary>
    [HttpGet("{payrollId}/payslip")]
    public async Task<IActionResult> GetPayslip(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid payrollId)
    {
        var query = new GetPayslipQuery(organizationId, payrollId);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFoundError("Payslip not found");

        return Success(result);
    }

    /// <summary>
    /// Get employee payroll history.
    /// </summary>
    [HttpGet("history/{employeeId}")]
    public async Task<IActionResult> GetEmployeePayrollHistory(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid employeeId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var query = new GetEmployeePayrollHistoryQuery(organizationId, employeeId, page, pageSize);
        var result = await _mediator.Send(query);
        return Success(result);
    }
}

#region Request DTOs
public record ApprovePayrollRequest(string ApprovedBy);
public record MarkPayrollPaidRequest(DateTime PaymentDate, string PaidBy);
public record DeletePayrollRequest(string DeletedBy);
#endregion
