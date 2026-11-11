using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RailCat.Settlement.Api.Models.DTOs;
using RailCat.Settlement.Api.Models.Entities;
using RailCat.Settlement.Api.Repositories;

namespace RailCat.Settlement.Api.Services;

public interface IShiftService
{
    Task<CashierShiftResponseDto> OpenShiftAsync(OpenShiftRequestDto dto);
    Task<CashierShiftResponseDto?> CloseShiftAsync(Guid shiftId, CloseShiftRequestDto dto);
    Task<CashierShiftResponseDto?> GetShiftByIdAsync(Guid shiftId);
    Task<CashierShiftResponseDto?> GetActiveShiftAsync(Guid? cashierUserId);
    Task<IEnumerable<CashierShiftResponseDto>> GetShiftsAsync(Guid? cashierUserId, string? status, int limit = 50);
}

public interface IPaymentService
{
    Task<PaymentResponseDto> ProcessPaymentAsync(ProcessPaymentRequestDto dto);
    Task<IEnumerable<PaymentResponseDto>> GetPaymentsAsync(Guid? shiftId, Guid? orderId, int limit = 50);
}

public interface IRazorpayService
{
    Task<RazorpayOrderResponseDto> CreateOrderAsync(CreateRazorpayOrderDto dto);
    Task<PaymentResponseDto> VerifyAndRecordPaymentAsync(VerifyRazorpayPaymentDto dto);
}

public interface IReportService
{
    Task<DailySalesSummaryDto> GetDailySummaryAsync(DateOnly? date);
}

public class ShiftService : IShiftService
{
    private readonly IShiftRepository _repo;

    public ShiftService(IShiftRepository repo)
    {
        _repo = repo;
    }

    public async Task<CashierShiftResponseDto> OpenShiftAsync(OpenShiftRequestDto dto)
    {
        var existing = await _repo.GetActiveShiftByCashierAsync(dto.CashierUserId);
        if (existing != null)
        {
            return MapToDto(existing);
        }

        var shift = new CashierShift
        {
            CashierUserId = dto.CashierUserId,
            StartingCash = dto.StartingCash,
            TotalExpected = dto.StartingCash,
            OpenedAt = DateTime.UtcNow,
            ShiftStatus = "OPEN"
        };

        var created = await _repo.AddAsync(shift);
        return MapToDto(created);
    }

    public async Task<CashierShiftResponseDto?> CloseShiftAsync(Guid shiftId, CloseShiftRequestDto dto)
    {
        var shift = await _repo.GetByIdAsync(shiftId);
        if (shift == null || shift.ShiftStatus != "OPEN") return null;

        shift.ClosedAt = DateTime.UtcNow;
        shift.ActualCashCounted = dto.ActualCashCounted;
        shift.ShiftStatus = "CLOSED";

        await _repo.UpdateAsync(shift);
        return MapToDto(shift);
    }

    public async Task<CashierShiftResponseDto?> GetShiftByIdAsync(Guid shiftId)
    {
        var shift = await _repo.GetByIdAsync(shiftId);
        return shift == null ? null : MapToDto(shift);
    }

    public async Task<CashierShiftResponseDto?> GetActiveShiftAsync(Guid? cashierUserId)
    {
        CashierShift? shift;
        if (cashierUserId.HasValue)
        {
            shift = await _repo.GetActiveShiftByCashierAsync(cashierUserId.Value);
        }
        else
        {
            shift = await _repo.GetAnyActiveShiftAsync();
        }
        return shift == null ? null : MapToDto(shift);
    }

    public async Task<IEnumerable<CashierShiftResponseDto>> GetShiftsAsync(Guid? cashierUserId, string? status, int limit = 50)
    {
        var shifts = await _repo.GetShiftsAsync(cashierUserId, status, limit);
        return shifts.Select(MapToDto);
    }

