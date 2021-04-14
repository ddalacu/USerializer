using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    public unsafe struct NonZeroIntPtrSet : IDisposable
    {
        private int _count;

        private int _capacity;

        private int _rehash;

        private IntPtr* _keys;

        private readonly Allocator _allocator;

        public int Count => _count;

        public bool IsValid => _keys != null;

        public NonZeroIntPtrSet(int capacity, Allocator allocator)
        {
            _capacity = capacity;
            _rehash = (int)(_capacity * 0.75f);
            _allocator = allocator;
            _count = 0;

            int size = _capacity * sizeof(IntPtr);
            _keys = (IntPtr*)UnsafeUtility.Malloc(size, sizeof(IntPtr), allocator);
            UnsafeUtility.MemClear(_keys, size);
        }

        [Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false)]
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

            int size = newCapacity * sizeof(IntPtr);
            IntPtr* expandedKeys = (IntPtr*)UnsafeUtility.Malloc(size, sizeof(IntPtr), _allocator);
            UnsafeUtility.MemClear(expandedKeys, size);
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
            UnsafeUtility.Free(_keys, _allocator);
            _capacity = newCapacity;
            _rehash = (int)(_capacity * 0.75f);
            _keys = expandedKeys;

            return true;
        }

        [Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false)]
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
                UnsafeUtility.MemClear(_keys, _capacity * sizeof(IntPtr));
                _count = 0;
            }
        }

        public void Dispose()
        {
            if (_keys != null)
            {
                UnsafeUtility.Free(_keys, _allocator);
                _keys = null;
            }
        }
    }
}