namespace RailCat.Orders.Api.Models.Entities;

public class DailyQueueSequence
{
    public DateOnly OperationalDate { get; set; }
    public int CurrentNumber { get; set; }
    public string TokenPrefix { get; set; } = "A";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Order
{
    public Guid OrderId { get; set; } = Guid.NewGuid();
    public string OrderNumber { get; set; } = string.Empty;
    public string QueueToken { get; set; } = string.Empty;
    public string CustomerType { get; set; } = "NORMAL"; // NORMAL, RAILWAY_EMPLOYEE
    public string? CustomerRef { get; set; }
    public string PickupCounter { get; set; } = "Counter 1";
    public string? SpecialInstructions { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal ConcessionDiscount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string OrderStatus { get; set; } = "NEW"; // NEW, PREPARING, READY, DELIVERED, CANCELLED
    public string PaymentStatus { get; set; } = "PENDING"; // PENDING, PAID, FAILED
    public string? PaymentMethod { get; set; } // UPI, CARD, CASH
    public Guid? CreatedByStaffId { get; set; }
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public DateTime? PreparingAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<PrintSpool> PrintJobs { get; set; } = new List<PrintSpool>();
}

public class OrderItem
{
    public long OrderItemId { get; set; }
    public Guid OrderId { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public Order? Order { get; set; }
}

public class PrintSpool
{
    public Guid JobId { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public string JobType { get; set; } = "KOT"; // KOT, RECEIPT
    public string RawContent { get; set; } = string.Empty;
    public string PrintStatus { get; set; } = "PENDING"; // PENDING, PRINTED, FAILED
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PrintedAt { get; set; }

    public Order? Order { get; set; }
}
