using Dapper;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Security.Claims;

namespace MiniAccountManagement.Authorization
{

    public class ModulePermissionHandler : AuthorizationHandler<ModulePermissionRequirement>
    {
        private readonly IDbConnection _db;

        public ModulePermissionHandler(IDbConnection db)
        {
            _db = db;
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
                    var authorizeData = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();

                    var moduleAuthorizeData = authorizeData.FirstOrDefault(d => d.Policy == "HasModulePermission" && !string.IsNullOrEmpty(d.Roles));
                    if (moduleAuthorizeData != null)
                    {
                        moduleName = moduleAuthorizeData.Roles;
                    }
                }
            }

            if (string.IsNullOrEmpty(moduleName))
            {
                return;
            }

            var parameters = new { UserId = userId, ModuleName = moduleName };
            var hasPermission = await _db.ExecuteScalarAsync<bool>("sp_CheckUserPermission", parameters, commandType: CommandType.StoredProcedure);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}
