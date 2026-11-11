using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailCat.Settlement.Api.Models.DTOs;
using RailCat.Settlement.Api.Services;

namespace RailCat.Settlement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN,STORE_MANAGER,COUNTER_OPERATOR")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;

    public ShiftsController(IShiftService shiftService)
    {
        _shiftService = shiftService;
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(CashierShiftResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActiveShift([FromQuery] Guid? cashierId)
    {
        var shift = await _shiftService.GetActiveShiftAsync(cashierId);
        if (shift == null) return NotFound(new { message = "No active open shift found." });
        return Ok(shift);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CashierShiftResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? cashierId,
        [FromQuery] string? status,
        [FromQuery] int limit = 50)
    {
        var shifts = await _shiftService.GetShiftsAsync(cashierId, status, limit);
        return Ok(shifts);
    }

    [HttpPost("open")]
    [ProducesResponseType(typeof(CashierShiftResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> OpenShift([FromBody] OpenShiftRequestDto dto)
    {
        var shift = await _shiftService.OpenShiftAsync(dto);
        return Ok(shift);
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(CashierShiftResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseShift(Guid id, [FromBody] CloseShiftRequestDto dto)
    {
        var shift = await _shiftService.CloseShiftAsync(id, dto);
        if (shift == null) return NotFound(new { message = $"Active shift with ID {id} not found." });
        return Ok(shift);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CashierShiftResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var shift = await _shiftService.GetShiftByIdAsync(id);
        if (shift == null) return NotFound(new { message = $"Shift with ID {id} not found." });
        return Ok(shift);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN,STORE_MANAGER,COUNTER_OPERATOR")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PaymentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? shiftId,
        [FromQuery] Guid? orderId,
        [FromQuery] int limit = 50)
    {
        var payments = await _paymentService.GetPaymentsAsync(shiftId, orderId, limit);
        return Ok(payments);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequestDto dto)
    {
        var payment = await _paymentService.ProcessPaymentAsync(dto);
        return Ok(payment);
    }
}

[ApiController]
[Route("api/payments/razorpay")]
[AllowAnonymous]
public class RazorpayController : ControllerBase
{
    private readonly IRazorpayService _razorpayService;

    public RazorpayController(IRazorpayService razorpayService)
    {
        _razorpayService = razorpayService;
    }

    [HttpPost("create-order")]
    [ProducesResponseType(typeof(RazorpayOrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateRazorpayOrderDto dto)
    {
        try
        {
            var order = await _razorpayService.CreateOrderAsync(dto);
            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("verify")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Verify([FromBody] VerifyRazorpayPaymentDto dto)
    {
        try
        {
            var payment = await _razorpayService.VerifyAndRecordPaymentAsync(dto);
            return Ok(payment);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN,STORE_MANAGER")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("daily-sales")]
    [ProducesResponseType(typeof(DailySalesSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDailySales([FromQuery] string? date)
    {
        DateOnly? parsedDate = null;
        if (!string.IsNullOrEmpty(date) && DateOnly.TryParse(date, out var d))
        {
            parsedDate = d;
        }

        var report = await _reportService.GetDailySummaryAsync(parsedDate);
        return Ok(report);
    }
}
