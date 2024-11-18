using System;
using System.IO;
using System.Threading.Tasks;

namespace Tetrifact.Core
{
    public class StreamProgressCopy
    {
        #region FIELDS
       
        private readonly Stream _source;
        private readonly Stream _target;
        private readonly int _bufSize;
        private bool _hasRun;
        public ProgressEvent OnProgress;

        #endregion

        #region CTORS

        public StreamProgressCopy(Stream source, Stream target, int bufSize)
        {
            _source = source;
            _target = target;
            _bufSize = bufSize;
        }

        #endregion

        #region METHODS

        public async Task Work()
        {
            if (_hasRun)
                throw new Exception("This instance has already copied.");

            _hasRun = true;

            while (_source.Position < _source.Length)
            {
                long lookup = _source.Length - _source.Position;
                int blockSize = lookup < _bufSize ? (int)lookup : _bufSize;
                byte[] buffer = new byte[blockSize];

                await _source.ReadAsync(buffer, 0, blockSize);
                await _target.WriteAsync(buffer);

                this.OnProgress?.Invoke(_source.Position, _source.Length);
            }
            
        }

        #endregion
    }
}
