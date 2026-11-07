namespace RailCat.Settlement.Api.Models.DTOs;

public class OpenShiftRequestDto
{
    public Guid CashierUserId { get; set; }
    public decimal StartingCash { get; set; } = 500.00m;
}

public class CloseShiftRequestDto
{
    public decimal ActualCashCounted { get; set; }
}

public class CashierShiftResponseDto
{
    public Guid ShiftId { get; set; }
    public Guid CashierUserId { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal StartingCash { get; set; }
    public decimal CashCollected { get; set; }
    public decimal UpiCollected { get; set; }
    public decimal CardCollected { get; set; }
    public decimal TotalExpected { get; set; }
    public decimal? ActualCashCounted { get; set; }
    public string ShiftStatus { get; set; } = string.Empty;
}

public class ProcessPaymentRequestDto
{
    public Guid OrderId { get; set; }
    public Guid? ShiftId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "UPI"; // UPI, CARD, CASH
    public string? GatewayReference { get; set; }
}

public class PaymentResponseDto
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? GatewayReference { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DailySalesSummaryDto
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
}

