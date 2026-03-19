using System;

namespace USerialization
{
    public sealed class TypeDictionary<TValue>
    {
        private IntPtr[] _keys;
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
            _keys = new IntPtr[expanded];
            _values = new TValue[expanded];
        }

        public bool Add(Type type, TValue value)
        {
            IntPtr handle = type.TypeHandle.Value;
            
            var hash = handle.GetHashCode();
            var capacity = _keys.Length;
            int position = (hash & 0x7FFFFFFF) % capacity;

            assign:
            var sampledKey = _keys[position];

            if (sampledKey != default)
            {
                if (sampledKey == handle) //already added
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
            _keys[position] = handle;
            _values[position] = value;

            if (_count != _rehash)
                return true;

            Expand(capacity);

            return true;
        }

        private void Expand(int capacity)
        {
            int newCapacity = PrimeHelpers.ExpandPrime(capacity);

            var expandedKeys = new IntPtr[newCapacity];
            var expandedValues = new TValue[newCapacity];

            int rehashed = 0;
            for (int i = 0; i < capacity; i++)
            {
                var existingKey = _keys[i];

                if (existingKey != default)
                {
                    var hash = existingKey.GetHashCode();

                    int newPosition = (hash & 0x7FFFFFFF) % newCapacity;
                    while (expandedKeys[newPosition] != default)
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

        public bool TryGetValue(Type type, out TValue value)
        {
            IntPtr handle = type.TypeHandle.Value;
            
            var hash = handle.GetHashCode();
            var capacity = _keys.Length;
            int position = (hash & 0x7FFFFFFF) % capacity;

            assign:
            var sampledKey = _keys[position];

            while (sampledKey != default)
            {
                if (sampledKey == handle) //already added
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