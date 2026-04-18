using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rascar.Toolbox.Collections
{
    public class SerializableDictionary { }

    [Serializable]
    public class SerializableDictionary<TKey, TValue> :
        SerializableDictionary,
        IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        ISerializationCallbackReceiver
    {
        public const string ENUMERATION_COLLECTION_MODIFIED = "Collection was modified; enumeration operation may not execute";
        public const string ENUMERATION_CANNOT_HAPPEN = "Enumeration cannot happen";

        [SerializeField] private List<SerializableKeyValuePair> _keyValueList = new();

        private Lazy<Dictionary<TKey, uint>> _keyPositions;
        [NonSerialized] private int _version;

        private KeyCollection _keys;
        private ValueCollection _values;

        private Dictionary<TKey, uint> KeyPositions => _keyPositions.Value;
        public KeyCollection Keys => _keys ??= new KeyCollection(this);
        public ValueCollection Values => _values ??= new ValueCollection(this);
        public int Count => _keyValueList.Count;
        public bool IsReadOnly => false;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        public TValue this[TKey key]
        {
            get
            {
                return _keyValueList[(int)KeyPositions[key]]._value;
            }
            set
            {
                if (KeyPositions.TryGetValue(key, out uint index))
                {
                    SerializableKeyValuePair keyValuePair = _keyValueList[(int)index];
                    keyValuePair.SetValue(value);
                    _keyValueList[(int)index] = keyValuePair;
                }
                else
                {
                    KeyPositions[key] = (uint)_keyValueList.Count;
                    _keyValueList.Add(new SerializableKeyValuePair(key, value));
                }

                _version++;
            }
        }

        public SerializableDictionary()
        {
            _keyPositions = new Lazy<Dictionary<TKey, uint>>(MakeKeyPositions);
        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _keyPositions = new Lazy<Dictionary<TKey, uint>>(MakeKeyPositions);

            if (dictionary == null)
            {
                throw new ArgumentException("The passed dictionary is null.");
            }

            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                Add(pair.Key, pair.Value);
            }
        }

        private Dictionary<TKey, uint> MakeKeyPositions()
        {
            int entryCount = _keyValueList.Count;

            Dictionary<TKey, uint> result = new(entryCount);

            for (int entryIndex = 0; entryIndex < entryCount; ++entryIndex)
            {
                result[_keyValueList[entryIndex]._key] = (uint)entryIndex;
            }

            return result;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            // after deserialization, the key positions might be changed
            _keyPositions = new Lazy<Dictionary<TKey, uint>>(MakeKeyPositions);
        }

        public bool ContainsValue(TValue value)
        {
            if (value == null)
            {
                return _keyValueList.Exists(keyValuePair => keyValuePair._value == null);
            }

            return _keyValueList.Exists(keyValuePair => EqualityComparer<TValue>.Default.Equals(keyValuePair._value, value));
        }

        public void Add(TKey key, TValue value)
        {
            if (KeyPositions.ContainsKey(key))
            {
                throw new ArgumentException("An element with the same key already exists in the dictionary.");
            }
            else
            {
                KeyPositions[key] = (uint)_keyValueList.Count;

                _keyValueList.Add(new SerializableKeyValuePair(key, value));
                _version++;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return KeyPositions.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            if (KeyPositions.TryGetValue(key, out uint index))
            {
                Dictionary<TKey, uint> keyPositions = KeyPositions;

                keyPositions.Remove(key);

                _keyValueList.RemoveAt((int)index);

                int entryCount = _keyValueList.Count;

                for (uint entryIndex = index; entryIndex < entryCount; entryIndex++)
                {
                    keyPositions[_keyValueList[(int)entryIndex]._key] = entryIndex;
                }

                _version++;

                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (KeyPositions.TryGetValue(key, out uint index))
            {
                value = _keyValueList[(int)index]._value;

                return true;
            }

            value = default;

            return false;
        }

        public void Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            Add(keyValuePair.Key, keyValuePair.Value);
            _version++;
        }

        public bool Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            return KeyPositions.ContainsKey(keyValuePair.Key);
        }

        public bool Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            return Remove(keyValuePair.Key);
        }

        public void Clear()
        {
            _keyValueList.Clear();
            KeyPositions.Clear();
            _version++;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int keysCount = _keyValueList.Count;

            if (array.Length - arrayIndex < keysCount)
            {
                throw new ArgumentException("arrayIndex");
            }

            for (int index = 0; index < keysCount; ++index, ++arrayIndex)
            {
                SerializableKeyValuePair entry = _keyValueList[index];

                array[arrayIndex] = new KeyValuePair<TKey, TValue>(entry._key, entry._value);
            }
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            return new Dictionary<TKey, TValue>(this);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static void ThrowIfArgumentNull(string name, object argument)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void ThrowIfArgumentOutOfRange(string name, int index, int count)
        {
            ThrowIfArgumentOutOfRange(name, index, 0, count);
        }

        public static void ThrowIfArgumentOutOfRange(string name, int index, int startIndex, int count)
        {
            if (index < startIndex || index >= count)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        [Serializable]
        public struct SerializableKeyValuePair
        {
            public TKey _key;
            public TValue _value;

            public SerializableKeyValuePair(TKey key, TValue value)
            {
                _key = key;
                _value = value;
            }

            public void SetValue(TValue value)
            {
                _value = value;
            }

            public readonly KeyValuePair<TKey, TValue> ToKeyValuePair()
            {
                return new KeyValuePair<TKey, TValue>(_key, _value);
            }
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            private readonly SerializableDictionary<TKey, TValue> _dictionary;
            private readonly int _version;

            private int _currentIndex;
            private KeyValuePair<TKey, TValue> _current;

            public readonly KeyValuePair<TKey, TValue> Current => _current;

            readonly DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if (!IsValidIndex(_currentIndex))
                    {
                        throw new InvalidOperationException(ENUMERATION_CANNOT_HAPPEN);
                    }

                    return new DictionaryEntry(_current.Key, _current.Value);
                }
            }

            readonly object IDictionaryEnumerator.Key
            {
                get
                {
                    if (!IsValidIndex(_currentIndex))
                    {
                        throw new InvalidOperationException(ENUMERATION_CANNOT_HAPPEN);
                    }

                    return _current.Key;
                }
            }

            readonly object IDictionaryEnumerator.Value
            {
                get
                {
                    if (!IsValidIndex(_currentIndex))
                    {
                        throw new InvalidOperationException(ENUMERATION_CANNOT_HAPPEN);
                    }

                    return _current.Value;
                }
            }

            readonly object IEnumerator.Current
            {
                get
                {
                    if (!IsValidIndex(_currentIndex))
                    {
                        throw new InvalidOperationException(ENUMERATION_CANNOT_HAPPEN);
                    }

                    return new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);
                }
            }

            internal Enumerator(SerializableDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
                _version = dictionary._version;
                _currentIndex = 0;
                _current = new KeyValuePair<TKey, TValue>();
            }

            public bool MoveNext()
            {
                if (_version != _dictionary._version)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute");
                }

                while (_currentIndex < _dictionary._keyValueList.Count)
                {
                    _current = _dictionary._keyValueList[_currentIndex].ToKeyValuePair();
                    _currentIndex++;

                    return true;
                }

                _currentIndex = _dictionary.Count + 1;
                _current = new KeyValuePair<TKey, TValue>();

                return false;
            }

            public readonly void Dispose() { }

            void IEnumerator.Reset()
            {
                if (_version != _dictionary._version)
                {
                    throw new InvalidOperationException(ENUMERATION_COLLECTION_MODIFIED);
                }

                _currentIndex = 0;
                _current = new KeyValuePair<TKey, TValue>();
            }

            private readonly bool IsValidIndex(int index)
            {
                return index >= 0 || index < _dictionary.Count;
            }
        }

        public sealed class KeyCollection : ICollection<TKey>, ICollection, IReadOnlyCollection<TKey>
        {
            private readonly SerializableDictionary<TKey, TValue> _dictionary;

            public int Count => _dictionary.Count;

            bool ICollection<TKey>.IsReadOnly => true;
            bool ICollection.IsSynchronized => false;
            object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

            public KeyCollection(SerializableDictionary<TKey, TValue> dictionary)
            {
                ThrowIfArgumentNull(nameof(dictionary), dictionary);

                _dictionary = dictionary;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(_dictionary);
            }

            public void CopyTo(TKey[] array, int index)
            {
                ThrowIfArgumentNull(nameof(array), array);
                ThrowIfArgumentOutOfRange(nameof(index), index, array.Length);

                if (array.Length - index < _dictionary.Count)
                {
                    throw new ArgumentException("Array is too small for copy");
                }

                int count = _dictionary.Count;

                for (int dictionaryIndex = 0; dictionaryIndex < count; dictionaryIndex++)
                {
                    array[index++] = _dictionary._keyValueList[dictionaryIndex]._key;
                }
            }

            public bool Contains(TKey item)
            {
                return _dictionary.ContainsKey(item);
            }

            void ICollection<TKey>.Add(TKey item)
            {
                throw new NotSupportedException();
            }

            void ICollection<TKey>.Clear()
            {
                throw new NotSupportedException();
            }

            bool ICollection<TKey>.Remove(TKey item)
            {
                throw new NotSupportedException();
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return new Enumerator(_dictionary);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(_dictionary);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                ThrowIfArgumentNull(nameof(array), array);

                if (array.Rank != 1)
                {
                    throw new ArgumentException("Array must not be multi-dimensionnal.");
                }

                ThrowIfArgumentOutOfRange(nameof(index), index, array.Length);

                if (array.Length - index < _dictionary.Count)
                {
                    throw new ArgumentException("Array is too small for copy");
                }

                if (array is TKey[] keys)
                {
                    CopyTo(keys, index);
                }
                else
                {
                    if (array is not object[] objects)
                    {
                        throw new ArgumentException("Invalid array type");
                    }

                    int count = _dictionary.Count;

                    try
                    {
                        for (int dictionaryIndex = 0; dictionaryIndex < count; dictionaryIndex++)
                        {
                            objects[index++] = _dictionary._keyValueList[dictionaryIndex]._key;
                        }
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        throw new ArgumentException("Invalid array type");
                    }
                }
            }

            [Serializable]
            public struct Enumerator : IEnumerator<TKey>, IEnumerator
            {
                private readonly SerializableDictionary<TKey, TValue> _dictionary;
                private readonly int _version;

                private int _currentIndex;
                private TKey _currentKey;

                public readonly TKey Current => _currentKey;

                readonly object IEnumerator.Current
                {
                    get
                    {
                        if (_currentIndex == 0 || _currentIndex == _dictionary.Count + 1)
                        {
                            throw new InvalidOperationException(ENUMERATION_CANNOT_HAPPEN);
                        }

                        return _currentKey;
                    }
                }

                void IEnumerator.Reset()
                {
                    if (_version != _dictionary._version)
                    {
                        throw new InvalidOperationException(ENUMERATION_COLLECTION_MODIFIED);
                    }

                    _currentIndex = 0;
                    _currentKey = default;
                }

                internal Enumerator(SerializableDictionary<TKey, TValue> dictionary)
                {
                    _dictionary = dictionary;
                    _version = dictionary._version;
                    _currentIndex = 0;
                    _currentKey = default;
                }

                public readonly void Dispose() { }

                public bool MoveNext()
                {
                    if (_version != _dictionary._version)
                    {
                        throw new InvalidOperationException(ENUMERATION_COLLECTION_MODIFIED);
                    }

                    while (_currentIndex < _dictionary._keyValueList.Count)
                    {
                        _currentKey = _dictionary._keyValueList[_currentIndex]._key;
                        _currentIndex++;

                        return true;
                    }

                    _currentIndex = _dictionary.Count + 1;
                    _currentKey = default;

                    return false;
                }
            }
        }

        public sealed class ValueCollection : ICollection<TValue>, ICollection, IReadOnlyCollection<TValue>
        {
            private readonly SerializableDictionary<TKey, TValue> _dictionary;

            public int Count => _dictionary.Count;

            bool ICollection<TValue>.IsReadOnly => true;
            bool ICollection.IsSynchronized => false;
            object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

            public ValueCollection(SerializableDictionary<TKey, TValue> dictionary)
            {
                ThrowIfArgumentNull(nameof(dictionary), dictionary);

                _dictionary = dictionary;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(_dictionary);
            }

            public void CopyTo(TValue[] array, int index)
            {
                ThrowIfArgumentNull(nameof(array), array);
                ThrowIfArgumentOutOfRange(nameof(index), index, array.Length);

                if (array.Length - index < _dictionary.Count)
                {
                    throw new ArgumentException("Array is too small for copy");
                }

                int count = _dictionary.Count;

                for (int dictionaryIndex = 0; dictionaryIndex < count; dictionaryIndex++)
                {
                    array[index++] = _dictionary._keyValueList[dictionaryIndex]._value;
                }
            }

            public bool Contains(TValue item)
            {
                return _dictionary.ContainsValue(item);
            }

            void ICollection<TValue>.Add(TValue item)
            {
                throw new NotSupportedException();
            }

            void ICollection<TValue>.Clear()
            {
                throw new NotSupportedException();
            }

            bool ICollection<TValue>.Remove(TValue item)
            {
                throw new NotSupportedException();
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return new Enumerator(_dictionary);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(_dictionary);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                ThrowIfArgumentNull(nameof(array), array);

                if (array.Rank != 1)
                {
                    throw new ArgumentException("Array must not be multi-dimensionnal.");
                }

                ThrowIfArgumentOutOfRange(nameof(index), index, array.Length);

                if (array.Length - index < _dictionary.Count)
                {
                    throw new ArgumentException("Array is too small for copy");
                }

                if (array is TValue[] values)
                {
                    CopyTo(values, index);
                }
                else
                {
                    if (array is not object[] objects)
                    {
                        throw new ArgumentException("Invalid array type");
                    }

                    int count = _dictionary.Count;

                    try
                    {
                        for (int dictionaryIndex = 0; dictionaryIndex < count; dictionaryIndex++)
                        {
                            objects[index++] = _dictionary._keyValueList[dictionaryIndex]._key;
                        }
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        throw new ArgumentException("Invalid array type");
                    }
                }
            }

            [Serializable]
            public struct Enumerator : IEnumerator<TValue>, IEnumerator
            {
                private readonly SerializableDictionary<TKey, TValue> _dictionary;
                private readonly int _version;

                private int _currentIndex;
                private TValue _currentKey;

                public readonly TValue Current => _currentKey;

                readonly object IEnumerator.Current
                {
                    get
                    {
                        if (_currentIndex == 0 || _currentIndex == _dictionary.Count + 1)
                        {
                            throw new InvalidOperationException(ENUMERATION_CANNOT_HAPPEN);
                        }

                        return _currentKey;
                    }
                }

                void IEnumerator.Reset()
                {
                    if (_version != _dictionary._version)
                    {
                        throw new InvalidOperationException(ENUMERATION_COLLECTION_MODIFIED);
                    }

                    _currentIndex = 0;
                    _currentKey = default;
                }

                internal Enumerator(SerializableDictionary<TKey, TValue> dictionary)
                {
                    _dictionary = dictionary;
                    _version = dictionary._version;
                    _currentIndex = 0;
                    _currentKey = default;
                }

                public readonly void Dispose() { }

                public bool MoveNext()
                {
                    if (_version != _dictionary._version)
                    {
                        throw new InvalidOperationException(ENUMERATION_COLLECTION_MODIFIED);
                    }

                    while (_currentIndex < _dictionary._keyValueList.Count)
                    {
                        _currentKey = _dictionary._keyValueList[_currentIndex]._value;
                        _currentIndex++;

                        return true;
                    }

                    _currentIndex = _dictionary.Count + 1;
                    _currentKey = default;

                    return false;
                }
            }
        }
    }
}