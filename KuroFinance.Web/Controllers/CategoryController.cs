using System.Security.Claims;
using KuroFinance.Data.Entities;
using KuroFinance.Data.Repositories.Interfaces;
using KuroFinance.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KuroFinance.Web.Controllers;

[Authorize]
public class CategoryController(ICategoryRepository categoryRepo) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categories = await categoryRepo.GetAllByUserAsync(GetUserId());
        return View(categories);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, error = "Nome da categoria é obrigatório." });

        var userId = GetUserId();

        if (await categoryRepo.ExistsAsync(userId, vm.Name, vm.Type))
            return Json(new { success = false, error = "Já existe uma categoria com este nome e tipo." });

        var category = new Category
        {
            Id     = Guid.NewGuid(),
            Name   = vm.Name.Trim(),
            Type   = vm.Type,
            UserId = userId,
        };

        await categoryRepo.AddAsync(category);
        await categoryRepo.SaveChangesAsync();

        return Json(new { success = true, id = category.Id, name = category.Name, type = category.Type.ToString() });
    }

    [HttpPost]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, error = "Nome da categoria é obrigatório." });

        var userId   = GetUserId();
        var category = await categoryRepo.GetByIdAsync(userId, id);
        if (category is null)
            return Json(new { success = false, error = "Categoria não encontrada." });

        if (await categoryRepo.ExistsAsync(userId, vm.Name, vm.Type, excludeId: id))
            return Json(new { success = false, error = "Já existe uma categoria com este nome e tipo." });

        category.Name = vm.Name.Trim();
        category.Type = vm.Type;
        await categoryRepo.SaveChangesAsync();

        return Json(new { success = true, name = category.Name, type = category.Type.ToString() });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId   = GetUserId();
        var category = await categoryRepo.GetByIdAsync(userId, id);
        if (category is null)
            return Json(new { success = false, error = "Categoria não encontrada." });

        if (await categoryRepo.HasTransactionsAsync(userId, id))
            return Json(new { success = false, error = "Não é possível excluir uma categoria com transações vinculadas." });

        await categoryRepo.DeleteAsync(category);
        await categoryRepo.SaveChangesAsync();

        return Json(new { success = true });
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
