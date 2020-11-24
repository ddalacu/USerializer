using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace USerialization
{
    [StructLayout(LayoutKind.Auto)]
    public struct FieldData
    {
        public readonly FieldInfo FieldInfo;
        public int FieldNameHash;
        public readonly SerializationMethods SerializationMethods;
        public readonly ushort Offset;

        public FieldData(FieldInfo fieldInfo, SerializationMethods serializationMethods, ushort offset)
        {
            FieldInfo = fieldInfo;
            SerializationMethods = serializationMethods;
            Offset = offset;
            FieldNameHash = fieldInfo.Name.GetInt32Hash();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T AsRef<T>(object obj)
        {
            if (FieldInfo.DeclaringType.IsInstanceOfType(obj) == false)
                throw new Exception($"Cannot get reference to \'{FieldInfo}\' because \'{obj}\' is not of type \'{FieldInfo.DeclaringType}\'");

            var address = (*(void**)Unsafe.AsPointer(ref obj));
            return ref Unsafe.AsRef<T>((byte*)address + Offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T AsRef<T>(void* ptr)
        {
            return ref Unsafe.AsRef<T>((byte*)ptr + Offset);
        }
    }

}