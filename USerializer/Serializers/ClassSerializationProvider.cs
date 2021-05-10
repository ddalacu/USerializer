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

    public class ClassSerializationProvider : ISerializationProvider
    {
        public bool TryGet(USerializer serializer, Type type, out DataSerializer serializationMethods)
        {
            serializationMethods = default;

            if (type.IsArray)
                return false;

            if (type.IsValueType)
                return false;

            if (type.IsPrimitive)
                return false;

            if (serializer.DataTypesDatabase.TryGet(out ObjectDataTypeLogic objectDataTypeLogic) == false)
                return false;

            if (serializer.SerializationPolicy.ShouldSerialize(type) == false)
                return false;

            serializationMethods = new ClassDataSerializer(type, objectDataTypeLogic.Value);

            return true;
        }

    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed unsafe class ClassDataSerializer : DataSerializer
    {
        private readonly Type _type;

        private FieldsSerializer _fieldsSerializer;

        private readonly bool _haveCtor;

        private readonly DataType _dataType;

        public override DataType GetDataType() => _dataType;

        protected override void Initialize(USerializer serializer)
        {
            var fields = FieldData.GetFields(_type, serializer);

            _fieldsSerializer = new FieldsSerializer(fields, serializer.DataTypesDatabase);

            foreach (var fieldData in fields)
                fieldData.SerializationMethods.RootInitialize(serializer);
        }

        public ClassDataSerializer(Type type, DataType objectDataType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsValueType)
                throw new ArgumentException(nameof(type));

            _type = type;
            var constructor = _type.GetConstructor(Type.EmptyTypes);
            _haveCtor = constructor != null;
            _dataType = objectDataType;
        }

        private int _stack;

        private const int MaxStack = 32;

        public override void Write(void* fieldAddress, SerializerOutput output)
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

        public override void Read(void* fieldAddress, SerializerInput input)
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