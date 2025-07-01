using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountManagement.Models;
using System.Data;

namespace MiniAccountManagement.Pages.Accounts
{
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
            // In a real app, call your sp_ManageChartOfAccounts here
            /*
            await _db.ExecuteAsync("sp_ManageChartOfAccounts", 
                new { Action = "DELETE", AccountId = accountId }, 
                commandType: CommandType.StoredProcedure);
            */

            return RedirectToPage();
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