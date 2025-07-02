namespace MiniAccountManagement.Models
{
    public class ManageRolePermissionsViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }

        public List<string> AvailableModules { get; } = new List<string>
    {
        "Dashboard",
        "ChartOfAccounts",
        "VoucherEntry",
        "UserManagement",
        "RoleManagement",
        "Reports"
    };
        public List<string> AssignedModules { get; set; } = new List<string>();
    }
}