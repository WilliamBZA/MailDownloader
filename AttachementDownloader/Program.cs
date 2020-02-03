using NServiceBus;
using NServiceBus.Transport.SQLServer;
using System;
using System.Threading.Tasks;

namespace AttachementDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Attachment Downloader";
            var endpointConfiguration = new EndpointConfiguration("Samples.SqlServer.AttachmentDownloader");
            endpointConfiguration.SendFailedMessagesTo("error");
            var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
            var connection = @"Data Source=(localdb)\ProjectsV13;Initial Catalog=Mail;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            transport.ConnectionString(connection);
            transport.Transactions(TransportTransactionMode.ReceiveOnly);
            
            transport.NativeDelayedDelivery().DisableTimeoutManagerCompatibility();

            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.EnableInstallers();

            SqlHelper.EnsureDatabaseExists(connection);
            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);
            Console.WriteLine("Press any key to exit");
            Console.WriteLine("Waiting for message from the Sender");
            Console.ReadKey();
            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}
