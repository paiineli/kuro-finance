using KuroFinance.Data.Entities;

namespace KuroFinance.Data.Repositories.Interfaces;

public record CategorySummary(string CategoryName, decimal Total);

public record MonthlySummary(
    int Month,
    int Year,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal Balance,
    List<CategorySummary> ExpensesByCategory
);

public interface ITransactionRepository
{
    Task<List<Transaction>> GetFilteredAsync(Guid userId, string? type, int? month, int? year);
    Task<Transaction?> GetByIdAsync(Guid userId, Guid id);
    Task<MonthlySummary> GetSummaryAsync(Guid userId, int month, int year);
    Task<List<MonthlySummary>> GetLast6MonthsAsync(Guid userId);
    Task<List<Transaction>> GetForExportAsync(Guid userId, int? month, int? year);
    Task AddAsync(Transaction transaction);
    Task DeleteAsync(Transaction transaction);
    Task SaveChangesAsync();
}
