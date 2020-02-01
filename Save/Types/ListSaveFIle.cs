using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Marina.Save.Types
{
    public class ListSaveFile<T> : SaveFile<List<T>>, IList<T>, IList, IReadOnlyList<T>
    {
        [NonSerialized]
        private object _syncRoot;


        public ListSaveFile(string name) : base(name) { }

        public T this[int index] { get => _data[index]; set => _data[index] = value; }
        object IList.this[int index] { get => _data[index]; set => _data[index] = (T)value; }

        public int Count => _data.Count;

        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        public bool IsSynchronized => false;

        public object SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                    Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);

                return _syncRoot;
            }
        }

        public void Add(T item) => _data.Add(item);

        public int Add(object value)
        {
            if (value == null && !(default(T) == null))
                throw new ArgumentNullException();

            try
            {
                Add((T)value);
            }
            catch (InvalidCastException exception)
            {
                throw exception;
            }

            return Count - 1;
        }

        public void Clear() => _data.Clear();

        public bool Contains(T item) => _data.Contains(item);

        public bool Contains(object value) => _data.Contains((T)value);

        public void CopyTo(T[] array, int arrayIndex) => _data.CopyTo(array, arrayIndex);

        public void CopyTo(Array array, int index)
        {
            if ((array != null) && (array.Rank != 1))
                throw new ArgumentException();

            Contract.EndContractBlock();

            try
            {
                // Array.Copy will check for NULL.
                Array.Copy(_data.ToArray(), 0, array, index, _data.Count);
            }
            catch (ArrayTypeMismatchException exception)
            {
                throw exception;
            }
        }

        public IEnumerator<T> GetEnumerator() => _data.GetEnumerator();

        public int IndexOf(T item) => _data.IndexOf(item);

        public int IndexOf(object value) => _data.IndexOf((T)value);

        public void Insert(int index, T item) => _data.Insert(index, item);

        public void Insert(int index, object value) => _data.Insert(index, (T)value);

        public bool Remove(T item) => _data.Remove(item);

        public void Remove(object value) => _data.Remove((T)value);

        public void RemoveAt(int index) => _data.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();
    }
}
