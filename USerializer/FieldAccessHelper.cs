using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace USerialization
{
    public readonly struct FieldAccessHelper<TOwner, T> where TOwner : class
    {
        private readonly int _offset;

        public FieldAccessHelper(FieldInfo fieldInfo, IRuntimeUtils helpers)
        {
            _offset = helpers.GetFieldOffset(fieldInfo);
            if (fieldInfo.FieldType.IsValueType)
            {
                if (fieldInfo.FieldType != typeof(T))
                    throw new ArgumentException("Field type is not correct");
            }
            else
            {
                if (typeof(T).IsAssignableFrom(fieldInfo.FieldType) == false)
                    throw new ArgumentException("Field type is not correct");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T GetFieldRef(ref TOwner owner)
        {
            var pinnable = Unsafe.As<TOwner, PinnableObject>(ref owner);
            fixed (byte* address = &pinnable.Pinnable)
                return ref Unsafe.AsRef<T>(address + _offset);
        }
    }
}