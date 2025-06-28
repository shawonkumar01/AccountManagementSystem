using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AccountManagementSystem.Data;
using AccountManagementSystem.Models;
using System.Collections.Generic;
using System.Linq;

[Authorize(Roles = "Admin,Accountant,Viewer")]
public class DashboardModel : PageModel
{
    private readonly ChartOfAccountsRepository _accountRepo;
    private readonly VoucherRepository _voucherRepo;

    public DashboardModel(ChartOfAccountsRepository accountRepo, VoucherRepository voucherRepo)
    {
        _accountRepo = accountRepo;
        _voucherRepo = voucherRepo;
    }

    [BindProperty(SupportsGet = true)]
    public string SearchCode { get; set; }

    [BindProperty(SupportsGet = true)]
    public string ReferenceSearch { get; set; }

    public ChartOfAccount SearchedAccount { get; set; }
    public VoucherWithDetails SearchedVoucher { get; set; }

    public void OnGet()
    {
        if (!string.IsNullOrWhiteSpace(SearchCode))
        {
            SearchedAccount = _accountRepo.GetAll()
                .FirstOrDefault(a => a.AccountCode == SearchCode);
        }

        if (!string.IsNullOrWhiteSpace(ReferenceSearch))
        {
            SearchedVoucher = _voucherRepo.GetAllVouchersWithDetails()
                .FirstOrDefault(v => v.ReferenceNo == ReferenceSearch);
        }
    }
}
