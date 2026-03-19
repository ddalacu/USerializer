using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace USerialization
{
    public abstract class CustomStructSerializer<T> : CustomDataSerializer where T : struct
    {
        private MemberSerializer _memberSerializer;

        public override DataType GetDataType() => DataType.Object;

        protected override void Initialize(USerializer serializer)
        {
            var adder = new StructMemberAdder<T>(serializer);
            LocalInit(adder);
            var members = adder.GetMembers();

            foreach (var member in members)
                member.DataSerializer.RootInitialize(serializer);

            _memberSerializer = new MemberSerializer(Unsafe.SizeOf<T>(), members, serializer.DataTypesDatabase);
        }

        public abstract void LocalInit(StructMemberAdder<T> adder);

        public override void Write(ReadOnlySpan<byte> span, SerializerOutput output, object context)
        {
            Debug.Assert(span.Length == Unsafe.SizeOf<T>());
            var track = output.BeginSizeTrack();
            _memberSerializer.Write(span, output, context);
            output.WriteSizeTrack(track);
        }

        public override void Read(Span<byte> span, SerializerInput input, object context)
        {
            Debug.Assert(span.Length == Unsafe.SizeOf<T>());

            if (input.BeginReadSize(out var end))
            {
                _memberSerializer.Read(span, input, context);

                input.EndObject(end);
            }
            else
            {
                ref var instance = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span));
                instance = default;
            }
        }
    }
}