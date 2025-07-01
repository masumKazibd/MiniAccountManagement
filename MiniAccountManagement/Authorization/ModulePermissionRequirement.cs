using Microsoft.AspNetCore.Authorization;

namespace MiniAccountManagement.Authorization
{
    public class ModulePermissionRequirement : IAuthorizationRequirement
    {
        public string ModuleName { get; set; }
        public ModulePermissionRequirement(string moduleName)
        {
            ModuleName = moduleName;
        }
    }
}
