using System.Security.Claims;
using ClosedXML.Excel;
using KuroFinance.Data.Entities;
using KuroFinance.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KuroFinance.Web.Controllers;

[Authorize]
public class ExportController(ITransactionRepository transactionRepo) : Controller
{
    public IActionResult Index() => View();

    public async Task<IActionResult> Excel(int? month, int? year)
    {
        var userId = GetUserId();
        var transactions = await transactionRepo.GetForExportAsync(userId, month, year);
        var bytes = GenerateExcel(transactions);

        var filename = month.HasValue
            ? $"kurofinance_{year ?? DateTime.Now.Year}_{month:D2}.xlsx"
            : "kurofinance_todas.xlsx";

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
    }

    private static byte[] GenerateExcel(List<Transaction> transactions)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Transações");

        var darkerBg  = XLColor.FromHtml("#0a0a0a");
        var darkBg    = XLColor.FromHtml("#111111");
        var altRowBg  = XLColor.FromHtml("#161616");
        var green     = XLColor.FromHtml("#4ade80");
        var red       = XLColor.FromHtml("#f87171");
        var greenDim  = XLColor.FromHtml("#182a1e");
        var redDim    = XLColor.FromHtml("#2d1d1d");
        var bodyColor = XLColor.FromHtml("#e5e7eb");
        var gray      = XLColor.FromHtml("#6b7280");

        var titleCell = sheet.Cell(1, 1);
        titleCell.Value = "kurofinance";
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontSize = 14;
        titleCell.Style.Font.FontColor = green;
        titleCell.Style.Fill.BackgroundColor = darkerBg;
        sheet.Range(1, 1, 1, 5).Merge();

        var subCell = sheet.Cell(2, 1);
        subCell.Value = "Exportado em " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        subCell.Style.Font.FontSize = 9;
        subCell.Style.Font.FontColor = gray;
        subCell.Style.Fill.BackgroundColor = darkerBg;
        sheet.Range(2, 1, 2, 5).Merge();

        var cols = new[] { "Data", "Descrição", "Tipo", "Valor", "Categoria" };
        int hdrRow = 4;
        for (int c = 0; c < cols.Length; c++)
        {
            var cell = sheet.Cell(hdrRow, c + 1);
            cell.Value = cols[c];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = green;
            cell.Style.Fill.BackgroundColor = darkBg;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            cell.Style.Border.BottomBorderColor = green;
        }

        int dataStart = hdrRow + 1;
        for (int i = 0; i < transactions.Count; i++)
        {
            var t = transactions[i];
            int row = dataStart + i;
            bool income = t.Type == TransactionType.Income;
            var rowBg = i % 2 == 0 ? darkBg : altRowBg;

            sheet.Cell(row, 1).Value = t.Date.ToString("dd/MM/yyyy");
            sheet.Cell(row, 1).Style.Font.FontColor = gray;
            sheet.Cell(row, 1).Style.Fill.BackgroundColor = rowBg;

            sheet.Cell(row, 2).Value = t.Description;
            sheet.Cell(row, 2).Style.Font.FontColor = bodyColor;
            sheet.Cell(row, 2).Style.Font.Bold = true;
            sheet.Cell(row, 2).Style.Fill.BackgroundColor = rowBg;

            sheet.Cell(row, 3).Value = income ? "Receita" : "Despesa";
            sheet.Cell(row, 3).Style.Font.Bold = true;
            sheet.Cell(row, 3).Style.Font.FontColor = income ? green : red;
            sheet.Cell(row, 3).Style.Fill.BackgroundColor = income ? greenDim : redDim;

            decimal amount = income ? t.Amount : -t.Amount;
            sheet.Cell(row, 4).Value = (double)amount;
            sheet.Cell(row, 4).Style.NumberFormat.Format = "R$ #,##0.00";
            sheet.Cell(row, 4).Style.Font.FontColor = income ? green : red;
            sheet.Cell(row, 4).Style.Font.Bold = true;
            sheet.Cell(row, 4).Style.Fill.BackgroundColor = rowBg;
            sheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            sheet.Cell(row, 5).Value = t.Category?.Name ?? "-";
            sheet.Cell(row, 5).Style.Font.FontColor = gray;
            sheet.Cell(row, 5).Style.Fill.BackgroundColor = rowBg;
        }

        if (transactions.Count > 0)
        {
            var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var expenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            var balance = income - expenses;
            int sumRow = dataStart + transactions.Count + 1;

            void SummaryRow(int r, string label, decimal value, XLColor labelColor, XLColor valueColor)
            {
                sheet.Cell(r, 3).Value = label;
                sheet.Cell(r, 3).Style.Font.Bold = true;
                sheet.Cell(r, 3).Style.Font.FontColor = labelColor;

                sheet.Cell(r, 4).Value = (double)value;
                sheet.Cell(r, 4).Style.NumberFormat.Format = "R$ #,##0.00";
                sheet.Cell(r, 4).Style.Font.Bold = true;
                sheet.Cell(r, 4).Style.Font.FontColor = valueColor;
                sheet.Cell(r, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            }

            SummaryRow(sumRow, "Total Receitas", income, green, green);
            SummaryRow(sumRow + 1, "Total Despesas", expenses, red, red);
            SummaryRow(sumRow + 2, "Saldo", balance, gray, balance >= 0 ? green : red);

            sheet.Range(sumRow, 3, sumRow, 4).Style.Border.TopBorder = XLBorderStyleValues.Medium;
            sheet.Range(sumRow, 3, sumRow, 4).Style.Border.TopBorderColor = green;
        }

        sheet.SheetView.Freeze(hdrRow, 0);
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
