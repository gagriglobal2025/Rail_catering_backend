using System.Text;
using RailCat.Orders.Api.Models.Entities;

namespace RailCat.Orders.Api.Helpers;

public static class EscPosHelper
{
    public static string GenerateKotText(Order order)
    {
        var sb = new StringBuilder();
        sb.AppendLine("================================");
        sb.AppendLine("       KITCHEN ORDER TICKET     ");
        sb.AppendLine("================================");
        sb.AppendLine($"TOKEN : {order.QueueToken}");
        sb.AppendLine($"ORDER : {order.OrderNumber}");
        sb.AppendLine($"TYPE  : {order.CustomerType}");
        sb.AppendLine($"TIME  : {order.PlacedAt:dd-MM-yyyy HH:mm:ss}");
        sb.AppendLine("--------------------------------");
        foreach (var item in order.Items)
        {
            sb.AppendLine($"{item.Quantity,-3} x {item.ItemName}");
        }
        sb.AppendLine("--------------------------------");
        if (!string.IsNullOrEmpty(order.SpecialInstructions))
        {
            sb.AppendLine($"NOTE: {order.SpecialInstructions}");
        }
        sb.AppendLine("================================");
        return sb.ToString();
    }

    public static string GenerateReceiptText(Order order)
    {
        var sb = new StringBuilder();
        sb.AppendLine("================================");
        sb.AppendLine("          RAILCAT QSR           ");
        sb.AppendLine("    Private Curry Point & Cafe  ");
        sb.AppendLine("================================");
        sb.AppendLine($"TOKEN : {order.QueueToken}");
        sb.AppendLine($"ORDER : {order.OrderNumber}");
        sb.AppendLine($"DATE  : {order.PlacedAt:dd-MM-yyyy HH:mm}");
        sb.AppendLine("--------------------------------");
        foreach (var item in order.Items)
        {
            sb.AppendLine($"{item.ItemName,-20} {item.LineTotal,8:F2}");
            sb.AppendLine($"  {item.Quantity} x {item.UnitPrice:F2}");
        }
        sb.AppendLine("--------------------------------");
        sb.AppendLine($"SUBTOTAL:             {order.SubtotalAmount,8:F2}");
        if (order.ConcessionDiscount > 0)
        {
            sb.AppendLine($"CONCESSION (20%):    -{order.ConcessionDiscount,8:F2}");
        }
        sb.AppendLine($"GST (5%):             {order.TaxAmount,8:F2}");
        sb.AppendLine($"TOTAL AMOUNT:         {order.GrandTotal,8:F2}");
        sb.AppendLine("--------------------------------");
        sb.AppendLine($"PAYMENT: {order.PaymentMethod ?? "CASH"} ({order.PaymentStatus})");
        sb.AppendLine("     Thank You! Visit Again     ");
        sb.AppendLine("================================");
        return sb.ToString();
    }
}
