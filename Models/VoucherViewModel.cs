using System;
using System.Collections.Generic;

namespace AccountManagementSystem.Models
{
    
    public class VoucherViewModel
    {
        public string VoucherType { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime VoucherDate { get; set; }

        // Each voucher can have one or more entries
        public List<VoucherEntryLine> Entries { get; set; } = new List<VoucherEntryLine>();
    }

    public class VoucherEntryLine
    {
        public int AccountId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }
}
