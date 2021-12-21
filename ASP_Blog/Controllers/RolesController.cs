using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASP_Blog.Models.Identity;
using ASP_Blog.ViewModels.Account;
using ASP_Blog.ViewModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ASP_Blog.Controllers
{
    [Authorize(Roles = "admin")]
    public class RolesController : Controller
    {
        RoleManager<IdentityRole> _roleManager;
        UserManager<User> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var model = _roleManager.Roles.ToList();
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(model.RoleName));
                if (result.Succeeded)
                {
                    return RedirectToAction("Index"); // редирект на метод Index контроллера RolesController
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {

            // Ищем роль по Id
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            // Если такая роль существует, заходим в тело условия
            if (role != null)
            {
                // Удаляем роль
                await _roleManager.DeleteAsync(role);
            }
            return RedirectToAction("Index");
        }

        public IActionResult UserList()
        {
            List<User> users = _userManager.Users.ToList();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string userId)
        {
            // Ищем юзера по Id
            User user = await _userManager.FindByIdAsync(userId);
            // Если такой пользователь существует, заходим в тело условия
            if (user != null)
            {
                // Получем список всех ролей выбранного пользователя
                var userRoles = await _userManager.GetRolesAsync(user) as List<string>;
                var allRoles = _roleManager.Roles.ToList();
                ChangeRoleViewModel model = new ChangeRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    AllRoles = allRoles,
                    UserRoles = userRoles
                };
                return View(model);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(string userId, List<string> roles)
        {
            // Получаем пользователя по Id
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Получем список ролей пользователя
                var userRoles = await _userManager.GetRolesAsync(user);
                // Получаем список ролей, которые были добавлены для пользователя
                var addedRoles = roles.Except(userRoles);
                // Получаем роли, которые были удалены у пользователя
                var removedRoles = userRoles.Except(roles);

                await _userManager.AddToRolesAsync(user, addedRoles);
                await _userManager.RemoveFromRolesAsync(user, removedRoles);

                return RedirectToAction("UserList");
            }
            return NotFound();
        }
    }
}
