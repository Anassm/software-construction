using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using v2.Core.Interfaces;
using v2.core.Interfaces;
using v2.infrastructure.Services;

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
}