using ClosedXML.Excel;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountManagement.Authorization;
using MiniAccountManagement.Models;
using System.Data;

namespace MiniAccountManagement.Pages.Accounts
{
    [ModuleAuthorize("ChartOfAccounts")]

    public class ChartOfAccountsModel : PageModel
    {
        private readonly IDbConnection _dbConnection;
        public ChartOfAccountsModel(IDbConnection connection)
        {
            _dbConnection = connection;
        }
        public List<AccountNode> AccountTree { get; set; } = new List<AccountNode>();
        public List<Account> FlatAccountList { get; set; } = new List<Account>();

        public async Task OnGetAsync()
        {
            var accounts = await _dbConnection.QueryAsync<Account>("sp_GetChartOfAccounts", commandType: CommandType.StoredProcedure);
            FlatAccountList = accounts.ToList();
            AccountTree = BuildTree(FlatAccountList);
            //await _dbConnection.ExecuteAsync("sp_GetChartOfAccounts", commandType: CommandType.StoredProcedure);
        }


        public async Task<IActionResult> OnPostCreateOrUpdateAsync(Account account)
        {
            var sql = "SELECT COUNT(1) FROM dbo.ChartOfAccounts WHERE AccountCode = @AccountCode AND AccountId != @AccountId";
            var isDuplicate = await _dbConnection.ExecuteScalarAsync<bool>(sql, new { account.AccountCode, account.AccountId });

            if (isDuplicate)
            {
                ModelState.AddModelError("Account.AccountCode", "This Account Code is already in use. Please choose another one.");

                await OnGetAsync();
                return Page();
            }

            string action = account.AccountId == 0 ? "CREATE" : "UPDATE";

            var parameters = new
            {
                Action = action,
                AccountId = account.AccountId,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                ParentAccountId = account.ParentAccountId
            };

            await _dbConnection.ExecuteAsync("sp_ManageChartOfAccounts", parameters, commandType: CommandType.StoredProcedure);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int accountId)
        {
            var Action = "DELETE";
            var parameters = new DynamicParameters();
            parameters.Add("@Action", Action);
            parameters.Add("@AccountId", accountId);
            await _dbConnection.ExecuteAsync("sp_ManageChartOfAccounts", parameters, commandType: CommandType.StoredProcedure);
            return RedirectToPage();
        }
        public async Task<IActionResult> OnGetExportAsync()
        {
            var accounts = await _dbConnection.QueryAsync<Account>("sp_GetChartOfAccounts", commandType: CommandType.StoredProcedure);
            var accountList = accounts.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Chart of Accounts");

                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Account ID";
                worksheet.Cell(currentRow, 2).Value = "Account Code";
                worksheet.Cell(currentRow, 3).Value = "Account Name";
                worksheet.Cell(currentRow, 4).Value = "Parent Account ID";

                // Style the header
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 4);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

                // Add the data rows from your account list.
                foreach (var account in accountList)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = account.AccountId;
                    worksheet.Cell(currentRow, 2).Value = account.AccountCode;
                    worksheet.Cell(currentRow, 3).Value = account.AccountName;
                    worksheet.Cell(currentRow, 4).Value = account.ParentAccountId;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    var fileName = $"ChartOfAccounts_{DateTime.Now:yyyyMMdd}.xlsx";

                    return File(content, contentType, fileName);
                }
            }
        }
        private List<AccountNode> BuildTree(List<Account> accounts)
        {
            var nodes = accounts.ToDictionary(
                acc => acc.AccountId,
                acc => new AccountNode
                {
                    AccountId = acc.AccountId,
                    AccountCode = acc.AccountCode,
                    AccountName = acc.AccountName,
                    ParentAccountId = acc.ParentAccountId
                }
            );

            var tree = new List<AccountNode>();
            foreach (var node in nodes.Values)
            {
                if (node.ParentAccountId.HasValue && nodes.ContainsKey(node.ParentAccountId.Value))
                {
                    nodes[node.ParentAccountId.Value].Children.Add(node);
                }
                else
                {
                    tree.Add(node); // This is a root node
                }
            }
            return tree;
        }
    }
}