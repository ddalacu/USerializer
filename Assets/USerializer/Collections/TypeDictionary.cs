using System;
using System.Runtime.CompilerServices;

namespace USerialization
{
    public sealed class TypeDictionary<TValue>
    {
        private Type[] _keys;
        private TValue[] _values;

        private int _count;
        private int _rehash;

        public int Count => _count;

        public int Capacity => _keys.Length;

        public TypeDictionary(int capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException("Negative capacity or 0 for dictionary!");
            }

            Initialize(capacity);
        }

        private void Initialize(int capacity)
        {
            var expanded = PrimeHelpers.ExpandPrime(capacity);

            _rehash = (int)(expanded * 0.75f);
            _keys = new Type[expanded];
            _values = new TValue[expanded];
        }

        public bool Add(Type key, TValue value)
        {
            if (key == null)
                throw new ArgumentOutOfRangeException(nameof(key));

            var capacity = _keys.Length;
            var hash = RuntimeHelpers.GetHashCode(key);
            int position = ((hash * 31) & 0x7FFFFFFF) % capacity;

            assign:
            var sampledKey = _keys[position];

            if (sampledKey != null)
            {
                if (sampledKey == key) //already added
                {
                    _values[position] = value;
                    return false;
                }

                position++;
                if (position == capacity)
                    position = 0;
                goto assign;
            }

            _count++;
            _keys[position] = key;
            _values[position] = value;

            if (_count != _rehash)
                return true;

            Expand(capacity);

            return true;
        }

        private void Expand(int capacity)
        {
            int newCapacity = PrimeHelpers.ExpandPrime(capacity);

            var expandedKeys = new Type[newCapacity];
            var expandedValues = new TValue[newCapacity];

            int rehashed = 0;
            for (int i = 0; i < capacity; i++)
            {
                var existingKey = _keys[i];

                if (existingKey != null)
                {
                    var hash = RuntimeHelpers.GetHashCode(existingKey);

                    int newPosition = ((hash * 31) & 0x7FFFFFFF) % newCapacity;
                    while (expandedKeys[newPosition] != null)
                    {
                        newPosition++;
                        if (newPosition == newCapacity)
                            newPosition = 0;
                    }

                    expandedKeys[newPosition] = existingKey;
                    expandedValues[newPosition] = _values[i];

                    rehashed++;
                    if (rehashed == _count)
                    {
                        break;
                    }
                }
            }

            _rehash = (int)(expandedKeys.Length * 0.75f);
            _keys = expandedKeys;
            _values = expandedValues;
        }

        public bool TryGetValue(Type key, out TValue value)
        {
            if (key == null)
                throw new ArgumentOutOfRangeException(nameof(key));

            var capacity = _keys.Length;
            var hash = RuntimeHelpers.GetHashCode(key);
            int position = ((hash * 31) & 0x7FFFFFFF) % capacity;

            assign:
            var sampledKey = _keys[position];

            while (sampledKey != null)
            {
                if (sampledKey == key) //already added
                {
                    value = _values[position];
                    return true;
                }

                position++;
                if (position == capacity)
                    position = 0;
                goto assign;
            }

            value = default;
            return false;
        }

        public bool ContainsKey(Type key)
        {
            if (key == null)
                return false;

            var capacity = _keys.Length;
            var hash = RuntimeHelpers.GetHashCode(key);
            int position = ((hash * 31) & 0x7FFFFFFF) % capacity;

            assign:
            var sampledKey = _keys[position];

            while (sampledKey != null)
            {
                if (sampledKey == key) //already added
                    return true;

                position++;
                if (position == capacity)
                    position = 0;
                goto assign;
            }

            return false;
        }

        public void Clear()
        {
            if (_count > 0)
            {
                Array.Clear(_keys, 0, _keys.Length);
                Array.Clear(_values, 0, _values.Length);
                _count = 0;
            }
        }

    }
}