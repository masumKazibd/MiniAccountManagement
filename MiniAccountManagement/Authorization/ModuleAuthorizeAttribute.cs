using Microsoft.AspNetCore.Authorization;

namespace MiniAccountManagement.Authorization
{
    public class ModuleAuthorizeAttribute : AuthorizeAttribute
    {
        public string ModuleName { get; }

        public ModuleAuthorizeAttribute(string moduleName)
        {
            Policy = "HasModulePermission";
            ModuleName = moduleName;
        }
    }
}