    private static CashierShiftResponseDto MapToDto(CashierShift s) => new()
    {
        ShiftId = s.ShiftId,
        CashierUserId = s.CashierUserId,
        OpenedAt = s.OpenedAt,
        ClosedAt = s.ClosedAt,
        StartingCash = s.StartingCash,
        CashCollected = s.CashCollected,
        UpiCollected = s.UpiCollected,
        CardCollected = s.CardCollected,
        TotalExpected = s.TotalExpected,
        ActualCashCounted = s.ActualCashCounted,
        ShiftStatus = s.ShiftStatus
    };
}

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IShiftRepository _shiftRepo;
    private readonly IReportRepository _reportRepo;

    public PaymentService(IPaymentRepository paymentRepo, IShiftRepository shiftRepo, IReportRepository reportRepo)
    {
        _paymentRepo = paymentRepo;
        _shiftRepo = shiftRepo;
        _reportRepo = reportRepo;
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(ProcessPaymentRequestDto dto)
    {
        var payment = new PaymentTransaction
        {
            OrderId = dto.OrderId,
            ShiftId = dto.ShiftId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            GatewayReference = dto.GatewayReference ?? $"PAY-{DateTime.UtcNow.Ticks}",
            PaymentStatus = "SUCCESS",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _paymentRepo.AddAsync(payment);

        // Update shift totals if shift provided
        if (dto.ShiftId.HasValue)
        {
            var shift = await _shiftRepo.GetByIdAsync(dto.ShiftId.Value);
            if (shift != null && shift.ShiftStatus == "OPEN")
            {
                if (dto.PaymentMethod == "CASH")
                {
                    shift.CashCollected += dto.Amount;
                    shift.TotalExpected += dto.Amount;
                }
                else if (dto.PaymentMethod == "UPI")
                {
                    shift.UpiCollected += dto.Amount;
                }
                else if (dto.PaymentMethod == "CARD" || dto.PaymentMethod == "RAZORPAY")
                {
                    shift.CardCollected += dto.Amount;
                }
                await _shiftRepo.UpdateAsync(shift);
            }
        }

        // Update daily sales summary
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var summary = await _reportRepo.GetSummaryByDateAsync(today) ?? new DailySalesSummary
        {
            SummaryDate = today
        };

        summary.TotalOrders++;
        summary.GrossSales += dto.Amount;
        summary.NetRevenue += dto.Amount;

        if (dto.PaymentMethod == "UPI") summary.UpiRevenue += dto.Amount;
        else if (dto.PaymentMethod == "CASH") summary.CashRevenue += dto.Amount;
        else if (dto.PaymentMethod == "CARD" || dto.PaymentMethod == "RAZORPAY") summary.CardRevenue += dto.Amount;

        await _reportRepo.UpdateSummaryAsync(summary);

        return new PaymentResponseDto
        {
            PaymentId = created.PaymentId,
            OrderId = created.OrderId,
            Amount = created.Amount,
            PaymentMethod = created.PaymentMethod,
            GatewayReference = created.GatewayReference,
            PaymentStatus = created.PaymentStatus,
            CreatedAt = created.CreatedAt
        };
    }

    public async Task<IEnumerable<PaymentResponseDto>> GetPaymentsAsync(Guid? shiftId, Guid? orderId, int limit = 50)
    {
        var payments = await _paymentRepo.GetFilteredPaymentsAsync(shiftId, orderId, limit);
        return payments.Select(p => new PaymentResponseDto
        {
            PaymentId = p.PaymentId,
            OrderId = p.OrderId,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod,
            GatewayReference = p.GatewayReference,
            PaymentStatus = p.PaymentStatus,
            CreatedAt = p.CreatedAt
        });
    }
}

public class ReportService : IReportService
{
    private readonly IReportRepository _repo;

    public ReportService(IReportRepository repo)
    {
        _repo = repo;
    }

    public async Task<DailySalesSummaryDto> GetDailySummaryAsync(DateOnly? date)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var summary = await _repo.GetSummaryByDateAsync(targetDate);

        if (summary == null)
        {
            return new DailySalesSummaryDto
            {
                SummaryDate = targetDate,
                TotalOrders = 0,
                GrossSales = 0,
                ConcessionsGiven = 0,
                TaxCollected = 0,
                NetRevenue = 0,
                UpiRevenue = 0,
                CashRevenue = 0,
                CardRevenue = 0
            };
        }

        return new DailySalesSummaryDto
        {
            SummaryDate = summary.SummaryDate,
            TotalOrders = summary.TotalOrders,
            GrossSales = summary.GrossSales,
            ConcessionsGiven = summary.ConcessionsGiven,
            TaxCollected = summary.TaxCollected,
            NetRevenue = summary.NetRevenue,
            UpiRevenue = summary.UpiRevenue,
            CashRevenue = summary.CashRevenue,
            CardRevenue = summary.CardRevenue
        };
    }
}

public class RazorpayService : IRazorpayService
{
    private readonly HttpClient _http;
    private readonly IPaymentService _paymentService;
    private readonly string _keyId;
    private readonly string _keySecret;

    public RazorpayService(HttpClient http, IPaymentService paymentService, IConfiguration configuration)
    {
        _keyId = configuration["RazorpaySettings:KeyId"] ?? string.Empty;
        _keySecret = configuration["RazorpaySettings:KeySecret"] ?? string.Empty;

        _http = http;
        _http.BaseAddress = new Uri("https://api.razorpay.com/v1/");
        var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_keyId}:{_keySecret}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

        _paymentService = paymentService;
    }

    public async Task<RazorpayOrderResponseDto> CreateOrderAsync(CreateRazorpayOrderDto dto)
    {
        var amountInPaise = (long)Math.Round(dto.Amount * 100, MidpointRounding.AwayFromZero);
        var payload = new
        {
            amount = amountInPaise,
            currency = "INR",
            receipt = dto.Receipt ?? $"rcpt_{DateTime.UtcNow.Ticks}"
        };

        var response = await _http.PostAsJsonAsync("orders", payload);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Razorpay order creation failed ({(int)response.StatusCode}): {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var razorpayOrderId = doc.RootElement.GetProperty("id").GetString() ?? string.Empty;

        return new RazorpayOrderResponseDto
        {
            RazorpayOrderId = razorpayOrderId,
            Amount = dto.Amount,
            Currency = "INR",
            KeyId = _keyId
        };
    }

    public async Task<PaymentResponseDto> VerifyAndRecordPaymentAsync(VerifyRazorpayPaymentDto dto)
    {
        var payload = $"{dto.RazorpayOrderId}|{dto.RazorpayPaymentId}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_keySecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var generatedSignature = Convert.ToHexString(hash).ToLowerInvariant();

        if (!string.Equals(generatedSignature, dto.RazorpaySignature, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Razorpay payment signature verification failed.");
        }

        return await _paymentService.ProcessPaymentAsync(new ProcessPaymentRequestDto
        {
            OrderId = dto.OrderId,
            ShiftId = dto.ShiftId,
            Amount = dto.Amount,
            PaymentMethod = "RAZORPAY",
            GatewayReference = dto.RazorpayPaymentId
        });
    }
}
