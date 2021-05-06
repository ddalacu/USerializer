using System;
using System.Reflection;
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

        private TypeDataCache _typeDataCache;

        public void Initialize(USerializer serializer)
        {
            _serializer = serializer;
            _typeDataCache = new TypeDataCache(512);
        }

        public void Start(USerializer serializer)
        {

        }

        public bool TryGet(Type type, out DataSerializer serializationMethods)
        {
            if (type.IsArray)
            {
                serializationMethods = default;
                return false;
            }

            if (type.IsValueType)
            {
                serializationMethods = default;
                return false;
            }

            if (type.IsPrimitive)
            {
                serializationMethods = default;
                return false;
            }

            if (_typeDataCache.GetTypeData(type, _serializer, out var typeData) == false)
            {
                serializationMethods = default;
                return false;
            }

            serializationMethods = GetSerializationMethods(type, typeData);
            return true;
        }

        public DataSerializer GetSerializationMethods(Type type, FieldsData typeData)
        {
            return new ClassDataSerializer(type, typeData, _serializer);
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public sealed class ClassDataSerializer : DataSerializer
        {
            private readonly Type _type;
            private readonly FieldsData _typeData;
            private readonly bool _haveCtor;
            private DataTypesDatabase _dataTypesDatabase;

            private DataType _dataType;

            public override DataType GetDataType() => _dataType;

            public ClassDataSerializer(Type type, FieldsData typeData, USerializer serializer)
            {
                if (type.IsValueType)
                    throw new ArgumentException(nameof(type));

                _type = type;
                _typeData = typeData;

                var ctor = _type.GetConstructor(Type.EmptyTypes);
                _haveCtor = ctor != null;

                _dataTypesDatabase = serializer.DataTypesDatabase;

                if (_dataTypesDatabase.TryGet(out ObjectDataTypeLogic arrayDataTypeLogic))
                    _dataType = arrayDataTypeLogic.Value;
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
                    _typeData.Fields.WriteFields(objectAddress, output);
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
                            instance = Activator.CreateInstance(_type);
                        else
                            instance = FormatterServices.GetUninitializedObject(_type);
                    }

                    var pinnable = Unsafe.As<object, PinnableObject>(ref instance);
                    fixed (byte* objectAddress = &pinnable.Pinnable)
                    {
                        _typeData.Fields.ReadFields(objectAddress, input, _dataTypesDatabase);
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