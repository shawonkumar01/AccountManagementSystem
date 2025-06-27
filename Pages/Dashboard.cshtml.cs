using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using AccountManagementSystem.Data;
using System.Linq;

[Authorize(Roles = "Admin,Accountant,Viewer")]
public class DashboardModel : PageModel
{
    private readonly ChartOfAccountsRepository _repo;

    public DashboardModel(ChartOfAccountsRepository repo)
    {
        _repo = repo;
    }

    [BindProperty(SupportsGet = true)]
    public string SearchCode { get; set; }

    public ChartOfAccount SearchedAccount { get; set; }

    public void OnGet()
    {
        if (!string.IsNullOrWhiteSpace(SearchCode))
        {
            var allAccounts = _repo.GetAll();
            SearchedAccount = allAccounts.FirstOrDefault(a => a.AccountCode == SearchCode);
        }
    }
}
