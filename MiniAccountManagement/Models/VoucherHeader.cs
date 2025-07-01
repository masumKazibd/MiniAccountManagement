using System.ComponentModel.DataAnnotations;

namespace MiniAccountManagement.Models
{
    public class VoucherHeader
    {
        public int VoucherId { get; set; }
        //[Required]
        public DateTime VoucherDate { get; set; } = DateTime.Today;
        //[Required]
        public string VoucherType { get; set; }
        public string ReferenceNo { get; set; }
        public string Narration { get; set; }
    }

}
