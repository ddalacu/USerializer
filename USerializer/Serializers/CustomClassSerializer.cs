using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public abstract class CustomClassSerializer<T> : CustomDataSerializer where T : class
    {
        public override Type SerializedType => _type;

        private Type _type;

        private MemberSerializer _memberSerializer;

        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        protected CustomClassSerializer()
        {
            _type = typeof(T);
        }

        public override bool TryInitialize(USerializer serializer)
        {
            if (serializer.DataTypesDatabase.TryGet(out ObjectDataTypeLogic objectDataTypeLogic))
            {
                _dataType = objectDataTypeLogic.Value;
                return true;
            }

            return false;
        }


        public abstract void LocalInit(ClassMemberAdder<T> adder);

        protected override void Initialize(USerializer serializer)
        {
            var adder = new ClassMemberAdder<T>(serializer);
            LocalInit(adder);
            var members = adder.GetMembers();
            _memberSerializer = new MemberSerializer(members, serializer.DataTypesDatabase);

            foreach (var member in _memberSerializer.Members)
                member.DataSerializer.RootInitialize(serializer);
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            if (Unsafe.Read<object>(fieldAddress) == null)
            {
                output.WriteNull();
                return;
            }

            var track = output.BeginSizeTrack();

            _memberSerializer.Write((byte*)fieldAddress, output);

            output.WriteSizeTrack(track);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            if (input.BeginReadSize(out var end))
            {
                ref var objectInstance = ref Unsafe.AsRef<object>(fieldAddress);
                if (objectInstance == null)
                    objectInstance = Activator.CreateInstance(_type);

                _memberSerializer.Read((byte*)fieldAddress, input);

                input.EndObject(end);
            }
            else
            {
                ref var objectInstance = ref Unsafe.AsRef<object>(fieldAddress);
                objectInstance = null;
            }

        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public abstract class CustomStructSerializer<T> : CustomDataSerializer where T : struct
    {
        public override Type SerializedType => _type;

        private USerializer _serializer;

        private Type _type;

        protected USerializer Serializer => _serializer;

        private MemberSerializer _memberSerializer;

        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        protected CustomStructSerializer()
        {
            _type = typeof(T);
        }

        public override bool TryInitialize(USerializer serializer)
        {
            _serializer = serializer;

            if (serializer.DataTypesDatabase.TryGet(out ObjectDataTypeLogic objectDataLogic))
            {
                _dataType = objectDataLogic.Value;
                var adder = new StructMemberAdder<T>(_serializer);
                LocalInit(adder);
                var members = adder.GetMembers();
                _memberSerializer = new MemberSerializer(members, _serializer.DataTypesDatabase);
                return true;
            }

            return false;
        }

        public abstract void LocalInit(StructMemberAdder<T> adder);

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var track = output.BeginSizeTrack();

            _memberSerializer.Write((byte*)fieldAddress, output);

            output.WriteSizeTrack(track);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            if (input.BeginReadSize(out var end))
            {
                _memberSerializer.Read((byte*)fieldAddress, input);

                input.EndObject(end);
            }
            else
            {
                ref var instance = ref Unsafe.AsRef<T>(fieldAddress);
                instance = default;
            }
        }
    }
}

