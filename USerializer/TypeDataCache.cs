using System;

namespace USerialization
{
    public readonly struct TypeDataCache
    {
        private readonly TypeDictionary<FieldsData> _datas;

        public TypeDataCache(int capacity)
        {
            _datas = new TypeDictionary<FieldsData>(capacity);
        }

        public bool GetTypeData(Type type, USerializer uSerializer, out FieldsData typeData)
        {
            if (_datas.TryGetValue(type, out typeData))
                return typeData != null;

            if (uSerializer.SerializationPolicy.ShouldSerialize(type) == false)
            {
                _datas.Add(type, default);
                return false;
            }

            typeData = new FieldsData();
            _datas.Add(type, typeData);//to prevent recursion when GetFields

            typeData.Fields = FieldsData.GetFields(type, uSerializer);

            //_datas.Add(type, typeData);

            return true;
        }
    }
}