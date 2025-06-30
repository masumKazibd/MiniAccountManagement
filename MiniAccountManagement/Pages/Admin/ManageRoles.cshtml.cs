using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace MiniAccountManagement.Data.Admin
{
    [Authorize(Roles = "Admin")]
    public class ManageRolesModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IDbConnection _dbConnection;
        public List<IdentityRole> Roles { get; set; }
        public ManageRolesModel(RoleManager<IdentityRole> roleManager, IDbConnection dbConnection)
        {
            _roleManager = roleManager;
            _dbConnection = dbConnection;
        }
        public async Task OnGetAsync()
        {
            Roles = await _roleManager.Roles.ToListAsync();
        }
        public async Task<IActionResult> OnPostCreateRole(string RoleName)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@RoleName", RoleName);
            await _dbConnection.ExecuteAsync("CreateRole", parameters, commandType: CommandType.StoredProcedure);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteRole(string RoleId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@RoleId", RoleId);
            await _dbConnection.ExecuteAsync("DeleteRole", parameters, commandType: CommandType.StoredProcedure);

            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostEditRole(string RoleId, string RoleName)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@RoleId", RoleId);
            parameters.Add("@RoleName", RoleName);

            await _dbConnection.ExecuteAsync("UpdateRole", parameters, commandType: CommandType.StoredProcedure);

            return RedirectToPage();
        }
    }
}