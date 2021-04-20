using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace USerialization
{

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public abstract class CustomClassSerializer<T> : CustomDataSerializer where T : class
    {
        public override Type SerializedType => _type;

        private USerializer _serializer;

        private Type _type;

        protected USerializer Serializer => _serializer;

        private MemberSerializerStruct[] _members;

        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        protected CustomClassSerializer()
        {
            _type = typeof(T);

            var callSerializationEvents = typeof(ISerializationCallbackReceiver).IsAssignableFrom(_type);
            if (callSerializationEvents)
                Debug.LogError("This is not supported!");
        }

        public override bool TryInitialize(USerializer serializer)
        {
            _serializer = serializer;

            var adder = new ClassMemberAdder<T>(serializer);

            LocalInit(adder);

            _members = adder.GetMembers();

            if (serializer.DataTypesDatabase.TryGet(out ObjectDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }


        public abstract void LocalInit(ClassMemberAdder<T> adder);

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            if (Unsafe.Read<object>(fieldAddress) == null)
            {
                output.WriteNull();
                return;
            }

            var track = output.BeginSizeTrack();
            _members.WriteFields((byte*)fieldAddress, output);
            output.WriteSizeTrack(track);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            if (input.BeginReadSize(out var end))
            {
                ref var objectInstance = ref Unsafe.AsRef<object>(fieldAddress);
                if (objectInstance == null)
                    objectInstance = Activator.CreateInstance(_type);

                _members.ReadFields((byte*)fieldAddress, input, _serializer.DataTypesDatabase);

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

        private MemberSerializerStruct[] _members;

        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        protected CustomStructSerializer()
        {
            _type = typeof(T);

            var callSerializationEvents = typeof(ISerializationCallbackReceiver).IsAssignableFrom(_type);
            if (callSerializationEvents)
                Debug.LogError("This is not supported!");
        }

        public override bool TryInitialize(USerializer serializer)
        {
            _serializer = serializer;

            var adder = new StructMemberAdder<T>(serializer);
            LocalInit(adder);
            _members = adder.GetMembers();

            if (serializer.DataTypesDatabase.TryGet(out ObjectDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public abstract void LocalInit(StructMemberAdder<T> adder);

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var track = output.BeginSizeTrack();
            _members.WriteFields((byte*)fieldAddress, output);
            output.WriteSizeTrack(track);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            if (input.BeginReadSize(out var end))
            {
                _members.ReadFields((byte*)fieldAddress, input, _serializer.DataTypesDatabase);
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

