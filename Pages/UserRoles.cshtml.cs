using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountManagementSystem.Pages
{
    [Authorize(Roles = "Admin")]
    public class UserRolesModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRolesModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<UserWithRole> Users { get; set; }
        public List<string> AllRoles { get; set; }

        [TempData]
        public string Message { get; set; }

        public async Task OnGetAsync()
        {
            Users = new List<UserWithRole>();
            var users = _userManager.Users.ToList();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserWithRole
                {
                    Id = user.Id,
                    Email = user.Email,
                    CurrentRole = roles.FirstOrDefault() ?? "None"
                });
            }

            AllRoles = _roleManager.Roles
                .Select(r => r.Name)
                .Where(r => r == "Admin" || r == "Accountant" || r == "Viewer")
                .ToList();
        }

        public async Task<IActionResult> OnPostAsync(string UserId, string SelectedRole)
        {
            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(SelectedRole))
            {
                Message = "Invalid request.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(UserId);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (await _roleManager.RoleExistsAsync(SelectedRole))
                {
                    await _userManager.AddToRoleAsync(user, SelectedRole);
                    Message = "Role updated successfully.";
                }
                else
                {
                    Message = "Selected role does not exist.";
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Message = "Invalid user ID.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (user.Email == User.Identity.Name)
                {
                    Message = "You cannot delete your own account.";
                    return RedirectToPage();
                }

                await _userManager.DeleteAsync(user);
                Message = "User deleted.";
            }

            return RedirectToPage();
        }

        public class UserWithRole
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public string CurrentRole { get; set; }
        }
    }
}
