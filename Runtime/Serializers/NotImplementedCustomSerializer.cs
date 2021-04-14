using System;

namespace USerialization
{
    /// <summary>
    /// Simply throws a exception when trying to write or read
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe class NotImplementedCustomSerializer<T> : DataSerializer, ICustomSerializer
    {
        public Type SerializedType => typeof(T);

        public void Initialize(USerializer serializer)
        {

        }

        public DataSerializer GetMethods()
        {
            return this;
        }

        public NotImplementedCustomSerializer() : base((DataType)0)
        {

        }

        public override void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            throw new NotImplementedException();
        }

        public override void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            throw new NotImplementedException();
        }
    }
}