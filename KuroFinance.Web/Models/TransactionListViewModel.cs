using KuroFinance.Data.Entities;

namespace KuroFinance.Web.Models;

public class TransactionListViewModel
{
    public List<Transaction> Transactions { get; set; } = [];
    public List<Category> Categories { get; set; } = [];
    public string? FilterType { get; set; }
    public int FilterMonth { get; set; }
    public int FilterYear { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
}
