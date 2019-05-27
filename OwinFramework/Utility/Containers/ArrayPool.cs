using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Utility.Containers
{
    /// <summary>
    /// Pools and reuses arrays of a specific type and size to 
    /// avoid garbage collection.
    /// </summary>
    public class ArrayPool<T>
    {
        private readonly int _length;
        private readonly LinkedList<ReusableArray<T>> _pool;

        /// <summary>
        /// Returns the length of the arrays in this pool
        /// </summary>
        public int ArrayLength { get { return _length; } }

        /// <summary>
        /// Constructs a new pool of reusable arrays
        /// </summary>
        /// <param name="length">The length of the arrays in this pool</param>
        public ArrayPool(int length)
        {
            _length = length;
            _pool = new LinkedList<ReusableArray<T>>();
        }

        /// <summary>
        /// Gets an array from the pool, constructing a new array if necessary.
        /// Dispose of the array to put is back into the pool for reuse.
        /// </summary>
        /// <returns></returns>
        public ReusableArray<T> GetArray()
        {
            var array = _pool.PopLast();

            if (ReferenceEquals(array, null))
                array = new ReusableArray<T>(_length, a => _pool.Append(a));

            return array;
        }

        /// <summary>
        /// Deletes all of the arrays from the pool. Any arrays that are in
        /// use will still be returned to the pool when they are disposed
        /// </summary>
        public void Clear()
        {
            _pool.Clear();
        }
    }
}
