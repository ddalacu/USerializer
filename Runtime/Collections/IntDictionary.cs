using System;
using UnityEngine;

namespace USerialization
{
    public sealed class IntDictionary<TValue>
    {
        private int[] _keys;
        private TValue[] _values;

        private int _count;
        private int _rehash;

        public int Count => _count;

        public int Capacity => _keys.Length;

        public IntDictionary(int capacity)
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
            _keys = new int[expanded];
            _values = new TValue[expanded];
        }

        public bool Add(int key, TValue value)
        {
            if (key == 0)
                throw new ArgumentOutOfRangeException(nameof(key));

            var capacity = _keys.Length;
            int position = ((key * 31) & 0x7FFFFFFF) % capacity;

        assign:
            var sampledKey = _keys[position];

            if (sampledKey != 0)
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

            var expandedKeys = new int[newCapacity];
            var expandedValues = new TValue[newCapacity];

            int rehashed = 0;
            for (int i = 0; i < capacity; i++)
            {
                var existingKey = _keys[i];

                if (existingKey != 0)
                {
                    int newPosition = ((existingKey * 31) & 0x7FFFFFFF) % newCapacity;
                    while (expandedKeys[newPosition] != 0)
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

        public bool TryGetValue(int key, out TValue value)
        {
            if (key == 0)
                throw new ArgumentOutOfRangeException(nameof(key));

            var capacity = _keys.Length;
            int position = ((key * 31) & 0x7FFFFFFF) % capacity;

        assign:
            var sampledKey = _keys[position];

            while (sampledKey != 0)
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

        public bool ContainsKey(int key)
        {
            if (key == 0)
                return false;

            var capacity = _keys.Length;
            int position = ((key * 31) & 0x7FFFFFFF) % capacity;

        assign:
            var sampledKey = _keys[position];

            while (sampledKey != 0)
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

        public void Foreach(Func<int, TValue, bool> onElement)
        {
            var capacity = _keys.Length;
            var done = 0;

            for (int i = 0; i < capacity && done < _count; i++)
            {
                var key = _keys[i];
                if (key != 0)
                {
                    if (onElement(key, _values[i]))
                        break;
                    done++;
                }
            }
        }

        public void Foreach(Action<int, TValue> onElement)
        {
            var capacity = _keys.Length;
            var done = 0;

            for (int i = 0; i < capacity && done < _count; i++)
            {
                var key = _keys[i];
                if (key != 0)
                {
                    onElement(key, _values[i]);
                    done++;
                }
            }
        }

        public void Foreach(Action<TValue> onElement)
        {
            var capacity = _keys.Length;
            var done = 0;

            for (int i = 0; i < capacity && done < _count; i++)
            {
                var key = _keys[i];
                if (key != 0)
                {
                    onElement(_values[i]);
                    done++;
                }
            }
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