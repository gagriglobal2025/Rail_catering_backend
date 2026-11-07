using Microsoft.EntityFrameworkCore;
using RailCat.Settlement.Api.Data;
using RailCat.Settlement.Api.Models.Entities;

namespace RailCat.Settlement.Api.Repositories;

public interface IShiftRepository
{
    Task<CashierShift?> GetActiveShiftByCashierAsync(Guid cashierUserId);
    Task<CashierShift?> GetByIdAsync(Guid shiftId);
    Task<CashierShift> AddAsync(CashierShift shift);
    Task UpdateAsync(CashierShift shift);
}

public interface IPaymentRepository
{
    Task<PaymentTransaction> AddAsync(PaymentTransaction payment);
    Task<IEnumerable<PaymentTransaction>> GetByShiftIdAsync(Guid shiftId);
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

    public async Task<CashierShift?> GetByIdAsync(Guid shiftId)
    {
        return await _context.CashierShifts
            .Include(s => s.Transactions)
            .FirstOrDefaultAsync(s => s.ShiftId == shiftId);
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
        if (await _context.DailySalesSummaries.AnyAsync(s => s.SummaryDate == summary.SummaryDate))
        {
            _context.DailySalesSummaries.Update(summary);
        }
        else
        {
            await _context.DailySalesSummaries.AddAsync(summary);
        }
        await _context.SaveChangesAsync();
    }
}

