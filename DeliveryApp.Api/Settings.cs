namespace DeliveryApp.Api;

public class Settings
{
    public string CONNECTION_STRING { get; set; }
    public string GEO_SERVICE_GRPC_HOST { get; set; }
    public string MESSAGE_BROKER_HOST { get; set; }
    public string BASKET_CONFIRMED_TOPIC { get; set; } = "basket.confirmed";
    public string ORDER_STATUS_CHANGED_TOPIC { get; set; } = "order.status.changed";
}