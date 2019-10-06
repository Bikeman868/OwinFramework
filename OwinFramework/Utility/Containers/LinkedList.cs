using System;
using System.Collections.Generic;

namespace OwinFramework.Utility.Containers
{
    /// <summary>
    /// This is a high performance thread-safe linked list whose
    /// design makes it possible to have the same objects exist on
    /// multiple lists (so that the objects can be traversed in
    /// different orders).
    /// Threads will be blocked during some operations that modify
    /// the list contents but multiple threads can concurrently
    /// enumerate the list, and the list can be safely enumerated
    /// whilst it is being modified by other threads.
    /// Also provides PopFirst and PopLast that enables the list
    /// to be used as a queue or stack.
    /// </summary>
    /// <typeparam name="T">The type of data to store in the list</typeparam>
    public class LinkedList<T> : IEnumerable<LinkedList<T>.ListElement>
    {
        private readonly object _lock = new object();
        private ListElement _head;
        private ListElement _tail;

        /// <summary>
        /// Returns true if the list is empty
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                lock (_lock) return _head == null;
            }
        }

        #region Modifying the list

        /// <summary>
        /// Removes all items from the list
        /// </summary>
        /// <param name="dispose">Pass true to call the Dispose() method
        /// on each item in the list</param>
        public void Clear(bool dispose = false)
        {
            ListElement head;

            lock (_lock)
            {
                head = _head;
                _head = null;
                _tail = null;
            }

            if (dispose)
            {
                while (head != null)
                {
                    var disposable = head.Data as IDisposable;
                    if (disposable != null) disposable.Dispose();
                    head = head.Next;
                }
            }
        }

        /// <summary>
        /// Adds a new element to the end of the list
        /// </summary>
        /// <param name="data">The data to add to the list</param>
        /// <returns>The new list element. This can be used to delete
        /// the element later, or enumerate the list starting from
        /// this element</returns>
        public ListElement Append(T data)
        {
            var listElement = new ListElement
            {
                Data = data
            };

            lock (_lock)
            {
                if (_tail == null)
                {
                    _tail = listElement;
                    _head = listElement;
                }
                else
                {
                    listElement.Prior = _tail;
                    _tail.Next = listElement;
                    _tail = listElement;
                }
            }

            return listElement;
        }

        /// <summary>
        /// Adds a new element to the start of the list
        /// </summary>
        /// <param name="data">The data to add to the list</param>
        /// <returns>The new list element. This can be used to delete
        /// the element later, or enumerate the list starting from
        /// this element</returns>
        public ListElement Prepend(T data)
        {
            var listElement = new ListElement
            {
                Data = data
            };

            lock (_lock)
            {
                if (_head == null)
                {
                    _tail = listElement;
                    _head = listElement;
                }
                else
                {
                    listElement.Next = _head;
                    _head.Prior = listElement;
                    _head = listElement;
                }
            }

            return listElement;
        }

        /// <summary>
        /// Adds an element to the list immediately after the specified element.
        /// To use this to maintain a sorted list with multiple threads inserting
        /// values you will need to block the threads for the duretion of the 
        /// find + insert operation. If you only have one thread inserting an
        /// multiple threads enumerating the list then no additional locks are 
        /// required.
        /// </summary>
        /// <param name="element">The element to add after</param>
        /// <param name="data">The data to add to the list</param>
        public ListElement InsertAfter(ListElement element, T data)
        {
            if (element == null)
                return Append(data);

            var newListElement = new ListElement
            {
                Data = data,
                Prior = element
            };

            lock (_lock)
            {
                newListElement.Next = element.Next;

                if (element.Next == null)
                    _tail = newListElement;
                else
                    element.Next.Prior = newListElement;

                element.Next = newListElement;
            }

            return newListElement;
        }

        /// <summary>
        /// Adds an element to the list immediately before the specified element
        /// To use this to maintain a sorted list with multiple threads inserting
        /// values you will need to block the threads for the duretion of the 
        /// find + insert operation. If you only have one thread inserting an
        /// multiple threads enumerating the list then no additional locks are 
        /// required.
        /// </summary>
        /// <param name="element">The element to add before</param>
        /// <param name="data">The data to add to the list</param>
        public ListElement InsertBefore(ListElement element, T data)
        {
            if (element == null)
                return Prepend(data);

            var newListElement = new ListElement
            {
                Data = data,
                Next = element
            };

            lock (_lock)
            {
                newListElement.Prior = element.Prior;

                if (element.Prior == null)
                    _head = newListElement;
                else
                    element.Prior.Next = newListElement;

                element.Prior = newListElement;
            }

            return newListElement;
        }

        /// <summary>
        /// Removes an element from anywhere in the list
        /// </summary>
        /// <param name="element">The element to remove</param>
        public void Delete(ListElement element)
        {
            lock (_lock)
            {
                if (element.Prior == null)
                    _head = element.Next;
                else
                    element.Prior.Next = element.Next;

                if (element.Next == null)
                    _tail = element.Prior;
                else
                    element.Next.Prior = element.Prior;
            }
        }

        /// <summary>
        /// Deletes elements from the list that match the supplied predicate
        /// </summary>
        /// <param name="predicate">Deletes elements that are true for this expression</param>
        /// <param name="dispose">Pass true to call the Dispose() method on deleted items</param>
        public void DeleteWhere(Func<T, bool> predicate, bool dispose = false)
        {
            lock (_lock)
            {
                var current = _head;
                while (current != null)
                {
                    if (predicate(current.Data))
                    {
                        Delete(current);
                        if (dispose)
                        {
                            var disposable = current.Data as IDisposable;
                            if (disposable != null)
                                disposable.Dispose();
                        }
                    }
                    current = current.Next;
                }
            }
        }

        /// <summary>
        /// Deletes an element from the list and all elements following
        /// it. Optionally disposes of the data in the list.
        /// </summary>
        /// <param name="element">The first element to delete</param>
        /// <param name="dispose">Pass true to call the Dispose() method on deleted items</param>
        public void Truncate(ListElement element, bool dispose)
        {
            if (element == null)
                return;

            lock (_lock)
            {
                if (element.Prior == null)
                {
                    _head = null;
                    _tail = null;
                }
                else
                {
                    element.Prior.Next = null;
                    _tail = element.Prior;
                }
            }

            if (dispose)
            {
                while (element != null)
                {
                    var disposable = element.Data as IDisposable;
                    if (disposable != null) disposable.Dispose();
                    element = element.Next;
                }
            }
        }

        #endregion

        #region Stack and queue operations

        /// <summary>
        /// Removes the first item from the list and returnes it in a thread-safe way
        /// ensuring that each thread will pop a different item from the list.
        /// </summary>
        public T PopFirst()
        {
            lock (_lock)
            {
                var result = _head;
                if (result == null) return default(T);

                _head = result.Next;
                if (_head == null)
                    _tail = null;
                else
                    _head.Prior = null;

                return result.Data;
            }
        }

        /// <summary>
        /// Removes the last item from the list and returnes it in a thread-safe way
        /// ensuring that each thread will pop a different item from the list.
        /// </summary>
        public T PopLast()
        {
            lock (_lock)
            {
                var result = _tail;
                if (result == null) return default(T);

                _tail = result.Prior;
                if (_tail == null)
                    _head = null;
                else
                    _tail.Next = null;

                return result.Data;
            }
        }

        #endregion

        #region Enumerating list elements

        /// <summary>
        /// Constructs a list of elements that match the supplied predicate
        /// </summary>
        /// <param name="predicate">Returns elements that are true for this expression</param>
        /// <returns>A list of list elements. You can pass these elements to the 
        /// Delete method to remove them from the list</returns>
        public IList<ListElement> ToElementList(Func<T, bool> predicate = null)
        {
            var result = new List<ListElement>();

            lock (_lock)
            {
                var current = _head;
                while (current != null)
                {
                    if (predicate == null || predicate(current.Data))
                        result.Add(current);
                    current = current.Next;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the first list element that matches the supplied predicate function.
        /// Returns null if there are no matching elements in the list
        /// </summary>
        /// <param name="predicate">Defines how to test list elements</param>
        /// <returns>The first list element that matches the predicate</returns>
        public ListElement FirstElementOrDefault(Func<T, bool> predicate = null)
        {
            lock (_lock)
            {
                var current = _head;
                while (current != null)
                {
                    if (predicate == null || predicate(current.Data))
                        return current;
                    current = current.Next;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the next element in the list or null if this is the last element
        /// </summary>
        /// <param name="start">The list element to start from</param>
        public ListElement NextElement(ListElement start)
        {
            return start.Next;
        }

        /// <summary>
        /// Returns the prior element in the list or null if this is the first element
        /// </summary>
        /// <param name="start">The list element to start from</param>
        public ListElement PriorElement(ListElement start)
        {
            return start.Prior;
        }

        /// <summary>
        /// Returns the first list element that matches the supplied predicate function.
        /// Throws an exception if there are no matching elements in the list
        /// </summary>
        /// <param name="predicate">Defines how to test list elements</param>
        /// <returns>The first list element that matches the predicate</returns>
        public ListElement FirstElement(Func<T, bool> predicate = null)
        {
            var element = FirstElementOrDefault(predicate);
            if (element == null)
                throw new Exception("No list element matches the supplied predicate");
            return element;
        }

        /// <summary>
        /// Returns the last list element that matches the supplied predicate function.
        /// Returns null if there are no matching elements in the list
        /// </summary>
        /// <param name="predicate">Defines how to test list elements</param>
        /// <returns>The last list element that matches the predicate</returns>
        public ListElement LastElementOrDefault(Func<T, bool> predicate = null)
        {
            lock (_lock)
            {
                var current = _tail;
                while (current != null)
                {
                    if (predicate == null || predicate(current.Data))
                        return current;
                    current = current.Prior;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the last list element that matches the supplied predicate function.
        /// Throws an exception if there are no matching elements in the list
        /// </summary>
        /// <param name="predicate">Defines how to test list elements</param>
        /// <returns>The last list element that matches the predicate</returns>
        public ListElement LastElement(Func<T, bool> predicate = null)
        {
            var element = LastElementOrDefault(predicate);
            if (element == null)
                throw new Exception("No list element matches the supplied predicate");
            return element;
        }

        /// <summary>
        /// Enumerates the rest of the list starting at the specified list element.
        /// The starting element will the Current value of the enumerator to begin
        /// with and the first call to MoveNext() will advance to the next item in
        /// the list
        /// </summary>
        /// <param name="start">Where to start. Pass null to start from the 
        /// beginning/end of the list</param>
        /// <param name="forwards">Choose whether to go forwards or backwards in
        /// the list</param>
        /// <returns>A thread-safe enumerator</returns>
        public IEnumerator<ListElement> EnumerateElementsFrom(ListElement start, bool forwards = true)
        {
            return forwards
                ? (IEnumerator<ListElement>)new ForwardElementEnumerator(this, start)
                : new ReverseElementEnumerator(this, start);
        }

        /// <summary>
        /// Implements IEnumerable so that you can use Linq expressions with the list
        /// The enumerator is thread-safe and can be reset to start back at the beginning
        /// of the list
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ListElement> GetEnumerator()
        {
            return new ForwardElementEnumerator(this, null);
        }

        /// <summary>
        /// Implements IEnumerable
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<ListElement> IEnumerable<ListElement>.GetEnumerator()
        {
            return new ForwardElementEnumerator(this, null);
        }

        #endregion

        #region Enumerating data in the list

        /// <summary>
        /// Constructs a list of data that match the supplied predicate
        /// </summary>
        /// <param name="predicate">Returns elements that are true for this expression</param>
        /// <returns>A list of list elements. You can pass these elements to the 
        /// Delete method to remove them from the list</returns>
        public IList<T> ToList(Func<T, bool> predicate = null)
        {
            var result = new List<T>();

            lock (_lock)
            {
                var current = _head;
                while (current != null)
                {
                    if (predicate == null || predicate(current.Data))
                        result.Add(current.Data);
                    current = current.Next;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the first item in the list that matches the supplied predicate function.
        /// Returns null if there are no matching elements in the list
        /// </summary>
        /// <param name="predicate">Defines how to test list elements</param>
        /// <returns>The first list element that matches the predicate</returns>
        public T FirstOrDefault(Func<T, bool> predicate = null)
        {
            lock (_lock)
            {
                var current = _head;
                while (current != null)
                {
                    if (predicate == null || predicate(current.Data))
                        return current.Data;
                    current = current.Next;
                }
            }
            return default(T);
        }

        /// <summary>
        /// Returns the first list element that matches the supplied predicate function.
        /// Throws an exception if there are no matching elements in the list
        /// </summary>
        /// <param name="predicate">Defines how to test list elements</param>
        /// <returns>The first list element that matches the predicate</returns>
        public T First(Func<T, bool> predicate = null)
        {
            var element = FirstElementOrDefault(predicate);
            if (element == null)
                throw new Exception("No list element matches the supplied predicate");
            return element.Data;
        }

        /// <summary>
        /// Returns the last list element that matches the supplied predicate function.
        /// Returns null if there are no matching elements in the list
        /// </summary>
        /// <param name="predicate">Defines how to test list elements</param>
        /// <returns>The last list element that matches the predicate</returns>
        public T LastOrDefault(Func<T, bool> predicate = null)
        {
            lock (_lock)
            {
                var current = _tail;
                while (current != null)
                {
                    if (predicate == null || predicate(current.Data))
                        return current.Data;
                    current = current.Prior;
                }
            }
            return default(T);
        }

        /// <summary>
        /// Returns the last list element that matches the supplied predicate function.
        /// Throws an exception if there are no matching elements in the list
        /// </summary>
        /// <param name="predicate">Defines how to test list elements</param>
        /// <returns>The last list element that matches the predicate</returns>
        public T Last(Func<T, bool> predicate = null)
        {
            var element = LastElementOrDefault(predicate);
            if (element == null)
                throw new Exception("No list element matches the supplied predicate");
            return element.Data;
        }

        /// <summary>
        /// Enumerates the rest of the list starting at the specified list element.
        /// The starting element will the Current value of the enumerator to begin
        /// with and the first call to MoveNext() will advance to the next item in
        /// the list
        /// </summary>
        /// <param name="start">Where to start. Pass null to start from the 
        /// beginning/end of the list</param>
        /// <param name="forwards">Choose whether to go forwards or backwards in
        /// the list</param>
        /// <returns>A thread-safe enumerator</returns>
        public IEnumerator<T> EnumerateFrom(ListElement start, bool forwards = true)
        {
            return forwards
                ? (IEnumerator<T>)new ForwardEnumerator(this, start)
                : new ReverseEnumerator(this, start);
        }

        #endregion

        #region Enumerators

        private class ForwardElementEnumerator : IEnumerator<ListElement>
        {
            private readonly LinkedList<T> _list;
            private ListElement _current;

            public ForwardElementEnumerator(LinkedList<T> list, ListElement start)
            {
                _list = list;
                _current = start;
            }

            public ListElement Current
            {
                get { return _current; }
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                if (_current == null)
                    _current = _list._head;
                else
                {
                    lock (_list._lock)
                    {
                        if (_current.Next == null)
                            return false;
                        _current = _current.Next;
                    }
                }
                return _current != null;
            }

            public void Reset()
            {
                _current = null;
            }
        }

        private class ReverseElementEnumerator : IEnumerator<ListElement>
        {
            private readonly LinkedList<T> _list;
            private ListElement _current;

            public ReverseElementEnumerator(LinkedList<T> list, ListElement start)
            {
                _list = list;
                _current = start;
            }

            public ListElement Current
            {
                get { return _current; }
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                if (_current == null)
                    _current = _list._tail;
                else
                {
                    lock (_list._lock)
                    {
                        if (_current.Prior == null)
                            return false;
                        _current = _current.Prior;
                    }
                }
                return _current != null;
            }

            public void Reset()
            {
                _current = null;
            }
        }

        private class ForwardEnumerator : IEnumerator<T>
        {
            private readonly LinkedList<T> _list;
            private ListElement _current;

            public ForwardEnumerator(LinkedList<T> list, ListElement start)
            {
                _list = list;
                _current = start;
            }

            public T Current
            {
                get { return _current == null ? default(T) : _current.Data; }
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                if (_current == null)
                    _current = _list._head;
                else
                {
                    lock (_list._lock)
                    {
                        if (_current.Next == null)
                            return false;
                        _current = _current.Next;
                    }
                }
                return _current != null;
            }

            public void Reset()
            {
                _current = null;
            }
        }

        private class ReverseEnumerator : IEnumerator<T>
        {
            private readonly LinkedList<T> _list;
            private ListElement _current;

            public ReverseEnumerator(LinkedList<T> list, ListElement start)
            {
                _list = list;
                _current = start;
            }

            public T Current
            {
                get { return _current == null ? default(T) : _current.Data; }
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                if (_current == null)
                    _current = _list._tail;
                else
                {
                    lock (_list._lock)
                    {
                        if (_current.Prior == null)
                            return false;
                        _current = _current.Prior;
                    }
                }
                return _current != null;
            }

            public void Reset()
            {
                _current = null;
            }
        }

        #endregion

        #region List elements

        /// <summary>
        /// Wrapper for an element in the list
        /// </summary>
        public class ListElement
        {
            /// <summary>
            /// The data stored in this list element
            /// </summary>
            public T Data;

            /// <summary>
            /// Pointer to the next list element
            /// </summary>
            public ListElement Next;

            /// <summary>
            /// Pointer to the prior list element
            /// </summary>
            public ListElement Prior;
        }

        #endregion
    }
}