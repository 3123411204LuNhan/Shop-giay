namespace ShopGiay.Infrastructure;

public sealed class StoreSettings(IConfiguration config)
{
    private int Read(string key,int fallback,int max)
    {
        var value = config.GetValue<int?>(key) ?? fallback;
        return value is > 0 && value <= max ? value : throw new InvalidOperationException($"{key} không hợp lệ.");
    }

    public int HoldMinutes => Read("PAYMENT_HOLD_MINUTES",30,10080);
    public int ReturnDays => Read("RETURN_WINDOW_DAYS",7,365);
    public object Public => new {
        environment = config["APP_ENV"] ?? "development", holdMinutes = HoldMinutes, returnDays = ReturnDays,
        bank = new { name = config["BANK_NAME"] ?? "", account = config["BANK_ACCOUNT"] ?? "", holder = config["BANK_HOLDER"] ?? "" },
        manualPayments = true, exchangeMode = "RETURN_AND_REORDER"
    };
}
