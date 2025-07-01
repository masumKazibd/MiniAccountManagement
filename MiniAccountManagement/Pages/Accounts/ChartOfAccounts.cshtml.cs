using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountManagement.Models;

namespace MiniAccountManagement.Pages.Accounts
{
    public class ChartOfAccountsModel : PageModel
    {

        // In a real app, inject your database service (e.g., using Dapper)
        // private readonly IDbConnection _db;
        // public ChartOfAccountsModel(IDbConnection db) { _db = db; }

        public List<AccountNode> AccountTree { get; set; } = new List<AccountNode>();
        public List<Account> FlatAccountList { get; set; } = new List<Account>();

        public async Task OnGetAsync()
        {
            // In a real app, you would call your sp_GetChartOfAccounts here
            // var accounts = await _db.QueryAsync<Account>("sp_GetChartOfAccounts", commandType: CommandType.StoredProcedure);
            // FlatAccountList = accounts.ToList();

            // For demonstration, using dummy data:
            FlatAccountList = GetDummyAccounts();

            // Build the hierarchical tree from the flat list
            AccountTree = BuildTree(FlatAccountList);
        }

        public async Task<IActionResult> OnPostCreateOrUpdateAsync(Account account)
        {
            // Determine if it's a Create or Update action
            string action = account.AccountId == 0 ? "CREATE" : "UPDATE";

            // In a real app, call your sp_ManageChartOfAccounts here
            /*
            var parameters = new {
                Action = action,
                AccountId = account.AccountId,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                ParentAccountId = account.ParentAccountId
            };
            await _db.ExecuteAsync("sp_ManageChartOfAccounts", parameters, commandType: CommandType.StoredProcedure);
            */

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

        // Helper method to build the tree structure
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

        // Dummy data method - replace with your actual database call
        private List<Account> GetDummyAccounts()
        {
            return new List<Account>
        {
            new Account { AccountId = 1, AccountCode = "1000", AccountName = "Assets", ParentAccountId = null },
            new Account { AccountId = 2, AccountCode = "1100", AccountName = "Current Assets", ParentAccountId = 1 },
            new Account { AccountId = 3, AccountCode = "1110", AccountName = "Cash", ParentAccountId = 2 },
            new Account { AccountId = 4, AccountCode = "1120", AccountName = "Bank", ParentAccountId = 2 },
            new Account { AccountId = 5, AccountCode = "1200", AccountName = "Fixed Assets", ParentAccountId = 1 },
            new Account { AccountId = 6, AccountCode = "2000", AccountName = "Liabilities", ParentAccountId = null },
            new Account { AccountId = 7, AccountCode = "3000", AccountName = "Equity", ParentAccountId = null }
        };
        }
    }
}