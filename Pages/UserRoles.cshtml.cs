using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccountManagementSystem.Pages
{
    [Authorize(Roles = "Admin")] // ✅ Restrict the page to Admins only
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
        }

        public async Task<IActionResult> OnPostAsync(string UserId, string SelectedRole)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, SelectedRole);
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
