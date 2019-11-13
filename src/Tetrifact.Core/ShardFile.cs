using System.Collections.Generic;
using System.IO;

namespace Tetrifact.Core
{
    public class ShardFile 
    {
        public byte[] Data { get; private set; }
        private string[] _index;
        private long _position;

        List<ShardIndexItem> _indexItems = new List<ShardIndexItem>();

        public ShardFile(string path) 
        {
            this.Data = File.ReadAllBytes(Path.Combine(path, "bin"));
            _index = File.ReadAllLines(Path.Combine(path, "index"));

            foreach (string index in _index)
                _indexItems.Add(new ShardIndexItem(index));
        }

        public byte Next() 
        {
            return byte.MinValue;            
        }
    }
}
