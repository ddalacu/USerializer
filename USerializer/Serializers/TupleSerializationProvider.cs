using System;
using System.Runtime.CompilerServices;

namespace USerialization
{
    public class TupleSerializationProvider : ISerializationProvider
    {
        public bool TryGet(USerializer serializer, Type type, out DataSerializer dataSerializer)
        {
            dataSerializer = default;

            if (typeof(ITuple).IsAssignableFrom(type) == false)
                return false;

            if (type.IsValueType == false)
                dataSerializer = new ClassDataSerializer(type, () => RuntimeHelpers.GetUninitializedObject(type),
                    (fieldInfo) => true);
            else
                dataSerializer = new StructDataSerializer(type, (fieldInfo) => true);

            return true;
        }
    }
}