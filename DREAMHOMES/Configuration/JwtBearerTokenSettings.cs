namespace DREAMHOMES.Configuration
{
    public class JwtBearerTokenSettings
    {
        public string SecretKey { get; set; } = null!;

        public string Audience { get; set; } = null!;

        public string Issuer { get; set; } = null!;

        public int ExpiryTimeInMinutes { get; set; } 
    }
}
