using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using v2.core.Interfaces;
using v2.Core.Interfaces;

namespace V2.Controllers
{
    [ApiController]
    [Route("billing")]
    public class BillingController : ControllerBase
    {
        private readonly IBilling _billingService;

        public BillingController(IBilling billingService)
        {
            _billingService = billingService;
        }

        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetInvoiceHistory()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized(new { error = "User not logged in" });

            var userId = Guid.Parse(userIdStr);

            var (statusCode, data) = await _billingService.GetInvoiceHistoryAsync(userId);
            return StatusCode(statusCode, data);
        }

       
    }
        
    }

