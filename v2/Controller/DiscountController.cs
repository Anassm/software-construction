using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using v2.Core.DTOs;
using v2.Core.Interfaces;

namespace v2.Controllers
{
    [ApiController]
    [Route("discounts")]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscounts _discountService;

        public DiscountController(IDiscounts discountService)
        {
            _discountService = discountService;
        }

        private string? GetIdentityUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DiscountCreateRequest dto)
        {
            var identityUserId = GetIdentityUserId();
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            if (dto == null)
                return StatusCode(StatusCodes.Status400BadRequest, new { error = "Request must contain a body." });

            var result = await _discountService.CreateAsync(dto, identityUserId);

            return result.statusCode switch
            {
                201 => StatusCode(StatusCodes.Status201Created, result.data),
                400 => StatusCode(StatusCodes.Status400BadRequest, result.data),
                403 => StatusCode(StatusCodes.Status403Forbidden, result.data),
                404 => StatusCode(StatusCodes.Status404NotFound, result.data),
                409 => StatusCode(StatusCodes.Status409Conflict, result.data),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result.data)
            };
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DiscountUpdateRequest dto)
        {
            var identityUserId = GetIdentityUserId();
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            var result = await _discountService.UpdateAsync(id, dto, identityUserId);

            return result.statusCode switch
            {
                200 => StatusCode(StatusCodes.Status200OK, result.data),
                400 => StatusCode(StatusCodes.Status400BadRequest, result.data),
                403 => StatusCode(StatusCodes.Status403Forbidden, result.data),
                404 => StatusCode(StatusCodes.Status404NotFound, result.data),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result.data)
            };
        }

        [HttpPut("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var identityUserId = GetIdentityUserId();
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            var result = await _discountService.DeactivateAsync(id, identityUserId);

            return result.statusCode switch
            {
                200 => StatusCode(StatusCodes.Status200OK, result.data),
                403 => StatusCode(StatusCodes.Status403Forbidden, result.data),
                404 => StatusCode(StatusCodes.Status404NotFound, result.data),
                409 => StatusCode(StatusCodes.Status409Conflict, result.data),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result.data)
            };
        }

        [HttpPut("{id:guid}/expiry")]
        public async Task<IActionResult> UpdateExpiry(Guid id, [FromBody] DateTime? expiryDate)
        {
            var identityUserId = GetIdentityUserId();
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            var result = await _discountService.UpdateExpiryAsync(id, expiryDate, identityUserId);

            return result.statusCode switch
            {
                200 => StatusCode(StatusCodes.Status200OK, result.data),
                403 => StatusCode(StatusCodes.Status403Forbidden, result.data),
                404 => StatusCode(StatusCodes.Status404NotFound, result.data),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result.data)
            };
        }

        [HttpPost("{id:guid}/links")]
        public async Task<IActionResult> LinkUsers(Guid id, [FromBody] DiscountLinkUsersRequest dto)
        {
            var identityUserId = GetIdentityUserId();
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            if (dto == null)
                return StatusCode(StatusCodes.Status400BadRequest, new { error = "Request must contain a body." });

            var result = await _discountService.LinkUsersAsync(id, dto, identityUserId);

            return result.statusCode switch
            {
                200 => StatusCode(StatusCodes.Status200OK, result.data),
                400 => StatusCode(StatusCodes.Status400BadRequest, result.data),
                403 => StatusCode(StatusCodes.Status403Forbidden, result.data),
                404 => StatusCode(StatusCodes.Status404NotFound, result.data),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result.data)
            };
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] DiscountApplyRequest dto)
        {
            var identityUserId = GetIdentityUserId();
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            var result = await _discountService.ValidateAndApplyAsync(dto, identityUserId);

            return result.statusCode switch
            {
                200 => StatusCode(StatusCodes.Status200OK, result.data),
                400 => StatusCode(StatusCodes.Status400BadRequest, result.data),
                403 => StatusCode(StatusCodes.Status403Forbidden, result.data),
                404 => StatusCode(StatusCodes.Status404NotFound, result.data),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result.data)
            };
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var result = await _discountService.GetStatisticsAsync();
            return result.statusCode switch
            {
                200 => StatusCode(StatusCodes.Status200OK, result.data),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result.data)
            };
        }
        
        [HttpGet("active")]
        public async Task<IActionResult> GetAllActiveCodes()
        {
            var identityUserId = GetIdentityUserId();
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            var result = await _discountService.GetAllActiveCodesAsync(identityUserId);

            return result.statusCode switch
            {
                200 => StatusCode(StatusCodes.Status200OK, result.data),
                403 => StatusCode(StatusCodes.Status403Forbidden, result.data),
                404 => StatusCode(StatusCodes.Status404NotFound, result.data),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result.data)
            };
        }

        [HttpGet("statistics/{filter}/{orderby}")]
        public async Task<IActionResult> GetStatisticsFilter()
        {
            var filter = "";
            switch (HttpContext.Request.RouteValues["filter"]?.ToString())  
            {
                case "totaluses":
                    filter = "totalUses";
                    break;
                case "remaininguses":
                    filter = "remainingUses";
                    break;
                case "totalsavedamount":
                    filter = "totalSavedAmount";
                    break;
                default:
                    return StatusCode(StatusCodes.Status400BadRequest, new { error = "Invalid filter parameter." });
            };
            var orderby = "";
            switch (HttpContext.Request.RouteValues["orderby"]?.ToString())  
            {
                case "asc":
                    orderby = "asc";
                    break;
                case "desc":
                    orderby = "desc";
                    break;
                default:
                    return StatusCode(StatusCodes.Status400BadRequest, new { error = "Invalid filter parameter." });
            };
            var result = await _discountService.GetStatisticsAsync(filter, orderby);
            return result.statusCode switch
            {
                200 => StatusCode(StatusCodes.Status200OK, result.data),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result.data)
            };
        }
        
        [HttpGet("used")]
        public async Task<IActionResult> GetUsedCodes([FromQuery] Guid? discountCodeId)
        {
            var identityUserId = GetIdentityUserId();
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            var result = await _discountService.GetUsedCodesAsync(
                discountCodeId,
                identityUserId
            );

            return result.statusCode switch
            {
                200 => StatusCode(StatusCodes.Status200OK, result.data),
                403 => StatusCode(StatusCodes.Status403Forbidden, result.data),
                404 => StatusCode(StatusCodes.Status404NotFound, result.data),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result.data)
            };
        }

    }
}
