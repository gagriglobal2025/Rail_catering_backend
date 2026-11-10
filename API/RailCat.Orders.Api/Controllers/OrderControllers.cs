using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailCat.Orders.Api.Models.DTOs;
using RailCat.Orders.Api.Repositories;
using RailCat.Orders.Api.Services;

namespace RailCat.Orders.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN,STORE_MANAGER,COUNTER_OPERATOR")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? paymentStatus,
        [FromQuery] string? search,
        [FromQuery] int limit = 50)
    {
        var orders = await _orderService.GetOrdersAsync(status, paymentStatus, search, limit);
        return Ok(orders);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto dto)
    {
        var created = await _orderService.CreateOrderAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.OrderId }, created);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound(new { message = $"Order with ID {id} not found." });
        return Ok(order);
    }

    [HttpGet("by-number/{orderNumber}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByNumber(string orderNumber)
    {
        var order = await _orderService.GetByOrderNumberAsync(orderNumber);
        if (order == null) return NotFound(new { message = $"Order '{orderNumber}' not found." });
        return Ok(order);
    }

    [HttpPatch("{id:guid}/payment")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptPayment(Guid id, [FromBody] AcceptOrderPaymentDto dto)
    {
        var order = await _orderService.AcceptPaymentAsync(id, dto);
        if (order == null) return NotFound(new { message = $"Order with ID {id} not found." });
        return Ok(order);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await _orderService.UpdateOrderStatusAsync(id, dto.NextStatus);
        if (order == null) return NotFound(new { message = $"Order with ID {id} not found." });
        return Ok(order);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN,STORE_MANAGER,KITCHEN_STAFF")]
public class KitchenController : ControllerBase
{
    private readonly IKitchenService _kitchenService;

    public KitchenController(IKitchenService kitchenService)
    {
        _kitchenService = kitchenService;
    }

    [HttpGet("tickets")]
    [ProducesResponseType(typeof(IEnumerable<KitchenTicketResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveTickets()
    {
        var tickets = await _kitchenService.GetKitchenTicketsAsync();
        return Ok(tickets);
    }

    [HttpPatch("tickets/{orderId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid orderId, [FromBody] UpdateOrderStatusDto dto)
    {
        var success = await _kitchenService.UpdateOrderStatusAsync(orderId, dto.NextStatus);
        if (!success) return NotFound(new { message = $"Order with ID {orderId} not found." });
        return Ok(new { message = $"Status updated to {dto.NextStatus}" });
    }
}

[ApiController]
[Route("api/print-spool")]
[Authorize(Roles = "ADMIN,STORE_MANAGER,COUNTER_OPERATOR,KITCHEN_STAFF")]
public class PrintSpoolController : ControllerBase
{
    private readonly IOrderRepository _repo;
    private readonly IOrderService _orderService;

    public PrintSpoolController(IOrderRepository repo, IOrderService orderService)
    {
        _repo = repo;
        _orderService = orderService;
    }

    [HttpGet("pending")]
    [ProducesResponseType(typeof(IEnumerable<PrintSpoolResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingPrintJobs()
    {
        var jobs = await _repo.GetPendingPrintJobsAsync();
        var dtos = jobs.Select(j => new PrintSpoolResponseDto
        {
            JobId = j.JobId,
            OrderId = j.OrderId,
            JobType = j.JobType,
            RawContent = j.RawContent,
            PrintStatus = j.PrintStatus,
            CreatedAt = j.CreatedAt
        });
        return Ok(dtos);
    }

    [HttpPatch("{jobId:guid}/printed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkJobPrinted(Guid jobId)
    {
        var success = await _orderService.MarkPrintJobPrintedAsync(jobId);
        if (!success) return NotFound(new { message = $"Print job with ID {jobId} not found." });
        return Ok(new { message = "Print job marked as PRINTED." });
    }
}
