using Microsoft.AspNetCore.Mvc;
using RailCat.Settlement.Api.Models.DTOs;
using RailCat.Settlement.Api.Services;

namespace RailCat.Settlement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;

    public ShiftsController(IShiftService shiftService)
    {
        _shiftService = shiftService;
    }

    [HttpPost("open")]
    [ProducesResponseType(typeof(CashierShiftResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> OpenShift([FromBody] OpenShiftRequestDto dto)
    {
        var shift = await _shiftService.OpenShiftAsync(dto);
        return Ok(shift);
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(CashierShiftResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseShift(Guid id, [FromBody] CloseShiftRequestDto dto)
    {
        var shift = await _shiftService.CloseShiftAsync(id, dto);
        if (shift == null) return NotFound(new { message = $"Active shift with ID {id} not found." });
        return Ok(shift);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CashierShiftResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var shift = await _shiftService.GetShiftByIdAsync(id);
        if (shift == null) return NotFound(new { message = $"Shift with ID {id} not found." });
        return Ok(shift);
    }
}

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequestDto dto)
    {
        var payment = await _paymentService.ProcessPaymentAsync(dto);
        return Ok(payment);
    }
}

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("daily-sales")]
    [ProducesResponseType(typeof(DailySalesSummaryDto), StatusCodes.Status200OK)]
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

