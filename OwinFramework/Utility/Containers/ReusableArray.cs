using System;
using System.Collections;
using System.Collections.Generic;

namespace OwinFramework.Utility.Containers
{
    /// <summary>
    /// This is an array that can be disposed when you are done with it
    /// to put it back into a pool of arrays available for reuse
    /// </summary>
    public class ReusableArray<T>: IDisposable, IEnumerable<T>
    {
        private readonly T[] _data;
        private readonly Action<ReusableArray<T>> _disposeAction;

        /// <summary>
        /// Constructs an array of fixed length
        /// </summary>
        /// <param name="length">The fixed length of this array</param>
        /// <param name="disposeAction">What to do when this instance is disposed</param>
        public ReusableArray(int length, Action<ReusableArray<T>> disposeAction)
        {
            _data = new T[length];
            _disposeAction = disposeAction;
        }

        /// <summary>
        /// Constructs an array of fixed length
        /// </summary>
        /// <param name="data">The array to initialize this instance with</param>
        /// <param name="disposeAction">What to do when this instance is disposed</param>
        public ReusableArray(T[] data, Action<ReusableArray<T>> disposeAction)
        {
            if (data == null) throw new ArgumentNullException(
                "data", 
                "You cannot construct a " + GetType().FullName + 
                " from a null array pointer");
            _data = data;
            _disposeAction = disposeAction;
        }

        void IDisposable.Dispose()
        {
            if (_disposeAction != null)
                _disposeAction(this);
        }

        /// <summary>
        /// Gets and sets the elements of this array
        /// </summary>
        public T this[int i]
        {
            get { return _data[i]; }
            set { _data[i] = value; }
        }

        /// <summary>
        /// Sets all elements of the array to the same value
        /// </summary>
        /// <param name="value">The value to set into every element of the array</param>
        public void Clear(T value = default(T))
        {
            for (var i = 0; i < _data.Length; i++)
                _data[i] = value;
        }

        /// <summary>
        /// Reurns the length of the array
        /// </summary>
        public int Length { get { return _data.Length; } }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        #region Enumerator

        private class Enumerator: IEnumerator<T>
        {
            private readonly ReusableArray<T> _array;
            private int _index;

            public Enumerator(ReusableArray<T> array)
            {
                _array = array;
                _index = -1;
            }

            T IEnumerator<T>.Current
            {
                get { return _array._data[_index]; }
            }

            void IDisposable.Dispose()
            {
            }

            object IEnumerator.Current
            {
                get { return _array._data[_index]; }
            }

            bool IEnumerator.MoveNext()
            {
                if (_index >= _array._data.Length - 1)
                    return false;

                _index++;
                return true;
            }

            void IEnumerator.Reset()
            {
                _index = -1;
            }
        }

        #endregion
    }
}
