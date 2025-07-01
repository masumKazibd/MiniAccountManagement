using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountManagement.Models;
using System.Data;

namespace MiniAccountManagement.Pages.Admin
{
    public class ManagePermissionsModel : PageModel
    {
        private readonly IDbConnection _dbConnection;
        private readonly RoleManager<IdentityRole> _roleManager;

        [BindProperty]
        public ManageRolePermissionsViewModel ViewModel { get; set; }

        public ManagePermissionsModel(IDbConnection connection, RoleManager<IdentityRole> roleManager)
        {
            _dbConnection = connection;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> OnGetAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound();
            }

            var assignedModules = await _dbConnection.QueryAsync<string>(
                "sp_GetRolePermissions",
                new { RoleId = roleId },
                commandType: CommandType.StoredProcedure);

            ViewModel = new ManageRolePermissionsViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                AssignedModules = assignedModules.ToList()
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var detailsTable = new DataTable();
            detailsTable.Columns.Add("ModuleName", typeof(string));

            var assignedModules = ViewModel.AssignedModules ?? new List<string>();

            foreach (var module in assignedModules)
            {
                detailsTable.Rows.Add(module);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@RoleId", ViewModel.RoleId);
            parameters.Add("@Modules", detailsTable.AsTableValuedParameter("dbo.ModuleListType"));

            await _dbConnection.ExecuteAsync("sp_UpdateRolePermissions", parameters, commandType: CommandType.StoredProcedure);

            TempData["SuccessMessage"] = $"Permissions for role '{ViewModel.RoleName}' updated successfully.";
            return RedirectToPage("./ManageRoles");
        }
    }
}
