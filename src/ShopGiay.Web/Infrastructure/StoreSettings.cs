namespace ShopGiay.Web.Infrastructure;
public sealed class StoreSettings(IConfiguration config)
{
    public int HoldMinutes => Math.Clamp(config.GetValue("Store:HoldMinutes", 30), 1, 10080);
    public int ReturnDays => Math.Clamp(config.GetValue("Store:ReturnDays", 7), 1, 365);

    // Order/ReportService → cấu hình công khai, không trả connection string hoặc SMTP password.
    public object Public() => new
    {
        environment = "development",
        holdMinutes = HoldMinutes,
        returnDays = ReturnDays,
        bank = new
        {
            name = config["Store:Bank:Name"] ?? "",
            account = config["Store:Bank:Account"] ?? "",
            holder = config["Store:Bank:Holder"] ?? ""
        },
        manualPayments = true,
        exchangeMode = "RETURN_AND_REORDER"
    };
}
