using KuroFinance.Data.Entities;
using KuroFinance.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KuroFinance.Data.Repositories;

public class TransactionRepository(AppDbContext db) : ITransactionRepository
{
    public Task<List<Transaction>> GetFilteredAsync(Guid userId, string? type, int? month, int? year)
    {
        var query = db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (Enum.TryParse<TransactionType>(type, ignoreCase: true, out var parsedType))
            query = query.Where(t => t.Type == parsedType);

        if (month.HasValue) query = query.Where(t => t.Date.Month == month.Value);
        if (year.HasValue)  query = query.Where(t => t.Date.Year  == year.Value);

        return query.OrderByDescending(t => t.Date).ToListAsync();
    }

    public Task<Transaction?> GetByIdAsync(Guid userId, Guid id) =>
        db.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Id == id);

    public async Task<MonthlySummary> GetSummaryAsync(Guid userId, int month, int year)
    {
        var transactions = await db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Date.Month == month && t.Date.Year == year)
            .ToListAsync();

        var income   = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var expenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        var byCategory = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category.Name)
            .Select(g => new CategorySummary(g.Key, g.Sum(t => t.Amount)))
            .OrderByDescending(c => c.Total)
            .ToList();

        return new MonthlySummary(month, year, income, expenses, income - expenses, byCategory);
    }

    public async Task<List<MonthlySummary>> GetLast6MonthsAsync(Guid userId)
    {
        var results = new List<MonthlySummary>();
        var now = DateTime.UtcNow;

        for (int i = 5; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            results.Add(await GetSummaryAsync(userId, date.Month, date.Year));
        }

        return results;
    }

    public Task<List<Transaction>> GetForExportAsync(Guid userId, int? month, int? year)
    {
        var query = db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (month.HasValue) query = query.Where(t => t.Date.Month == month.Value);
        if (year.HasValue)  query = query.Where(t => t.Date.Year  == year.Value);

        return query.OrderByDescending(t => t.Date).ToListAsync();
    }

    public async Task AddAsync(Transaction transaction) => await db.Transactions.AddAsync(transaction);

    public Task DeleteAsync(Transaction transaction)
    {
        db.Transactions.Remove(transaction);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
