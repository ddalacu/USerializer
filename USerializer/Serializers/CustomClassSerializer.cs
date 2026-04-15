using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace USerialization
{
    public abstract class CustomClassSerializer<T> : CustomDataSerializer where T : class
    {
        private readonly Type _type;
        private readonly Func<object> _activator;

        private MemberSerializer _memberSerializer;

        private DataType _dataType;

        public override DataType DataType => DataType.Object;

        protected CustomClassSerializer()
        {
            _type = typeof(T);
            _activator = ObjectActivator.GetActivator(typeof(T));
        }

        public abstract void LocalInit(ClassMemberAdder<T> adder);

        protected override void Initialize(USerializer serializer)
        {
            var adder = new ClassMemberAdder<T>(serializer);
            LocalInit(adder);
            var members = adder.GetMembers();

            foreach (var member in members)
                member.DataSerializer.RootInitialize(serializer);

            _memberSerializer = new MemberSerializer(IntPtr.Size, members, serializer.DataTypesDatabase);
        }

        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output, object context)
        {
            Debug.Assert(span.Length == IntPtr.Size);
            ref var obj = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span));

            if (obj == null)
            {
                output.WriteNull();
                return;
            }

            var track = output.BeginSizeTrack();

            _memberSerializer.Write(span, ref output, context);

            output.WriteSizeTrack(track);
        }

        public override void Read(Span<byte> span, ref SerializerInput input, object context)
        {
            Debug.Assert(span.Length == IntPtr.Size);

            ref var objectInstance = ref Unsafe.As<byte, Object>(ref MemoryMarshal.GetReference(span));

            if (input.NotNull())
            {
                if (objectInstance == null)
                    objectInstance = _activator();

                _memberSerializer.Read(span, ref input, context);
            }
            else
            {
                objectInstance = null;
            }
        }
    }
}