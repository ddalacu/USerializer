using System;

namespace USerialization
{
    /// <summary>
    /// Simply throws a exception when trying to write or read
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe class NotImplementedCustomSerializer<T> : CustomDataSerializer
    {
        public override DataType GetDataType() => DataType.None;

        protected override void Write(void* fieldAddress, SerializerOutput output)
        {
            throw new NotImplementedException();
        }

        protected override void Read(void* fieldAddress, SerializerInput input)
        {
            throw new NotImplementedException();
        }
    }
}