using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers;

[Authorize(Roles = "FamilyHead,FamilyMember")]
public class ExpenseController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ExpenseController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string searchTerm, DateTime? startDate, DateTime? endDate, int page = 1,
        int pageSize = 10)
    {
        IQueryable<Expense> expenses = _context.Expenses;
        expenses = expenses.Include(e => e.User);
        if (User.IsInRole("FamilyMember"))
        {
            expenses = expenses.Where(e => e.User.UserName == User.Identity.Name);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            expenses = expenses.Where(e => e.Name.ToLower().Contains(searchTerm.ToLower()));
        }

        if (startDate.HasValue)
        {
            expenses = expenses.Where(e => e.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            expenses = expenses.Where(e => e.Date <= endDate.Value);
        }

        var totalItems = await expenses.CountAsync();
        var expensesList = await expenses
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderByDescending(expense => expense.Date)
            .ToListAsync();
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        return View(expensesList);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddExpense(Expense expense)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        expense.UserId = user.Id;
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}