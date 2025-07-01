using System.ComponentModel.DataAnnotations;

namespace MiniAccountManagement.Models
{
    public class VoucherDetail
    {
        [Required]
        public int AccountId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }
}
