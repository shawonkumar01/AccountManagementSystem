using System.Collections.Generic;

namespace YourProject.Models
{
    public class ChartOfAccount
    {
        public int Id { get; set; }
        public string AccountName { get; set; }
        public int? ParentId { get; set; }
        public string AccountCode { get; set; }
        public string AccountType { get; set; }

        public List<ChartOfAccount> Children { get; set; } = new List<ChartOfAccount>();
    }
}
