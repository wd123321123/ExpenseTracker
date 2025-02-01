using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public UserController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var usersWithRoles = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            usersWithRoles.Add(new UserDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                Role = roles.ToList().FirstOrDefault()
            });
        }

        return View(usersWithRoles);
    }

    public async Task<IActionResult> AssignRole(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        var roles = await _userManager.GetRolesAsync(user);
        var model = new UserDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            Role = roles.ToList().FirstOrDefault()
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AssignRole(UserDto model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            return View(model);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var result = await _userManager.RemoveFromRolesAsync(user, currentRoles.ToArray());
        result = await _userManager.AddToRoleAsync(user, model.Role);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }
}