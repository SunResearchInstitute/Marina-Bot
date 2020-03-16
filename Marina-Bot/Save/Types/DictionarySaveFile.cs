using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace Marina.Save.Types
{
    //follow normal dictionary specs
    public class DictionarySaveFile<T, K> : SaveFile<Dictionary<T, K>>, ICollection<KeyValuePair<T, K>>, IEnumerable<KeyValuePair<T, K>>, IEnumerable, IDictionary<T, K>, IReadOnlyCollection<KeyValuePair<T, K>>, IReadOnlyDictionary<T, K>, ICollection, IDictionary, IDeserializationCallback, ISerializable where T : notnull
    {
        private object _syncRoot;

        public DictionarySaveFile(string name) : base(name) { }

        public K this[T key] { get => _data[key]; set => _data[key] = value; }
        public object this[object key] { get => _data[(T)key]; set => _data[(T)key] = (K)value; }

        public ICollection<T> Keys => _data.Keys;

        public ICollection<K> Values => _data.Values;

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

        IEnumerable<T> IReadOnlyDictionary<T, K>.Keys => _data.Keys;

        ICollection IDictionary.Keys => _data.Keys;

        IEnumerable<K> IReadOnlyDictionary<T, K>.Values => _data.Values;

        ICollection IDictionary.Values => _data.Values;

        public void Add(T key, K value) => _data.Add(key, value);

        public void Add(KeyValuePair<T, K> item) => _data.Add(item.Key, item.Value);

        public void Add(object key, object value) => _data.Add((T)key, (K)value);

        public void Clear() => _data.Clear();

        public bool Contains(KeyValuePair<T, K> item) => _data.ContainsKey(item.Key) && _data.ContainsValue(item.Value);

        public bool Contains(object key) => _data.ContainsKey((T)key);

        public bool ContainsKey(T key) => _data.ContainsKey(key);

        public void CopyTo(KeyValuePair<T, K>[] array, int arrayIndex) => _data.ToArray().CopyTo(array, arrayIndex);

        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentException();
            }

            if (array.Rank != 1)
            {
                throw new RankException();
            }

            if (array.GetLowerBound(0) != 0)
            {
                //??
                throw new Exception();
            }

            if (index < 0 || index > array.Length)
            {
                throw new IndexOutOfRangeException();
            }

            if (array.Length - index < _data.Count)
            {
                throw new Exception();
            }

            if (array is T[] keys)
            {
                CopyTo(keys, index);
            }
            else
            {
                if (!(array is object[] objects))
                {
                    throw new Exception();
                }

                int count = _data.Count;
                try
                {
                    for (int i = 0; i < count; i++)
                        if (_data.ElementAt(i).GetHashCode() >= 0) objects[index++] = _data.ElementAt(i).Key;
                }
                catch (ArrayTypeMismatchException exception)
                {
                    throw exception;
                }
            }
        }

        public IEnumerator<KeyValuePair<T, K>> GetEnumerator() => _data.GetEnumerator();

        public void GetObjectData(SerializationInfo info, StreamingContext context) => _data.GetObjectData(info, context);

        public void OnDeserialization(object sender) => _data.OnDeserialization(sender);

        public bool Remove(T key) => _data.Remove(key);

        public bool Remove(KeyValuePair<T, K> item) => _data.Remove(item.Key);

        public void Remove(object key) => _data.Remove((T)key);

        public bool TryGetValue(T key, [MaybeNullWhen(false)] out K value) => _data.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

        IDictionaryEnumerator IDictionary.GetEnumerator() => _data.GetEnumerator();
    }
}
