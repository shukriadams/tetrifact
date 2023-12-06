using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Tests
{
    /// <summary>
    /// A collection for testing, that returns next item without requiring index management.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RotatingCollection<T>
    {
        private readonly IList<T> _items;
        private int _index;
        
        public RotatingCollection(IEnumerable<T> items)
        {
            _items = items.ToList();
        }

        public T Next()
        {
            if (_items == null || _items.Count == 0)
                return default(T);

            _index++;

            if (_index >= _items.Count)
                _index = 0;

            return _items[_index];
        }
    }
}