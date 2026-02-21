using System.ComponentModel.DataAnnotations;
using KuroFinance.Data.Entities;

namespace KuroFinance.Web.Models;

public class CategoryFormViewModel
{
    [Required(ErrorMessage = "Nome da categoria é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome não pode exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    public TransactionType Type { get; set; } = TransactionType.Expense;
}
