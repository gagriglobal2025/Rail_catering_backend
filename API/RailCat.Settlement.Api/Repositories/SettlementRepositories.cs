using Microsoft.EntityFrameworkCore;
using RailCat.Settlement.Api.Data;
using RailCat.Settlement.Api.Models.Entities;

namespace RailCat.Settlement.Api.Repositories;

public interface IShiftRepository
{
    Task<CashierShift?> GetActiveShiftByCashierAsync(Guid cashierUserId);
    Task<CashierShift?> GetAnyActiveShiftAsync();
    Task<CashierShift?> GetByIdAsync(Guid shiftId);
    Task<IEnumerable<CashierShift>> GetShiftsAsync(Guid? cashierUserId, string? status, int limit = 50);
    Task<CashierShift> AddAsync(CashierShift shift);
    Task UpdateAsync(CashierShift shift);
}

public interface IPaymentRepository
{
    Task<PaymentTransaction> AddAsync(PaymentTransaction payment);
    Task<IEnumerable<PaymentTransaction>> GetByShiftIdAsync(Guid shiftId);
    Task<IEnumerable<PaymentTransaction>> GetFilteredPaymentsAsync(Guid? shiftId, Guid? orderId, int limit = 50);
}

public interface IReportRepository
{
    Task<DailySalesSummary?> GetSummaryByDateAsync(DateOnly date);
    Task UpdateSummaryAsync(DailySalesSummary summary);
}

public class ShiftRepository : IShiftRepository
{
    private readonly SettlementDbContext _context;

    public ShiftRepository(SettlementDbContext context)
    {
        _context = context;
    }

    public async Task<CashierShift?> GetActiveShiftByCashierAsync(Guid cashierUserId)
    {
        return await _context.CashierShifts
            .FirstOrDefaultAsync(s => s.CashierUserId == cashierUserId && s.ShiftStatus == "OPEN");
    }

    public async Task<CashierShift?> GetAnyActiveShiftAsync()
    {
        return await _context.CashierShifts
            .FirstOrDefaultAsync(s => s.ShiftStatus == "OPEN");
    }

    public async Task<CashierShift?> GetByIdAsync(Guid shiftId)
    {
        return await _context.CashierShifts
            .Include(s => s.Transactions)
            .FirstOrDefaultAsync(s => s.ShiftId == shiftId);
    }

    public async Task<IEnumerable<CashierShift>> GetShiftsAsync(Guid? cashierUserId, string? status, int limit = 50)
    {
        var query = _context.CashierShifts.AsQueryable();
        if (cashierUserId.HasValue) query = query.Where(s => s.CashierUserId == cashierUserId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(s => s.ShiftStatus == status);
        return await query.OrderByDescending(s => s.OpenedAt).Take(limit).ToListAsync();
    }

    public async Task<CashierShift> AddAsync(CashierShift shift)
    {
        await _context.CashierShifts.AddAsync(shift);
        await _context.SaveChangesAsync();
        return shift;
    }

    public async Task UpdateAsync(CashierShift shift)
    {
        _context.CashierShifts.Update(shift);
        await _context.SaveChangesAsync();
    }
}

public class PaymentRepository : IPaymentRepository
{
    private readonly SettlementDbContext _context;

    public PaymentRepository(SettlementDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentTransaction> AddAsync(PaymentTransaction payment)
    {
        await _context.PaymentTransactions.AddAsync(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<IEnumerable<PaymentTransaction>> GetByShiftIdAsync(Guid shiftId)
    {
        return await _context.PaymentTransactions
            .Where(p => p.ShiftId == shiftId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentTransaction>> GetFilteredPaymentsAsync(Guid? shiftId, Guid? orderId, int limit = 50)
    {
        var query = _context.PaymentTransactions.AsQueryable();
        if (shiftId.HasValue) query = query.Where(p => p.ShiftId == shiftId.Value);
        if (orderId.HasValue) query = query.Where(p => p.OrderId == orderId.Value);
        return await query.OrderByDescending(p => p.CreatedAt).Take(limit).ToListAsync();
    }
}

public class ReportRepository : IReportRepository
{
    private readonly SettlementDbContext _context;

    public ReportRepository(SettlementDbContext context)
    {
        _context = context;
    }

    public async Task<DailySalesSummary?> GetSummaryByDateAsync(DateOnly date)
    {
        return await _context.DailySalesSummaries.FirstOrDefaultAsync(s => s.SummaryDate == date);
    }

    public async Task UpdateSummaryAsync(DailySalesSummary summary)
    {
        summary.UpdatedAt = DateTime.UtcNow;
        var existing = await _context.DailySalesSummaries.FirstOrDefaultAsync(s => s.SummaryDate == summary.SummaryDate);
        if (existing == null)
        {
            await _context.DailySalesSummaries.AddAsync(summary);
        }
        else
        {
            _context.DailySalesSummaries.Update(summary);
        }
        await _context.SaveChangesAsync();
    }
}
