using NServiceBus;

public class NewMailReceived :
    IEvent
{
    public string ExchangeMailId { get; set; }
}