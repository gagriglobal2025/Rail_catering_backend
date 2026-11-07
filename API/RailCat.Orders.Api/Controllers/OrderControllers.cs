using Microsoft.AspNetCore.Mvc;
using RailCat.Orders.Api.Models.DTOs;
using RailCat.Orders.Api.Repositories;
using RailCat.Orders.Api.Services;

namespace RailCat.Orders.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
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
}

[ApiController]
[Route("api/[controller]")]
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
public class PrintSpoolController : ControllerBase
{
    private readonly IOrderRepository _repo;

    public PrintSpoolController(IOrderRepository repo)
    {
        _repo = repo;
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
}
