using Microsoft.AspNetCore.Mvc.Rendering;

namespace MiniAccountManagement.Models
{
    public class CreateVoucherViewModel
    {
        public VoucherHeader Header { get; set; } = new VoucherHeader();
        public List<VoucherDetail> Details { get; set; } = new List<VoucherDetail>();

        public SelectList AccountList { get; set; }
    }
}
