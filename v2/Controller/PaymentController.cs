using Microsoft.AspNetCore.Mvc;
using v2.core.Interfaces;
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

    [HttpPost("create")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _paymentService.CreatePaymentAsync(request);

        if (result is null)
        {
            return StatusCode(500, "Failed to create payment.");
        }

        return StatusCode(StatusCodes.Status201Created, result);
    }
}