using System;
using UnityEngine;

namespace USerialization
{
    public class TypeData
    {
        public Type Type;
        public FieldData[] Fields;

        public void Validate()
        {
            var fieldsLength = Fields.Length;
            if (fieldsLength > 255)
                throw new Exception();
            for (int i = 0; i < fieldsLength; i++)
            {
                for (int j = 0; j < fieldsLength; j++)
                {
                    if (i == j)
                        continue;

                    if (Fields[i].FieldNameHash == Fields[j].FieldNameHash)
                        throw new Exception("Field hash collision!");
                }
            }

            //important
            Array.Sort(Fields, (a, b) =>
            {
                if (a.FieldNameHash > b.FieldNameHash)
                    return 1;
                if (a.FieldNameHash < b.FieldNameHash)
                    return -1;
                return 0;
            });
        }

        public bool GetAlternate(DataType type, int field, out FieldData compatible)
        {
            int fieldsLength = Fields.Length;

            for (var index = 0; index < fieldsLength; index++)
            {
                var fieldData = Fields[index];

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