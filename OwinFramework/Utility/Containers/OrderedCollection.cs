﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OwinFramework.Utility.Containers
{
    /// <summary>
    /// Provides a non-blocking thread-safe container for ordered items.
    /// You can use this in place of the built-in List class in places
    /// where non-blocking thread safety is needed.
    /// Note that arrays are already shread-safe and non-blocking so if
    /// you know the size of your collection this is the best choice.
    /// Internally this class maintains a linked list of arrays.
    /// </summary>
    public class OrderedCollection<T>: IDisposable, IList<T>
    {
        private readonly ArrayPool<T> _arrayPool;
        private readonly int _arrayLength;
        private readonly object _lock;
        private LinkedList<ReusableArray<T>> _arrayList;
        private int _count;

        bool ICollection<T>.IsReadOnly { get { return false; } }

        /// <summary>
        /// Returns the number of elements in the collection
        /// </summary>
        public int Count { get { return _count; } }

        /// <summary>
        /// Constructs a new ordered collection using the supplied array pool
        /// </summary>
        /// <param name="arrayPool">This collection will retrieve arrays from
        /// this pool and return them to the pool when this collection is disposed</param>
        public OrderedCollection(ArrayPool<T> arrayPool)
        {
            _arrayPool = arrayPool;
            _arrayList = new LinkedList<ReusableArray<T>>();
            _arrayLength = arrayPool.ArrayLength;
            _lock = new object();
        }

        void IDisposable.Dispose()
        {
            Clear();
        }

        /// <summary>
        /// Gets and sets individual elements in the collection by their index.
        /// The first item added is at index position 0. The last item added is
        /// at index position Count-1
        /// </summary>
        public T this[int index]
        {
            get
            {
                var arrayIndex = index / _arrayLength;
                var elementIndex = index % _arrayLength;

                var array = _arrayList.FirstElement();

                while (arrayIndex-- > 0)
                    array = array.Next;

                return array.Data[elementIndex];
            }
            set
            {
                var arrayIndex = index / _arrayLength;
                var elementIndex = index % _arrayLength;

                var array = _arrayList.FirstElement();

                while (arrayIndex-- > 0)
                    array = array.Next;

                array.Data[elementIndex] = value;
            }
        }

        /// <summary>
        /// Adds a new item to the collection
        /// </summary>
        public void Add(T item)
        {
            lock (_lock)
            {
                var elementIndex = _count % _arrayLength;
                if (elementIndex == 0)
                {
                    var newArray = _arrayPool.GetArray();
                    newArray[0] = item;
                    _arrayList.Append(newArray);
                }
                else
                {
                    var arrayIndex = _count / _arrayLength;
                    var arrayElement = _arrayList.Skip(arrayIndex).First();
                    var array = arrayElement.Data;
                    array[elementIndex] = item;
                }
                _count++;
            }
        }

        /// <summary>
        /// Adds a number of items to the collection more efficiently
        /// than adding the items individually
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            lock (_lock)
            {
                var elementIndex = _count % _arrayLength;
                var lastArray = _arrayList.LastElementOrDefault();

                foreach (var item in items)
                {
                    if (elementIndex == 0)
                        lastArray = _arrayList.Append(_arrayPool.GetArray());

                    lastArray.Data[elementIndex++] = item;

                    if (elementIndex >= _arrayLength)
                        elementIndex = 0;

                    _count++;
                }
            }
        }

        /// <summary>
        /// Removes an item from the collection and returns true if the
        /// item was present
        /// </summary>
        public bool Remove(T item)
        {
            lock (_lock)
            {
                if (_count <= 0) return false;

                var found = false;
                var oldArrayList = _arrayList;
                var remaining = _count;
                var newArrayList = new LinkedList<ReusableArray<T>>();
                var newArray = newArrayList.Append(_arrayPool.GetArray());
                var newCount = 0;
                var newElementIndex = 0;

                foreach (var arrayElement in _arrayList)
                {
                    for (var i = 0; i < _arrayLength; i++)
                    {
                        var value = arrayElement.Data[i];

                        if (remaining-- == 0)
                            break;

                        if (item.Equals(value))
                        {
                            found = true;
                            continue;
                        }

                        if (newElementIndex >= _arrayLength)
                        {
                            newArray = newArrayList.Append(_arrayPool.GetArray());
                            newElementIndex = 0;
                        }

                        newArray.Data[newElementIndex++] = value;
                        newCount++;
                    }
                }

                _arrayList = newArrayList;
                _count = newCount;
                oldArrayList.Clear(true);

                return found;
            }
        }

        /// <summary>
        /// Empties the collection
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _arrayList.Clear(true);
                _count = 0;
            }
        }

        /// <summary>
        /// Finds the index position of an item in the collection
        /// </summary>
        public int IndexOf(T item)
        {
            var index = 0;
            foreach (var array in _arrayList)
            {
                for (var i = 0; i < _arrayLength; i++)
                {
                    if (item.Equals(array.Data[i]))
                        return index + i;
                }
                index += _arrayLength;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            return !ReferenceEquals(item, null) && Enumerable.Contains(this, item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var element in this)
                array[arrayIndex++] = element;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        #region Enumerator

        private class Enumerator : IEnumerator<T>
        {
            private readonly OrderedCollection<T> _collection;
            private LinkedList<ReusableArray<T>>.ListElement _currentArrayElement;
            private int _currentIndex;
            private int _index;

            public Enumerator(OrderedCollection<T> collection)
            {
                _collection = collection;
                _index = -1;
            }

            T IEnumerator<T>.Current
            {
                get { return _currentArrayElement.Data[_currentIndex]; }
            }

            void IDisposable.Dispose()
            {
            }

            object IEnumerator.Current
            {
                get { return _currentArrayElement.Data[_currentIndex]; }
            }

            bool IEnumerator.MoveNext()
            {
                lock (_collection._lock)
                {
                    if (_collection._count == 0 || _index >= _collection._count - 1) 
                        return false;

                    if (_currentArrayElement == null)
                    {
                        _currentArrayElement = _collection._arrayList.FirstElementOrDefault();
                        _currentIndex = 0;
                        _index = 0;
                        return true;
                    }

                    _index++;

                    if (_currentIndex < _collection._arrayLength - 1)
                    {
                        _currentIndex++;
                        return true;
                    }

                    if (_currentArrayElement.Next == null) 
                        return false;

                    _currentArrayElement = _currentArrayElement.Next;
                    _currentIndex = 0;
                    return true;
                }
            }

            void IEnumerator.Reset()
            {
                _currentArrayElement = null;
            }
        }

        #endregion
    }
}
