namespace RailCat.Settlement.Api.Models.Entities;

public class CashierShift
{
    public Guid ShiftId { get; set; } = Guid.NewGuid();
    public Guid CashierUserId { get; set; }
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public decimal StartingCash { get; set; } = 500.00m;
    public decimal CashCollected { get; set; }
    public decimal UpiCollected { get; set; }
    public decimal CardCollected { get; set; }
    public decimal TotalExpected { get; set; } = 500.00m;
    public decimal? ActualCashCounted { get; set; }
    public string ShiftStatus { get; set; } = "OPEN"; // OPEN, CLOSED

    public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
}

public class PaymentTransaction
{
    public Guid PaymentId { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid? ShiftId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "UPI"; // UPI, CARD, CASH
    public string? GatewayReference { get; set; }
    public string PaymentStatus { get; set; } = "SUCCESS"; // SUCCESS, FAILED, REFUNDED
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CashierShift? Shift { get; set; }
}

public class DailySalesSummary
{
    public DateOnly SummaryDate { get; set; }
    public int TotalOrders { get; set; }
    public decimal GrossSales { get; set; }
    public decimal ConcessionsGiven { get; set; }
    public decimal TaxCollected { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal UpiRevenue { get; set; }
    public decimal CashRevenue { get; set; }
    public decimal CardRevenue { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

