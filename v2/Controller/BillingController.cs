using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using v2.Core.Interfaces;
using v2.core.Interfaces;
using v2.infrastructure.Services;
using v2.Core.DTOs;

namespace V2.Controllers;

[ApiController]
[Route("billing")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IBilling _billingService;

    public BillingController(IBilling billingService) 
    {
        _billingService = billingService;
    }

    
    [HttpGet("invoices")]
    public async Task<IActionResult> GetMyInvoiceHistory()
    {
       
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        
        if (string.IsNullOrEmpty(identityUserId))
            return StatusCode(StatusCodes.Status401Unauthorized, 
                new { error = "Unauthorized: Invalid or missing session token" });

        
        var (statusCode, message) = await _billingService.GetMyInvoiceHistoryAsync(identityUserId);

        return statusCode switch
        {
            200 => Ok(message),
            404 => NotFound(message),
            _ => StatusCode(statusCode, message)
        };
    }



[HttpGet("invoices/{invoiceId:guid}")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> GetInvoiceDetails(Guid invoiceId)
    {
  
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        
        if (string.IsNullOrEmpty(identityUserId))
            return StatusCode(StatusCodes.Status401Unauthorized, 
                new { error = "Unauthorized: Invalid or missing session token" });

      
        var (statusCode, message) = await _billingService.GetInvoiceDetailsAsync(invoiceId, identityUserId);

        return statusCode switch
        {
            200 => Ok(message),
            403 => StatusCode(StatusCodes.Status403Forbidden, message),
            404 => NotFound(message),
            _ => StatusCode(statusCode, message)
        };
    }

[HttpPost("invoices/bundle")]
    [Authorize(Roles = "Admin,Employee,Business")]
    public async Task<IActionResult> CreateBundleInvoice([FromBody] CreateBundleInvoiceDto dto)
    {
        
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        
        if (string.IsNullOrEmpty(identityUserId))
            return StatusCode(StatusCodes.Status401Unauthorized, 
                new { error = "Unauthorized: Invalid or missing session token" });

        
        if (dto == null)
            return BadRequest(new { error = "Request body is required" });

        if (dto.SessionIds == null || !dto.SessionIds.Any())
            return BadRequest(new { error = "At least one session ID is required" });

       
        var (statusCode, message) = await _billingService.CreateBundleInvoiceAsync(dto, identityUserId);

        return statusCode switch
        {
            201 => StatusCode(StatusCodes.Status201Created, message),
            400 => BadRequest(message),
            403 => StatusCode(StatusCodes.Status403Forbidden, message),
            404 => NotFound(message),
            409 => Conflict(message),
            _ => StatusCode(statusCode, message)
        };
}

}