using AccountManagementSystem.Data;
using AccountManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Admin,Accountant")]
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
    public async Task<IActionResult> OnPostExportAsync()
    {
        var vouchers = _voucherRepo.GetAllVouchersWithDetails();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Vouchers");

        // Headers
        ws.Cells[1, 1].Value = "Voucher Type";
        ws.Cells[1, 2].Value = "Reference No";
        ws.Cells[1, 3].Value = "Voucher Date";
        ws.Cells[1, 4].Value = "Debit";
        ws.Cells[1, 5].Value = "Credit";
        ws.Cells[1, 6].Value = "Created By";

        // Data
        for (int i = 0; i < vouchers.Count; i++)
        {
            var v = vouchers[i];
            ws.Cells[i + 2, 1].Value = v.VoucherType;
            ws.Cells[i + 2, 2].Value = v.ReferenceNo;
            ws.Cells[i + 2, 3].Value = v.VoucherDate.ToString("yyyy-MM-dd");
            ws.Cells[i + 2, 4].Value = v.TotalDebit;
            ws.Cells[i + 2, 5].Value = v.TotalCredit;
            ws.Cells[i + 2, 6].Value = v.CreatedBy;
        }

        ws.Cells.AutoFitColumns();

        var stream = new MemoryStream(package.GetAsByteArray());
        var fileName = $"Vouchers_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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
