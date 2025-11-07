using Microsoft.AspNetCore.Mvc;
using v2.Core.DTOs;
using v2.Core.Interfaces;

namespace V2.Controllers;

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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _paymentService.CreatePaymentAsync(request);

        if (result is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create payment.");
        }

        var response = new { status = "Success", payment = result };

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{paymentId}")]
    public async Task<IActionResult> UpdatePayment(Guid paymentId, [FromBody] ConfirmPaymentRequestDTO confirmRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var confirmedPayment = await _paymentService.ConfirmPaymentAsync(paymentId, confirmRequest.Validation);

        if (confirmedPayment != null)
        {
            var response = new { status = "Success", payment = confirmedPayment };
            return Ok(response);
        }

        if (confirmedPayment == null)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, new
            {
                error = "Validation failed",
                info = "The validation of the security hash could not be validated for this transaction."
            });
        }

        return NotFound($"Payment with ID {paymentId} not found.");
    }

    [HttpPost("refund")]
    public async Task<IActionResult> RefundPayment([FromBody] RefundPaymentRequestDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _paymentService.RefundPaymentAsync(request.PaymentId, request.Reason);

        if (result == null)
        {
            return BadRequest("Refund failed (e.g., payment ID not found or already refunded).");
        }

        var response = new { status = "Success", payment = result };
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaymentsForCurrentUser()
    {
        // TODO: Vervang dit met de geauthenticeerde gebruiker.
        string initiator = "test_user_v2";

        var payments = await _paymentService.GetPaymentsByUserAsync(initiator);

        if (payments == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve payments.");
        }
        return Ok(payments);
    }

    [HttpGet("{initiator}")]
    public async Task<IActionResult> GetPaymentsByInitiator(string initiator)
    {
        var payments = await _paymentService.GetPaymentsByUserAsync(initiator);

        if (payments == null)
        {
            return NotFound($"No payments found for initiator: {initiator}");
        }

        return Ok(payments);
    }

    [HttpPatch("{paymentId}")]
    public async Task<IActionResult> PartialUpdatePayment(Guid paymentId, [FromBody] UpdatePaymentRequestDTO updateData)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedPayment = await _paymentService.UpdatePaymentAsync(paymentId, updateData);

            if (updatedPayment == null)
            {
                return NotFound($"Payment with ID {paymentId} not found or update data was invalid.");
            }

            return Ok(new { status = "Success", payment = updatedPayment });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}