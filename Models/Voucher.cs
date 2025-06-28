namespace AccountManagementSystem.Models
{
    public class VoucherWithDetails
    {
        public int Id { get; set; }
        public string VoucherType { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime VoucherDate { get; set; } = DateTime.Today;

        public string CreatedBy { get; set; }

        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
    }
}
