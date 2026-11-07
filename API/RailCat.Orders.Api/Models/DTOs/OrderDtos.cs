namespace RailCat.Orders.Api.Models.DTOs;

public class OrderItemRequestDto
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CreateOrderRequestDto
{
    public string CustomerType { get; set; } = "NORMAL";
    public string? CustomerRef { get; set; }
    public string PickupCounter { get; set; } = "Counter 1";
    public string? SpecialInstructions { get; set; }
    public string? PaymentMethod { get; set; } // UPI, CARD, CASH
    public Guid? CreatedByStaffId { get; set; }
    public List<OrderItemRequestDto> Items { get; set; } = new();
}

public class OrderResponseDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string QueueToken { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public string? CustomerRef { get; set; }
    public string PickupCounter { get; set; } = string.Empty;
    public string? SpecialInstructions { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal ConcessionDiscount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public DateTime PlacedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class KitchenTicketResponseDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string QueueToken { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public string PickupCounter { get; set; } = string.Empty;
    public string? SpecialInstructions { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime PlacedAt { get; set; }
    public int ElapsedMinutes { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class UpdateOrderStatusDto
{
    public string NextStatus { get; set; } = string.Empty; // PREPARING, READY, DELIVERED, CANCELLED
}

public class PrintSpoolResponseDto
{
    public Guid JobId { get; set; }
    public Guid OrderId { get; set; }
    public string JobType { get; set; } = string.Empty;
    public string RawContent { get; set; } = string.Empty;
    public string PrintStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

