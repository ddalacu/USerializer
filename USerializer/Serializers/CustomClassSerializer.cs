using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public abstract class CustomClassSerializer<T> : CustomDataSerializer where T : class
    {
        private readonly Type _type;

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

            foreach (var member in members)
                member.DataSerializer.RootInitialize(serializer);

            _memberSerializer = new MemberSerializer(members, serializer.DataTypesDatabase);
        }

        public override unsafe void Write(ReadOnlySpan<byte> span, SerializerOutput output, object context)
        {
            ref var obj = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span));

            if (obj == null)
            {
                output.WriteNull();
                return;
            }

            var track = output.BeginSizeTrack();

            _memberSerializer.Write(span, output, context);

            output.WriteSizeTrack(track);
        }

        public override unsafe void Read(Span<byte> span, SerializerInput input, object context)
        {
            ref var objectInstance = ref Unsafe.As<byte, Object>(ref MemoryMarshal.GetReference(span));

            if (input.BeginReadSize(out var end))
            {
                if (objectInstance == null)
                    objectInstance = Activator.CreateInstance(_type);

                _memberSerializer.Read(span, input, context);

                input.EndObject(end);
            }
            else
            {
                objectInstance = null;
            }
        }
    }
}