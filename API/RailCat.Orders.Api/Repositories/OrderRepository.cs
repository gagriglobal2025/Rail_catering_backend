using Microsoft.EntityFrameworkCore;
using RailCat.Orders.Api.Data;
using RailCat.Orders.Api.Models.Entities;

namespace RailCat.Orders.Api.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<IEnumerable<Order>> GetActiveOrdersAsync();
    Task<IEnumerable<Order>> GetFilteredOrdersAsync(string? status, string? paymentStatus, string? search, int limit = 50);
    Task<Order> AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<DailyQueueSequence> GetOrCreateDailySequenceAsync(DateOnly date);
    Task AddPrintSpoolAsync(PrintSpool printJob);
    Task<IEnumerable<PrintSpool>> GetPendingPrintJobsAsync();
    Task<bool> MarkPrintJobPrintedAsync(Guid jobId);
}

public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderId == id);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public async Task<IEnumerable<Order>> GetActiveOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.OrderStatus != "DELIVERED" && o.OrderStatus != "CANCELLED")
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetFilteredOrdersAsync(string? status, string? paymentStatus, string? search, int limit = 50)
    {
        var query = _context.Orders.Include(o => o.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(o => o.OrderStatus == status);
        }

        if (!string.IsNullOrWhiteSpace(paymentStatus) && !paymentStatus.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(o => o.PaymentStatus == paymentStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(o => o.OrderNumber.ToLower().Contains(s) ||
                                     o.QueueToken.ToLower().Contains(s) ||
                                     (o.CustomerRef != null && o.CustomerRef.ToLower().Contains(s)));
        }

        return await query.OrderByDescending(o => o.PlacedAt).Take(limit).ToListAsync();
    }

    public async Task<Order> AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(Order order)
    {
        order.UpdatedAt = DateTime.UtcNow;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task<DailyQueueSequence> GetOrCreateDailySequenceAsync(DateOnly date)
    {
        var seq = await _context.DailyQueueSequences.FirstOrDefaultAsync(s => s.OperationalDate == date);
        if (seq == null)
        {
            seq = new DailyQueueSequence
            {
                OperationalDate = date,
                CurrentNumber = 1,
                TokenPrefix = "A",
                UpdatedAt = DateTime.UtcNow
            };
            await _context.DailyQueueSequences.AddAsync(seq);
        }
        else
        {
            seq.CurrentNumber++;
            seq.UpdatedAt = DateTime.UtcNow;
            _context.DailyQueueSequences.Update(seq);
        }
        await _context.SaveChangesAsync();
        return seq;
    }

    public async Task AddPrintSpoolAsync(PrintSpool printJob)
    {
        await _context.PrintSpools.AddAsync(printJob);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<PrintSpool>> GetPendingPrintJobsAsync()
    {
        return await _context.PrintSpools
            .Where(p => p.PrintStatus == "PENDING")
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> MarkPrintJobPrintedAsync(Guid jobId)
    {
        var job = await _context.PrintSpools.FirstOrDefaultAsync(p => p.JobId == jobId);
        if (job == null) return false;
        job.PrintStatus = "PRINTED";
        await _context.SaveChangesAsync();
        return true;
    }
}
