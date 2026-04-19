using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace USerialization
{
    public struct NonZeroIntPtrSet
    {
        private int _count;

        private int _capacity;

        private int _rehash;

        private IntPtr[] _keys;

        public int Count => _count;

        public bool IsValid => _keys != null;

        public NonZeroIntPtrSet(int capacity)
        {
            _capacity = capacity;
            _rehash = (int)(_capacity * 0.75f);
            _count = 0;

            _keys = new IntPtr[_capacity];
        }
        
        public bool Insert(IntPtr key)
        {
            if (key == IntPtr.Zero)
                throw new ArgumentException(nameof(key));

            int position = (key.GetHashCode() & 0x7FFFFFFF) % _capacity;

            assign:
            var sampledKey = _keys[position];
            if (sampledKey != IntPtr.Zero)
            {
                if (sampledKey == key)//already added
                    return false;

                position++;
                if (position == _capacity)
                    position = 0;
                goto assign;
            }

            _count++;
            _keys[position] = key;

            if (_count != _rehash)
                return true;
            //expand
            int newCapacity = _capacity * 2;

            IntPtr[] expandedKeys = new IntPtr[newCapacity];

            int rehashed = 0;
            for (int i = 0; i < _capacity; i++)
            {
                var existingKey = _keys[i];
                if (existingKey != IntPtr.Zero)
                {
                    int newPosition = (existingKey.GetHashCode() & 0x7FFFFFFF) % newCapacity;
                    while (expandedKeys[newPosition] != IntPtr.Zero)
                    {
                        newPosition++;
                        if (newPosition == newCapacity)
                            newPosition = 0;
                    }

                    expandedKeys[newPosition] = existingKey;
                    rehashed++;
                    if (rehashed == _count)
                    {
                        break;
                    }
                }
            }

            _capacity = newCapacity;
            _rehash = (int)(_capacity * 0.75f);
            _keys = expandedKeys;

            return true;
        }
        
        public bool Contains(IntPtr key)
        {
            if (key == IntPtr.Zero)
                return false;

            int position = (key.GetHashCode() & 0x7FFFFFFF) % _capacity;

            assign:
            var sampledKey = _keys[position];

            while (sampledKey != IntPtr.Zero)
            {
                if (sampledKey == key)//already added
                    return true;

                position++;
                if (position == _capacity)
                    position = 0;
                goto assign;
            }

            return false;
        }

        public void Clear()
        {
            if (_keys != null)
            {
                Array.Clear(_keys, 0, _keys.Length);
                _count = 0;
            }
        }
    }
}