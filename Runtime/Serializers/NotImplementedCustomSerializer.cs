using System;

namespace USerialization
{
    /// <summary>
    /// Simply throws a exception when trying to write or read
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe class NotImplementedCustomSerializer<T> : ICustomSerializer
    {
        public Type SerializedType => typeof(T);

        public void Initialize(USerializer serializer)
        {

        }

        public SerializationMethods GetMethods()
        {
            return new SerializationMethods(Write, Read, DataType.Object);
        }

        private unsafe void Read(void* fieldaddress, SerializerInput input)
        {
            throw new NotImplementedException(typeof(T).FullName);
        }

        private unsafe void Write(void* fieldaddress, SerializerOutput output)
        {
            throw new NotImplementedException(typeof(T).FullName);
        }
    }
}