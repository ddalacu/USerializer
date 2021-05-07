using System;

namespace USerialization
{
    public readonly struct FieldDataCache
    {
        private readonly TypeDictionary<FieldsData> _datas;

        public FieldDataCache(int capacity)
        {
            _datas = new TypeDictionary<FieldsData>(capacity);
        }

        public bool GetTypeData(Type type, USerializer uSerializer, out FieldsData typeData)
        {
            if (_datas.TryGetValue(type, out typeData))
                return typeData != null;

            typeData = new FieldsData();
            _datas.Add(type, typeData);//to prevent recursion when GetFields

            typeData.Fields = FieldsData.GetFields(type, uSerializer);

            return true;
        }
    }
}