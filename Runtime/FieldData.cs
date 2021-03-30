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
        //public readonly FieldInfo FieldInfo;
        public readonly int FieldNameHash;
        public readonly ushort Offset;
        public readonly SerializationMethods SerializationMethods;

        public readonly int[] AlternateHashes;

        private static Type _formerlySerializedAsType;

        public FieldData(FieldInfo fieldInfo, SerializationMethods serializationMethods, ushort offset)
        {   
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
    }

}