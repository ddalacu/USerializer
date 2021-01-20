using System;

namespace USerialization
{
    public readonly struct TypeData
    {
        public readonly Type Type;
        public readonly FieldData[] Fields;

        public TypeData(Type type, FieldData[] fields)
        {
            Type = type;

            OrderFields(fields);
            Fields = fields;
        }

        public static void OrderFields(FieldData[] fields)
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

                if (type != fieldData.SerializationMethods.DataType)
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
    }

}