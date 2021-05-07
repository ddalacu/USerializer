using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    public class CircularReferenceException : Exception
    {
        public CircularReferenceException(string message) : base(message)
        {

        }
    }

    public unsafe class ClassSerializationProvider : ISerializationProvider
    {
        private USerializer _serializer;

        private FieldDataCache _fieldDataCache;

        public void Initialize(USerializer serializer)
        {
            _serializer = serializer;
            _fieldDataCache = new FieldDataCache(512);
        }

        public void Start(USerializer serializer)
        {

        }

        public bool TryGet(Type type, out DataSerializer serializationMethods)
        {
            serializationMethods = default;

            if (_serializer.DataTypesDatabase.TryGet(out ObjectDataTypeLogic objectDataTypeLogic) == false)
                return false;

            if (type.IsArray)
                return false;

            if (type.IsValueType)
                return false;

            if (type.IsPrimitive)
                return false;

            if (_serializer.SerializationPolicy.ShouldSerialize(type) == false)
                return false;

            if (_fieldDataCache.GetTypeData(type, _serializer, out var fieldsData) == false)
            {
                serializationMethods = default;
                return false;
            }

            var fieldsSerializer = new FieldsSerializer(fieldsData, _serializer.DataTypesDatabase);

            serializationMethods = new ClassDataSerializer(type, fieldsSerializer, objectDataTypeLogic.Value);

            return true;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public sealed class ClassDataSerializer : DataSerializer
        {
            private readonly Type _type;
            private FieldsSerializer _fieldsSerializer;
            private readonly bool _haveCtor;

            private readonly DataType _dataType;

            public override DataType GetDataType() => _dataType;

            public ClassDataSerializer(Type type, FieldsSerializer fieldsSerializer, DataType objectDataType)
            {
                if (type == null)
                    throw new ArgumentNullException(nameof(type));

                if (type.IsValueType)
                    throw new ArgumentException(nameof(type));

                _type = type;
                _fieldsSerializer = fieldsSerializer;
                var constructor = _type.GetConstructor(Type.EmptyTypes);
                _haveCtor = constructor != null;
                _dataType = objectDataType;
            }

            private int _stack;

            private const int MaxStack = 32;

            public override void WriteDelegate(void* fieldAddress, SerializerOutput output)
            {
                var obj = Unsafe.Read<object>(fieldAddress);

                if (obj == null)
                {
                    output.WriteNull();
                    return;
                }

                if (_stack >= MaxStack)
                    throw new CircularReferenceException("Circular references are not suported!");

                _stack++;

                var track = output.BeginSizeTrack();

                var pinnable = Unsafe.As<object, PinnableObject>(ref obj);

                fixed (byte* objectAddress = &pinnable.Pinnable)
                {
                    _fieldsSerializer.Write(objectAddress, output);
                }

                output.WriteSizeTrack(track);

                _stack--;
            }

            public override void ReadDelegate(void* fieldAddress, SerializerInput input)
            {
                ref var instance = ref Unsafe.AsRef<object>(fieldAddress);

                if (input.BeginReadSize(out var end))
                {
                    if (instance == null)
                    {
                        if (_haveCtor)
                        {
                            instance = Activator.CreateInstance(_type);
                        }
                        else
                            instance = FormatterServices.GetUninitializedObject(_type);
                    }

                    var pinnable = Unsafe.As<object, PinnableObject>(ref instance);
                    fixed (byte* objectAddress = &pinnable.Pinnable)
                    {
                        _fieldsSerializer.Read(objectAddress, input);
                    }

                    input.EndObject(end);
                }
                else
                {
                    instance = null;
                }
            }
        }

    }
}