using ExpenseTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    [Authorize(Roles = "FamilyHead,FamilyMember")]
    public class StatsController : Controller
    {
        private static readonly List<string> Months = new()
        {
            "Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec", "Lipiec", "Sierpień", "Wrzesień", "Październik",
            "Listopad", "Grudzień"
        };

        private readonly ApplicationDbContext _context;

        public StatsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int year = -1)
        {
            if (year == -1)
                year = DateTime.Now.Year;

            var expenses = await _context.Expenses
                .Include(e => e.User)
                .Where(e => e.Date.Year == year &&
                            (User.IsInRole("FamilyHead") || e.User.UserName == User.Identity.Name))
                .GroupBy(e => e.Date.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TotalAmount = g.Sum(e => e.Amount)
                })
                .OrderBy(e => e.Month)
                .ToListAsync();

            var availableYears = await _context.Expenses
                .Select(e => e.Date.Year)
                .Distinct()
                .OrderByDescending(year => year)
                .ToListAsync();
            var expenseData = new decimal[12];
            foreach (var expense in expenses)
            {
                expenseData[expense.Month - 1] = expense.TotalAmount;
            }

            ViewData["ExpenseData"] = expenseData;
            ViewData["Months"] = Months;
            ViewData["SelectedYear"] = year;
            ViewData["AvailableYears"] = availableYears;
            return View();
        }
    }
}