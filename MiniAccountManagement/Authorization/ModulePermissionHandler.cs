using Dapper;
using Microsoft.AspNetCore.Authorization;
using NuGet.Protocol.Plugins;
using System.Data;
using System.Security.Claims;

namespace MiniAccountManagement.Authorization
{

    public class ModulePermissionHandler : AuthorizationHandler<ModulePermissionRequirement>
    {
        private readonly IDbConnection _dbConnection;

        public ModulePermissionHandler(IDbConnection connection)
        {
            _dbConnection = connection;
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

            string moduleName = string.Empty;

            if (context.Resource is HttpContext httpContext)
            {
                var endpoint = httpContext.GetEndpoint();
                if (endpoint != null)
                {
                    var moduleAuthorizeAttribute = endpoint.Metadata.GetMetadata<ModuleAuthorizeAttribute>();
                    if (moduleAuthorizeAttribute != null)
                    {
                        moduleName = moduleAuthorizeAttribute.ModuleName;
                    }
                }
            }

            if (string.IsNullOrEmpty(moduleName))
            {
                context.Fail();
                return;
            }

            var parameters = new { UserId = userId, ModuleName = moduleName };
            var hasPermission = await _dbConnection.ExecuteScalarAsync<bool>("sp_CheckUserPermission", parameters, commandType: CommandType.StoredProcedure);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}
