using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Serialization;

namespace USerialization
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct FieldData
    {
        public readonly FieldInfo FieldInfo;
        public readonly int FieldNameHash;
        public readonly SerializationMethods SerializationMethods;
        public readonly ushort Offset;

        public readonly int[] AlternateHashes;

        private static Type _formerlySerializedAsType;

        public FieldData(FieldInfo fieldInfo, SerializationMethods serializationMethods, ushort offset)
        {
            FieldInfo = fieldInfo;
            SerializationMethods = serializationMethods;
            Offset = offset;
            FieldNameHash = fieldInfo.Name.GetInt32Hash();

            if (_formerlySerializedAsType == null)
                _formerlySerializedAsType = typeof(FormerlySerializedAsAttribute);

            var attributes = (FormerlySerializedAsAttribute[])fieldInfo.GetCustomAttributes(_formerlySerializedAsType, false);

            if (attributes.Length != 0)
            {
                AlternateHashes = new int[attributes.Length];
                for (var index = 0; index < attributes.Length; index++)
                {
                    var formerlySerializedAsAttribute = attributes[index];
                    AlternateHashes[index] = formerlySerializedAsAttribute.oldName.GetInt32Hash();
                }
            }
            else
                AlternateHashes = null;
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