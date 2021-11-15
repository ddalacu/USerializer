using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public abstract class CustomStructSerializer<T> : CustomDataSerializer where T : struct
    {
        private MemberSerializer _memberSerializer;

        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            if (serializer.DataTypesDatabase.TryGet(out ObjectDataTypeLogic objectDataTypeLogic))
            {
                _dataType = objectDataTypeLogic.Value;
                return true;
            }

            return false;
        }

        protected override void Initialize(USerializer serializer)
        {
            var adder = new StructMemberAdder<T>(serializer);
            LocalInit(adder);
            var members = adder.GetMembers();

            foreach (var member in members)
                member.DataSerializer.RootInitialize(serializer);

            _memberSerializer = new MemberSerializer(members, serializer.DataTypesDatabase);
        }

        public abstract void LocalInit(StructMemberAdder<T> adder);

        public override unsafe void Write(void* fieldAddress, SerializerOutput output, object context)
        {
            var track = output.BeginSizeTrack();

            _memberSerializer.Write((byte*) fieldAddress, output, context);

            output.WriteSizeTrack(track);
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input, object context)
        {
            if (input.BeginReadSize(out var end))
            {
                _memberSerializer.Read((byte*) fieldAddress, input, context);

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