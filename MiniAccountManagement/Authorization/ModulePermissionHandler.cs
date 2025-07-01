using Dapper;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Security.Claims;

namespace MiniAccountManagement.Authorization
{

    public class ModulePermissionHandler : AuthorizationHandler<ModulePermissionRequirement>
    {
        private readonly IDbConnection _dbConnection;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ModulePermissionHandler(IDbConnection connection, IHttpContextAccessor httpContextAccessor)
        {
            _dbConnection = connection;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ModulePermissionRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            var parameters = new { UserId = userId, ModuleName = requirement.ModuleName };
            var hasPermission = await _dbConnection.ExecuteScalarAsync<bool>("sp_CheckUserPermission", parameters, commandType: CommandType.StoredProcedure);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}
