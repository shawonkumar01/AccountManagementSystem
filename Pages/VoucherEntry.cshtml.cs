using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AccountManagementSystem.Data;
using AccountManagementSystem.Models;
using System.Collections.Generic;
using System.Linq;

public class VoucherEntryModel : PageModel
{
    private readonly ChartOfAccountsRepository _chartRepo;
    private readonly VoucherRepository _voucherRepo;

    public VoucherEntryModel(ChartOfAccountsRepository chartRepo, VoucherRepository voucherRepo)
    {
        _chartRepo = chartRepo;
        _voucherRepo = voucherRepo;
    }

    [BindProperty]
    public VoucherViewModel Voucher { get; set; }

    public List<ChartOfAccount> AllAccounts { get; set; } = new();
    public List<Voucher> SavedVouchers { get; set; } = new(); // 🔁 FIXED type

    public void OnGet()
    {
        LoadAccounts();

        // Initialize with one entry
        Voucher = new VoucherViewModel
        {
            VoucherDate = DateTime.Today,
            Entries = new List<VoucherEntryLine> { new VoucherEntryLine() }
        };

        SavedVouchers = _voucherRepo.GetAllVouchers();
    }

    public IActionResult OnPost()
    {
        LoadAccounts();

        if (Voucher?.Entries == null || Voucher.Entries.All(e => e.DebitAmount == 0 && e.CreditAmount == 0))
        {
            ModelState.AddModelError("", "Please enter at least one valid Debit or Credit entry.");
            return Page();
        }

        _voucherRepo.SaveVoucher(Voucher, User.Identity?.Name ?? "System");
        TempData["Message"] = "Voucher saved successfully.";
        if (Voucher.VoucherDate < new DateTime(1753, 1, 1))
        {
            ModelState.AddModelError("Voucher.VoucherDate", "Please select a valid date.");
            return Page();
        }

        return RedirectToPage();
    }

    private void LoadAccounts()
    {
        AllAccounts = _chartRepo.GetAll();
    }
}
