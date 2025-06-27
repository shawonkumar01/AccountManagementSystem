using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize(Roles = "Admin,Accountant")]
public class ChartOfAccountsModel : PageModel
{
    private readonly ChartOfAccountsRepository _repo;

    public ChartOfAccountsModel(ChartOfAccountsRepository repo)
    {
        _repo = repo;
    }

    [BindProperty]
    public ChartOfAccount NewAccount { get; set; }
    public ChartOfAccount SearchedAccount { get; set; }

    public List<ChartOfAccount> AllAccounts { get; set; }
    public List<string> AccountTree { get; set; }

    public void OnGet(int? searchId)
    {
        LoadData();

        if (searchId.HasValue)
        {
            SearchedAccount = _repo.GetById(searchId.Value);
        }

        NewAccount = new ChartOfAccount(); // Prevent null form binding
    }


    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            LoadData();
            return Page();
        }

        if (NewAccount.Id == 0)
            _repo.ManageAccount("Insert", NewAccount);
        else
            _repo.ManageAccount("Update", NewAccount);

        return RedirectToPage();
    }

    public IActionResult OnPostDelete(int id)
    {
        _repo.ManageAccount("Delete", new ChartOfAccount { Id = id });
        return RedirectToPage();
    }

    public IActionResult OnPostEdit(int id)
    {
        NewAccount = _repo.GetById(id);
        LoadData();
        return Page();
    }

    private void LoadData()
    {
        AllAccounts = _repo.GetAll();
        AccountTree = BuildTree(null, "", AllAccounts);
    }

    private List<string> BuildTree(int? parentId, string indent, List<ChartOfAccount> accounts)
    {
        var lines = new List<string>();
        foreach (var acc in accounts.Where(x => x.ParentId == parentId))
        {
            lines.Add($"{indent}📁 {acc.AccountName}");
            lines.AddRange(BuildTree(acc.Id, indent + "&nbsp;&nbsp;&nbsp;", accounts));
        }
        return lines;
    }
}
