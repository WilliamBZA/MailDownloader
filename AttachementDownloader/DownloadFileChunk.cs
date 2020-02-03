using NServiceBus;

namespace AttachementDownloader
{
    public class DownloadFileChunk : ICommand
    {
        public string ExchangeMailId { get; internal set; }
        public int StartChunkBytes { get; internal set; }
        public int EndChunkBytes { get; internal set; }
    }
}
