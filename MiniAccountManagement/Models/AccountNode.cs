namespace MiniAccountManagement.Models
{
    public class AccountNode : Account
    {
        public List<AccountNode> Children { get; set; } = new List<AccountNode>();
    }
}
