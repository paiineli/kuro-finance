using KuroFinance.Data.Repositories.Interfaces;

namespace KuroFinance.Web.Models;

public class DashboardViewModel
{
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance { get; set; }
    public List<CategorySummary> ExpensesByCategory { get; set; } = [];
    public List<MonthlySummary> Last6Months { get; set; } = [];
}
