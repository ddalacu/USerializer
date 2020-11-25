using System;

namespace USerialization
{
    public class TypeData
    {
        public Type Type;
        public FieldData[] Fields;

        public void CheckDuplicateHashes()
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
        }

    }

}