using System;
using System.Reflection;

namespace USerialization
{
    public class TypeData
    {
        public readonly Type Type;
        public FieldData[] Fields;

        public TypeData(Type type)
        {
            Type = type;
        }

        private static void OrderFields(FieldData[] fields)
        {
            var fieldsLength = fields.Length;
            if (fieldsLength > 255)
                throw new Exception();
            for (var i = 0; i < fieldsLength; i++)
            {
                for (var j = 0; j < fieldsLength; j++)
                {
                    if (i == j)
                        continue;

                    if (fields[i].FieldNameHash == fields[j].FieldNameHash)
                        throw new Exception("Field hash collision!");
                }
            }

            //important
            Array.Sort(fields, (a, b) =>
            {
                if (a.FieldNameHash > b.FieldNameHash)
                    return 1;
                if (a.FieldNameHash < b.FieldNameHash)
                    return -1;
                return 0;
            });
        }

        public static bool GetAlternate(FieldData[] fields, DataType type, int field, out FieldData compatible)
        {
            int fieldsLength = fields.Length;

            for (var index = 0; index < fieldsLength; index++)
            {
                var fieldData = fields[index];

                if (type != fieldData.SerializationMethods.GetDataType())
                    continue;

                var alternateHashes = fieldData.AlternateHashes;
                if (fieldData.AlternateHashes == null)
                    continue;

                var alternateHashesLength = alternateHashes.Length;
                for (var j = 0; j < alternateHashesLength; j++)
                {
                    if (field == alternateHashes[j])
                    {
                        compatible = fieldData;
                        return true;
                    }
                }
            }

            compatible = default;
            return false;
        }

        public static FieldData[] GetFields(Type type, USerializer uSerializer)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            using (var fieldsIterator = new TypeFieldsIterator(8))
            {
                var size = fieldsIterator.Fill(type, bindingFlags);
                var fields = new FieldData[size];
                var index = 0;

                for (var i = 0; i < size; i++)
                {
                    var fieldInfo = fieldsIterator[i];

                    if (uSerializer.SerializationPolicy.ShouldSerialize(fieldInfo) == false)
                        continue;

                    if (uSerializer.TryGetDataSerializer(fieldInfo.FieldType, out var serializationMethods) == false)
                        continue;

                    var fieldOffset = UnsafeUtils.GetFieldOffset(fieldInfo);
                    if (fieldOffset > short.MaxValue)
                        throw new Exception("Field offset way to big!");

                    var alternateNames = uSerializer.SerializationPolicy.GetAlternateNames(fieldInfo);

                    fields[index] = new FieldData(fieldInfo.Name, alternateNames, serializationMethods, (ushort)fieldOffset);
                    index++;
                }

                if (index != fields.Length)
                    Array.Resize(ref fields, index);

                OrderFields(fields);

                return fields;
            }
        }

    }

}