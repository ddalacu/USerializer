using System;

namespace USerialization
{
    /// <summary>
    /// Simply throws a exception when trying to write or read
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe class NotImplementedCustomSerializer<T> : CustomDataSerializer
    {
        public override Type SerializedType => typeof(T);

        public NotImplementedCustomSerializer() : base(DataType.None)
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