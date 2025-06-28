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
    public List<VoucherWithDetails> SavedVouchers { get; set; } = new();

    public void OnGet()
    {
        LoadAccounts();

        Voucher ??= new VoucherViewModel
        {
            VoucherDate = DateTime.Today, // Prevent default year 0001
            Entries = new List<VoucherEntryLine>
            {
                new VoucherEntryLine()
            }
        };

        SavedVouchers = _voucherRepo.GetAllVouchersWithDetails();
    }

    public IActionResult OnPost()
    {
        LoadAccounts();

        if (Voucher == null || Voucher.Entries == null || !Voucher.Entries.Any())
        {
            ModelState.AddModelError("", "Voucher or its entries are missing.");
            return Page();
        }

        if (Voucher.VoucherDate < new DateTime(1753, 1, 1))
        {
            ModelState.AddModelError("", "Invalid voucher date. Please select a date after 01/01/1753.");
            return Page();
        }

        if (!ModelState.IsValid || Voucher.Entries.All(e => e.DebitAmount == 0 && e.CreditAmount == 0))
        {
            ModelState.AddModelError("", "At least one entry must have a Debit or Credit amount.");
            return Page();
        }

        try
        {
            _voucherRepo.SaveVoucher(Voucher, User.Identity?.Name ?? "System");
            TempData["Message"] = "Voucher saved successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error while saving voucher: " + ex.Message);
            return Page();
        }
    }

    public IActionResult OnPostDelete(int id)
    {
        try
        {
            _voucherRepo.DeleteVoucher(id);
            TempData["Message"] = "Voucher deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error deleting voucher: " + ex.Message;
        }

        return RedirectToPage();
    }

    private void LoadAccounts()
    {
        AllAccounts = _chartRepo.GetAll();
    }
}
