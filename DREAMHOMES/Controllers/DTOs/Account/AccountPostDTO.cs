namespace DREAMHOMES.Controllers.DTOs.Account
{
    /// <summary>
    /// Represents the structure of the creation object of Account.
    /// </summary>
    public class AccountPostDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
