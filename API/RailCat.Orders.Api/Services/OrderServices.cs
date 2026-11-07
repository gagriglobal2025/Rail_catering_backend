using RailCat.Orders.Api.Helpers;
using RailCat.Orders.Api.Models.DTOs;
using RailCat.Orders.Api.Models.Entities;
using RailCat.Orders.Api.Repositories;

namespace RailCat.Orders.Api.Services;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto dto);
    Task<OrderResponseDto?> GetOrderByIdAsync(Guid id);
    Task<OrderResponseDto?> GetByOrderNumberAsync(string orderNumber);
}

public interface IKitchenService
{
    Task<IEnumerable<KitchenTicketResponseDto>> GetKitchenTicketsAsync();
    Task<bool> UpdateOrderStatusAsync(Guid orderId, string nextStatus);
}

public interface IQueueTokenService
{
    Task<string> GetNextTokenAsync();
}

public class QueueTokenService : IQueueTokenService
{
    private readonly IOrderRepository _repo;

    public QueueTokenService(IOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task<string> GetNextTokenAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var seq = await _repo.GetOrCreateDailySequenceAsync(today);
        return $"{seq.TokenPrefix}-{seq.CurrentNumber:D3}";
    }
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repo;
    private readonly IQueueTokenService _tokenService;

    public OrderService(IOrderRepository repo, IQueueTokenService tokenService)
    {
        _repo = repo;
        _tokenService = tokenService;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto dto)
    {
        var queueToken = await _tokenService.GetNextTokenAsync();
        var datePrefix = DateTime.UtcNow.ToString("yyyyMMdd");
        var orderNumber = $"ORD-{datePrefix}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

        // Business pricing calculations: 20% concession for Railway Employees, 5% GST
        decimal subtotal = dto.Items.Sum(i => i.Quantity * i.UnitPrice);
        decimal concession = 0;
        if (dto.CustomerType == "RAILWAY_EMPLOYEE")
        {
            concession = Math.Round(subtotal * 0.20m, 2);
        }
        decimal taxable = subtotal - concession;
        decimal tax = Math.Round(taxable * 0.05m, 2);
        decimal grandTotal = taxable + tax;

        var order = new Order
        {
            OrderNumber = orderNumber,
            QueueToken = queueToken,
            CustomerType = dto.CustomerType,
            CustomerRef = dto.CustomerRef,
            PickupCounter = dto.PickupCounter,
            SpecialInstructions = dto.SpecialInstructions,
            SubtotalAmount = subtotal,
            ConcessionDiscount = concession,
            TaxAmount = tax,
            GrandTotal = grandTotal,
            OrderStatus = "NEW",
            PaymentStatus = string.IsNullOrEmpty(dto.PaymentMethod) ? "PENDING" : "PAID",
            PaymentMethod = dto.PaymentMethod,
            CreatedByStaffId = dto.CreatedByStaffId,
            PlacedAt = DateTime.UtcNow,
            PaidAt = string.IsNullOrEmpty(dto.PaymentMethod) ? null : DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var item in dto.Items)
        {
            order.Items.Add(new OrderItem
            {
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.Quantity * item.UnitPrice
            });
        }

        // Auto-spool KOT if paid
        if (order.PaymentStatus == "PAID")
        {
            order.PrintJobs.Add(new PrintSpool
            {
                JobType = "KOT",
                RawContent = EscPosHelper.GenerateKotText(order),
                PrintStatus = "PENDING"
            });
            order.PrintJobs.Add(new PrintSpool
            {
                JobType = "RECEIPT",
                RawContent = EscPosHelper.GenerateReceiptText(order),
                PrintStatus = "PENDING"
            });
        }

        var created = await _repo.AddAsync(order);
        return MapToDto(created);
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(Guid id)
    {
        var order = await _repo.GetByIdAsync(id);
        return order == null ? null : MapToDto(order);
    }

    public async Task<OrderResponseDto?> GetByOrderNumberAsync(string orderNumber)
    {
        var order = await _repo.GetByOrderNumberAsync(orderNumber);
        return order == null ? null : MapToDto(order);
    }

    private static OrderResponseDto MapToDto(Order o) => new()
    {
        OrderId = o.OrderId,
        OrderNumber = o.OrderNumber,
        QueueToken = o.QueueToken,
        CustomerType = o.CustomerType,
        CustomerRef = o.CustomerRef,
        PickupCounter = o.PickupCounter,
        SpecialInstructions = o.SpecialInstructions,
        SubtotalAmount = o.SubtotalAmount,
        ConcessionDiscount = o.ConcessionDiscount,
        TaxAmount = o.TaxAmount,
        GrandTotal = o.GrandTotal,
        OrderStatus = o.OrderStatus,
        PaymentStatus = o.PaymentStatus,
        PaymentMethod = o.PaymentMethod,
        PlacedAt = o.PlacedAt,
        Items = o.Items.Select(i => new OrderItemDto
        {
            ItemId = i.ItemId,
            ItemName = i.ItemName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            LineTotal = i.LineTotal
        }).ToList()
    };
}

public class KitchenService : IKitchenService
{
    private readonly IOrderRepository _repo;

    public KitchenService(IOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<KitchenTicketResponseDto>> GetKitchenTicketsAsync()
    {
        var orders = await _repo.GetActiveOrdersAsync();
        return orders
            .Where(o => o.PaymentStatus == "PAID")
            .Select(o => new KitchenTicketResponseDto
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                QueueToken = o.QueueToken,
                CustomerType = o.CustomerType,
                PickupCounter = o.PickupCounter,
                SpecialInstructions = o.SpecialInstructions,
                OrderStatus = o.OrderStatus,
                PlacedAt = o.PlacedAt,
                ElapsedMinutes = (int)(DateTime.UtcNow - o.PlacedAt).TotalMinutes,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    ItemId = i.ItemId,
                    ItemName = i.ItemName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal
                }).ToList()
            });
    }

    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string nextStatus)
    {
        var order = await _repo.GetByIdAsync(orderId);
        if (order == null) return false;

        order.OrderStatus = nextStatus;
        if (nextStatus == "PREPARING") order.PreparingAt = DateTime.UtcNow;
        else if (nextStatus == "READY") order.ReadyAt = DateTime.UtcNow;
        else if (nextStatus == "DELIVERED") order.DeliveredAt = DateTime.UtcNow;
        else if (nextStatus == "CANCELLED") order.CancelledAt = DateTime.UtcNow;

        await _repo.UpdateAsync(order);
        return true;
    }
}
