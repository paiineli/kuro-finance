namespace KuroFinance.Data.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }

    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}

public enum TransactionType
{
    Income,
    Expense
}
