namespace Tetrifact.Core
{
    public class StreamProgressEventArgs
    {
        public long Position { get; }

        public long Total { get; }

        public StreamProgressEventArgs(long position, long total)
        {
            this.Position = position;
            this.Total = total;
        }

    }
}
