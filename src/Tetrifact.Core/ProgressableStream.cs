using System.IO;

namespace Tetrifact.Core
{
    /// <summary>
    /// A stream abstraction that exposes progress and complete events for any underlying stream.
    /// </summary>
    public class ProgressableStream : Stream
    {
        #region FIELDS

        private Stream _baseStream = null;

        public ProgressEvent OnProgress;

        public CompleteEvent OnComplete;

        #endregion

        #region PROPERTIES

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanWrite => _baseStream.CanWrite;

        public override long Length => _baseStream.Length;

        public override long Position 
        {
            get { return _baseStream.Position; }
            set { _baseStream.Position = value;}
        }

        #endregion

        #region CTORS

        public ProgressableStream(Stream input)
        {
            _baseStream = input;
        }

        #endregion

        #region METHODS

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = _baseStream.Read(buffer, offset, count);

            OnProgress?.Invoke(_baseStream.Position, _baseStream.Length);
            
            if (_baseStream.Position == _baseStream.Length)
                OnComplete?.Invoke();

            return read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Write(buffer, offset, count);
        }

        #endregion

    }

}
