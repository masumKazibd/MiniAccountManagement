using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniAccountManagement.Models;
using System.Data;

namespace MiniAccountManagement.Pages.Vouchers
{
    public class CreateModel : PageModel
    {
        private readonly IDbConnection _dbConnection;

        [BindProperty]
        public CreateVoucherViewModel ViewModel { get; set; }

        public CreateModel(IDbConnection connection)
        {
            _dbConnection = connection;
        }

        public async Task OnGetAsync()
        {
            await PopulateAccountList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateAccountList();
                return Page();
            }

            var totalDebit = ViewModel.Details.Sum(d => d.DebitAmount);
            var totalCredit = ViewModel.Details.Sum(d => d.CreditAmount);

            if (totalDebit != totalCredit)
            {
                TempData["Message"] =  "Total debits must equal total credits.";
                await PopulateAccountList();
                return Page();
            }
            
            var detailsTable = new DataTable();
            detailsTable.Columns.Add("AccountId", typeof(int));
            detailsTable.Columns.Add("DebitAmount", typeof(decimal));
            detailsTable.Columns.Add("CreditAmount", typeof(decimal));

            foreach (var detail in ViewModel.Details)
            {
                detailsTable.Rows.Add(detail.AccountId, detail.DebitAmount, detail.CreditAmount);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@VoucherDate", ViewModel.Header.VoucherDate);
            parameters.Add("@VoucherType", ViewModel.Header.VoucherType);
            parameters.Add("@ReferenceNo", ViewModel.Header.ReferenceNo);
            parameters.Add("@Narration", ViewModel.Header.Narration);

            parameters.Add("@Details", detailsTable.AsTableValuedParameter("dbo.VoucherDetailType"));

            await _dbConnection.ExecuteAsync("sp_SaveVoucher", parameters, commandType: CommandType.StoredProcedure);

            return RedirectToPage("/Index");
        }

        private async Task PopulateAccountList()
        {
            var accounts = await _dbConnection.QueryAsync<Account>("sp_GetChartOfAccounts", commandType: CommandType.StoredProcedure);
            ViewModel = new CreateVoucherViewModel
            {
                AccountList = new SelectList(accounts, "AccountId", "AccountName")
            };
        }
    }
}
