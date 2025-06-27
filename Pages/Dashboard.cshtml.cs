// ✅ Pages/Dashboard.cshtml.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccountManagementSystem.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        public void OnGet() { }
    }
}
