using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

public class MyHandler :
    IHandleMessages<NewMailReceived>, IHandleMessages<MailHasBeenCaptured>
{
    static ILog log = LogManager.GetLogger<MyHandler>();

    public async Task Handle(NewMailReceived message, IMessageHandlerContext context)
    {
        log.Info("Hello from MyHandler");

        var body = DownloadBodyFromExchange(message.ExchangeMailId);
        var subject = DownloadSubjectFromExchange(message.ExchangeMailId);

        using (var conn = new SqlConnection(@"Data Source=(localdb)\ProjectsV13;Initial Catalog=Mail;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INsert into alskdhjaklsjdjasd";

                await context.Publish(new MailHasBeenCaptured());
            }
        }
    }

    public Task Handle(MailHasBeenCaptured message, IMessageHandlerContext context)
    {
        // parse body and subject

        return Task.CompletedTask;
    }

    private string DownloadBodyFromExchange(string exchangeMailId)
    {
        return "";
    }

    private string DownloadSubjectFromExchange(string exchangeMailId)
    {
        return "";
    }
}

public class MailHasBeenCaptured
{
}