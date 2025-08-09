namespace StockDataAggregator.Api.Options;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Key { get; set; } = "";
    public int ExpiresMinutes { get; set; } = 60;
    public string Username { get; set; } = "user";
    public string Password { get; set; } = "";
}
