using Microsoft.AspNetCore.Mvc;
using v2.Core.Interfaces;
using v2.Core.DTOs;

namespace v2.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IPayments _paymentService;

    public PaymentController(IPayments paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>Create a new payment</summary>
    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var payment = await _paymentService.CreatePaymentAsync(request);
            return CreatedAtAction(nameof(GetPaymentsByUser), new { initiator = payment.Initiator }, payment);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Confirm a payment via hash</summary>
    [HttpPost("{paymentId}/confirm")]
    public async Task<IActionResult> ConfirmPayment(Guid paymentId, [FromBody] ConfirmPaymentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var payment = await _paymentService.ConfirmPaymentAsync(paymentId, request.Hash);
            return Ok(payment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Get payments for a user/initiator</summary>
    [HttpGet("user/{initiator}")]
    public async Task<IActionResult> GetPaymentsByUser(string initiator)
    {
        try
        {
            var payments = await _paymentService.GetPaymentsByUserAsync(initiator);
            return Ok(payments);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Update an incomplete payment (partial update)</summary>
    [HttpPatch("{paymentId}")]
    public async Task<IActionResult> UpdatePayment(Guid paymentId, [FromBody] UpdatePaymentRequestDTO updateData)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var payment = await _paymentService.UpdatePaymentAsync(paymentId, updateData);
            return Ok(payment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Refund a payment</summary>
    [HttpPost("{paymentId}/refund")]
    public async Task<IActionResult> RefundPayment(Guid paymentId, [FromBody] RefundPaymentRequest? request)
    {
        try
        {
            var payment = await _paymentService.RefundPaymentAsync(paymentId, request?.Reason);
            return Ok(payment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}

// Helper DTOs for request bodies
public class ConfirmPaymentRequest
{
    public required string Hash { get; set; }
}

public class RefundPaymentRequest
{
    public string? Reason { get; set; }
}
