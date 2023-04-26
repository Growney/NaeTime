using System;
using System.Collections;
using System.Collections.Generic;

namespace Core.Collections
{
    public class DisposableEnumerable<T> : IEnumerable<T>, IDisposable where T : IDisposable
    {
        private IEnumerable<T> _collection;
        public DisposableEnumerable(IEnumerable<T> collection)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();

        public void Dispose()
        {
            foreach (var item in _collection)
            {
                item.Dispose();
            }
        }
    }
}
