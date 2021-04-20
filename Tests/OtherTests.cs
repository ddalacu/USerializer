using System;
using System.IO;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Tests;
using UnityEngine;
using USerialization;

[assembly: CustomSerializer(typeof(OtherTests.SkipMeSerializer))]

public class OtherTests
{
    public struct SkipMe
    {
        public int Value;
    }

    public class SkipMeDataType : IDataTypeLogic
    {
        public DataType Value { get; set; }

        public unsafe void Skip(SerializerInput input)
        {
            input.Skip(sizeof(SkipMe));
        }
    }

    public class SkipMeSerializer : CustomDataSerializer
    {
        public override Type SerializedType => typeof(SkipMe);

        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out SkipMeDataType arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            ref var instance = ref Unsafe.AsRef<SkipMe>(fieldAddress);
            output.WriteInt(instance.Value);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            ref var instance = ref Unsafe.AsRef<SkipMe>(fieldAddress);
            instance.Value = input.ReadInt();
        }
    }

    [Serializable]
    public class FormA
    {
        public int A;
        public SkipMe B;
        public int C;
    }

    [Serializable]
    public class FormB
    {
        public int A;
        public int C;
    }

    [Test]
    public void SkipCustomDataField()
    {
        var a = new FormA();

        var providers = ProvidersUtils.GetDefaultProviders();
        var dataTypeLogic = new DataTypesDatabase();

        dataTypeLogic.Register<SkipMeDataType>();

        var serializer = new USerializer(new UnitySerializationPolicy(), providers, dataTypeLogic);

        var stream = new MemoryStream();

        var serializerOutput = new SerializerOutput(1024, stream);
        serializer.Serialize(serializerOutput, a);
        serializerOutput.Flush();

        var end = stream.Position;
        stream.Position = 0;

        serializer.TryDeserialize(new SerializerInput(1024, stream), out FormB b);

        Debug.Assert(end == stream.Position);

        Debug.Assert(a.A == b.A);
        Debug.Assert(a.C == b.C);
    }

}
