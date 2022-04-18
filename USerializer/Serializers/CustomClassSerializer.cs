using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public abstract class CustomClassSerializer<T> : CustomDataSerializer where T : class
    {
        private TypeInstantiator _instantiator;

        private MemberSerializer _memberSerializer;

        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        protected CustomClassSerializer()
        {
            _instantiator = new TypeInstantiator(typeof(T));//cache typeof to improve il2cpp perf
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

            foreach (var member in members)
                member.DataSerializer.RootInitialize(serializer);

            _memberSerializer = new MemberSerializer(members, serializer.DataTypesDatabase);
        }

        public override unsafe void Write(void* fieldAddress, SerializerOutput output, object context)
        {
            if (Unsafe.Read<object>(fieldAddress) == null)
            {
                output.WriteNull();
                return;
            }

            var track = output.BeginSizeTrack();

            _memberSerializer.Write((byte*)fieldAddress, output, context);

            output.WriteSizeTrack(track);
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input, object context)
        {
            if (input.BeginReadSize(out var end))
            {
                ref var objectInstance = ref Unsafe.AsRef<object>(fieldAddress);
                if (objectInstance == null)
                    objectInstance = _instantiator.CreateInstance();

                _memberSerializer.Read((byte*)fieldAddress, input, context);

                input.EndObject(end);
            }
            else
            {
                ref var objectInstance = ref Unsafe.AsRef<object>(fieldAddress);
                objectInstance = null;
            }

        }
    }
}

