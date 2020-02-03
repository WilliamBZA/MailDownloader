using NServiceBus;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AttachementDownloader
{
    class NewMailHandler : Saga<MailDownloader>, IAmStartedByMessages<NewMailReceived>, IHandleMessages<DownloadFileChunk>,
        IHandleTimeouts<CheckDownloadState>
    {
        public async Task Handle(NewMailReceived message, IMessageHandlerContext context)
        {
            var attachmentStream = GetStreamForAttachment(message.ExchangeMailId);

            this.Data.TotalBytes = attachmentStream.Value;

            await context.Send(new DownloadFileChunk { ExchangeMailId = message.ExchangeMailId, StartChunkBytes = 0, EndChunkBytes = 32767 }).ConfigureAwait(false);

            await RequestTimeout<CheckDownloadState>(context, TimeSpan.FromHours(1));
        }

        public async Task Handle(DownloadFileChunk message, IMessageHandlerContext context)
        {
            byte[] bytes = GetBytesForAttachment(message.ExchangeMailId, message.StartChunkBytes, message.EndChunkBytes);

            // commit those bytes to the DB

            using (var fs = new FileStream("", FileMode.OpenOrCreate))
            {
                fs.Seek(message.StartChunkBytes, SeekOrigin.Begin);
                fs.Write(bytes);
            }

            Data.BytesCompleted += message.EndChunkBytes - message.StartChunkBytes;

            if (Data.BytesCompleted == Data.TotalBytes)
            {
                // update flag in DB to say attachment has completed
                MarkAsComplete();
            }
            else
            {
                var outstandingBytes = Data.TotalBytes - Data.BytesCompleted;

                await context.Send(new DownloadFileChunk { ExchangeMailId = message.ExchangeMailId, StartChunkBytes = message.EndChunkBytes, EndChunkBytes = message.EndChunkBytes + (int)(outstandingBytes > 32767 ? 32767 : outstandingBytes) }).ConfigureAwait(false);
            }
        }

        private byte[] GetBytesForAttachment(string exchangeMailId, int startChunkBytes, int endChunkBytes)
        {
            throw new NotImplementedException();
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MailDownloader> mapper)
        {
            mapper.ConfigureMapping<NewMailReceived>(newMail => newMail.ExchangeMailId).ToSaga(saga => saga.ExchangeMailId);
            mapper.ConfigureMapping<DownloadFileChunk>(downloadMail => downloadMail.ExchangeMailId).ToSaga(saga => saga.ExchangeMailId);
            mapper.ConfigureMapping<CheckDownloadState>(check => check.ExchangeMailId).ToSaga(saga => saga.ExchangeMailId);
        }

        private KeyValuePair<long, long> GetStreamForAttachment(string exchangeMailId)
        {
            return new KeyValuePair<long, long>(0, 8765217635);
        }

        public Task Timeout(CheckDownloadState state, IMessageHandlerContext context)
        {
            var percentage = Data.BytesCompleted / Data.TotalBytes;


            return Task.CompletedTask;
        }
    }

    public class CheckDownloadState
    {
        public string ExchangeMailId { get; set; }
    }

    public class MailDownloader : IContainSagaData
    {
        public Guid Id { get; set; }
        public string ExchangeMailId { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }

        public long BytesCompleted { get; set; }
        public long TotalBytes { get; set; }
    }
}
