namespace StockDataAggregator.Infrastructure.Yahoo;

public class YahooClientOptions
{
    public string BaseUrl { get; set; } = "https://yfapi.net/v11/";
    public string ApiKey { get; set; } = "";
}
