using System.Security.Claims;
using KuroFinance.Data.Repositories.Interfaces;
using KuroFinance.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KuroFinance.Web.Controllers;

[Authorize]
public class DashboardController(ITransactionRepository transactionRepo) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Filtered(int? month, int? year)
    {
        var userId = GetUserId();
        var now    = DateTime.Now;
        var m      = month ?? now.Month;
        var y      = year  ?? now.Year;

        var summary = await transactionRepo.GetSummaryAsync(userId, m, y);

        return Json(new
        {
            totalIncome        = summary.TotalIncome,
            totalExpenses      = summary.TotalExpenses,
            balance            = summary.Balance,
            expensesByCategory = summary.ExpensesByCategory
                .Select(c => new { label = c.CategoryName, value = (double)c.Total }),
        });
    }

    public async Task<IActionResult> Index(int? month, int? year)
    {
        var userId = GetUserId();
        var now    = DateTime.Now;
        var m      = month ?? now.Month;
        var y      = year  ?? now.Year;

        var summary  = await transactionRepo.GetSummaryAsync(userId, m, y);
        var last6    = await transactionRepo.GetLast6MonthsAsync(userId);

        var vm = new DashboardViewModel
        {
            Month              = m,
            Year               = y,
            TotalIncome        = summary.TotalIncome,
            TotalExpenses      = summary.TotalExpenses,
            Balance            = summary.Balance,
            ExpensesByCategory = summary.ExpensesByCategory,
            Last6Months        = last6,
        };

        return View(vm);
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
