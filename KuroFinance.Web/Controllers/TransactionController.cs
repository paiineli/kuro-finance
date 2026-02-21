using System.Security.Claims;
using KuroFinance.Data.Entities;
using KuroFinance.Data.Repositories.Interfaces;
using KuroFinance.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KuroFinance.Web.Controllers;

[Authorize]
public class TransactionController(
    ITransactionRepository transactionRepo,
    ICategoryRepository    categoryRepo) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var now    = DateTime.Now;

        var transactions = await transactionRepo.GetFilteredAsync(userId, null, now.Month, now.Year);
        var categories   = await categoryRepo.GetAllByUserAsync(userId);

        var vm = new TransactionListViewModel
        {
            Transactions  = transactions,
            Categories    = categories,
            FilterType    = null,
            FilterMonth   = now.Month,
            FilterYear    = now.Year,
            TotalIncome   = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
            TotalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Filtered(string? type, int? month, int? year)
    {
        var userId = GetUserId();
        var now    = DateTime.Now;
        var m      = month ?? now.Month;
        var y      = year  ?? now.Year;

        var transactions = await transactionRepo.GetFilteredAsync(userId, type, m, y);
        var categories   = await categoryRepo.GetAllByUserAsync(userId);

        var vm = new TransactionListViewModel
        {
            Transactions  = transactions,
            Categories    = categories,
            FilterType    = type,
            FilterMonth   = m,
            FilterYear    = y,
            TotalIncome   = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
            TotalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
        };

        return PartialView("_TransactionTable", vm);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TransactionFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, error = "Dados inválidos." });

        var userId   = GetUserId();
        var category = await categoryRepo.GetByIdAsync(userId, vm.CategoryId);
        if (category is null)
            return Json(new { success = false, error = "Categoria não encontrada." });

        var transaction = new Transaction
        {
            Id          = Guid.NewGuid(),
            Description = vm.Description.Trim(),
            Amount      = vm.Amount,
            Type        = vm.Type,
            Date        = DateTime.SpecifyKind(vm.Date.Date, DateTimeKind.Utc),
            UserId      = userId,
            CategoryId  = vm.CategoryId,
            CreatedAt   = DateTime.UtcNow,
        };

        await transactionRepo.AddAsync(transaction);
        await transactionRepo.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> Update(Guid id, [FromBody] TransactionFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, error = "Dados inválidos." });

        var userId      = GetUserId();
        var transaction = await transactionRepo.GetByIdAsync(userId, id);
        if (transaction is null)
            return Json(new { success = false, error = "Transação não encontrada." });

        var category = await categoryRepo.GetByIdAsync(userId, vm.CategoryId);
        if (category is null)
            return Json(new { success = false, error = "Categoria não encontrada." });

        transaction.Description = vm.Description.Trim();
        transaction.Amount      = vm.Amount;
        transaction.Type        = vm.Type;
        transaction.Date        = DateTime.SpecifyKind(vm.Date.Date, DateTimeKind.Utc);
        transaction.CategoryId  = vm.CategoryId;

        await transactionRepo.SaveChangesAsync();
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId      = GetUserId();
        var transaction = await transactionRepo.GetByIdAsync(userId, id);
        if (transaction is null)
            return Json(new { success = false, error = "Transação não encontrada." });

        await transactionRepo.DeleteAsync(transaction);
        await transactionRepo.SaveChangesAsync();
        return Json(new { success = true });
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
