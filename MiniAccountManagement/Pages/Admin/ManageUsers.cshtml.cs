using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MiniAccountManagement.Authorization;
using System.Data;
using System.Data.Common;

namespace MiniAccountManagement.Data.Admin
{
    [Authorize(Roles = "Admin")]
    [ModuleAuthorize("UserManagement")]
    public class ManageUsersModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IDbConnection _dbConnection;
        private readonly RoleManager<IdentityRole> _roleManager;
        public List<UserWithRoles> Users { get; set; }
        public List<IdentityRole> Roles { get; set; }
        public ManageUsersModel(UserManager<IdentityUser> userManager, 
                                IDbConnection connection, 
                                RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _dbConnection = connection;
            _roleManager = roleManager;
        }

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            Roles = await _roleManager.Roles.ToListAsync();
            Users = new List<UserWithRoles>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserWithRoles
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = userRoles.ToList()
                });
            }
        }
        public async Task<IActionResult> OnPostAssignUserRole(string UserId, string RoleId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", UserId);
            parameters.Add("@RoleId", RoleId);

            await _dbConnection.ExecuteAsync("AssignUserRole", parameters, commandType: CommandType.StoredProcedure);

            return RedirectToPage();
        }
    }

    public class UserWithRoles
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }
}