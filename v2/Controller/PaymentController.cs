using v2.Core.DTOs;
using v2.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace V2.Controllers
{
    [ApiController]
    [Route("payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPayment _paymentService;

        public PaymentController(IPayment paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDTO request)
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            if (request == null)
                return StatusCode(StatusCodes.Status400BadRequest, new { error = "Request must contain a body." });

            if (request.Amount <= 0)
                return StatusCode(StatusCodes.Status400BadRequest, new { error = "Required field missing, field: amount" });

            var result = await _paymentService.CreatePaymentAsync(request, identityUserId);

            return result.statusCode switch
            {
                201 => StatusCode(StatusCodes.Status201Created, result.data),
                404 => StatusCode(StatusCodes.Status404NotFound, result.data),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result.data)
            };
        }

        [HttpPut("{paymentId:guid}")]
        [Consumes("application/json")]
        public async Task<IActionResult> ConfirmPayment(Guid paymentId, [FromBody] ConfirmPaymentRequestDTO request)
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            if (request == null)
                return StatusCode(StatusCodes.Status400BadRequest, new { error = "Request must contain a body." });

            if (string.IsNullOrEmpty(request.Validation))
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    error = "Required field missing, field: validation"
                });

            if (request.T_Data == null)
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    error = "Required field missing, field: t_data"
                });

            var result = await _paymentService.ConfirmPaymentAsync(paymentId, request, identityUserId);
            return result.statusCode switch
            {
                200 => StatusCode(StatusCodes.Status200OK, result.data),
                401 => StatusCode(StatusCodes.Status401Unauthorized, result.data),
                403 => StatusCode(StatusCodes.Status403Forbidden, result.data),
                404 => StatusCode(StatusCodes.Status404NotFound, result.data),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result.data)
            };
        }

        [HttpPost("refund")]
        public async Task<IActionResult> RefundPayment([FromBody] RefundPaymentRequestDTO request)
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            if (request == null || request.PaymentId == Guid.Empty)
                return StatusCode(StatusCodes.Status400BadRequest, new { error = "Required field missing, field: paymentId" });

            var result = await _paymentService.RefundPaymentAsync(request, identityUserId);

            return result.statusCode switch
            {
                201 => StatusCode(StatusCodes.Status201Created, result.data),
                403 => StatusCode(StatusCodes.Status403Forbidden, result.data),
                404 => StatusCode(StatusCodes.Status404NotFound, result.data),
                409 => StatusCode(StatusCodes.Status409Conflict, result.data),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result.data)
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            var result = await _paymentService.GetPaymentsByUserAsync(null, identityUserId);

            return result.statusCode switch
            {
                200 => StatusCode(StatusCodes.Status200OK, result.data),
                404 => StatusCode(StatusCodes.Status404NotFound, result.data),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result.data)
            };
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetPaymentsByUsername(string username)
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            var result = await _paymentService.GetPaymentsByUserAsync(username, identityUserId);

            return result.statusCode switch
            {
                200 => StatusCode(StatusCodes.Status200OK, result.data),
                403 => StatusCode(StatusCodes.Status403Forbidden, result.data),
                404 => StatusCode(StatusCodes.Status404NotFound, result.data),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result.data)
            };
        }

        [HttpPatch("{paymentId:guid}")]
        public async Task<IActionResult> PartialUpdatePayment(Guid paymentId, [FromBody] UpdatePaymentRequestDTO dto)
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            var result = await _paymentService.UpdatePaymentAsync(paymentId, dto, identityUserId);

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