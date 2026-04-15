using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace USerialization
{
    public class MemberAdder<T>
    {
        protected readonly USerializer _serializer;

        private List<MemberSerializerStruct> _members;

        public USerializer Serializer => _serializer;

        public MemberAdder(USerializer serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            _serializer = serializer;
            _members = new List<MemberSerializerStruct>(6);
        }

        public MemberSerializerStruct[] GetMembers() => _members.ToArray();

        public void Add(MemberSerializerStruct item)
        {
            var membersCount = _members.Count;

            if (membersCount >= 255)
                throw new Exception("To many fields!");

            for (var i = 0; i < membersCount; i++)
            {
                if (item.Hash == _members[i].Hash)
                    throw new Exception("Hash already added!");

                if (item.Hash < _members[i].Hash)
                {
                    _members.Insert(i, item);
                    return;
                }
            }

            _members.Add(item);
        }

        public bool AddArrayField<TElement>(int hash,
            FieldArrayWriter<T, TElement>.GetLengthDelegate getLength,
            FieldArrayWriter<T, TElement>.GetElementDelegate getElement,
            FieldArrayWriter<T, TElement>.SetLengthDelegate setLength,
            FieldArrayWriter<T, TElement>.SetElementDelegate setElement)
        {
            var memberType = typeof(TElement);
            if (_serializer.TryGetDataSerializer(memberType, out var elementSerializer, false) == false)
            {
                _serializer.Logger.Error(
                    $"{typeof(T)} custom serializer, cannot get serialization methods for element of type {memberType}");
                return false;
            }

            var toAdd = new MemberSerializerStruct(hash,
                new FieldArrayWriter<T, TElement>(elementSerializer, getLength, setLength, getElement, setElement));
            Add(toAdd);
            return true;
        }
    }

    public class ClassMemberAdder<T> : MemberAdder<T> where T : class
    {
        public ClassMemberAdder(USerializer serializer) : base(serializer)
        {
        }

        public bool AddField<TMember>(int hash, SetPropertyDelegate<T, TMember> set,
            GetPropertyDelegate<T, TMember> get)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            if (get == null)
                throw new ArgumentNullException(nameof(get));

            var memberType = typeof(TMember);
            if (Serializer.TryGetDataSerializer(memberType, out var methods, false) == false)
            {
                _serializer.Logger.Error(
                    $"{typeof(T)} custom serializer, cannot get serialization methods for field of type {memberType}");
                return false;
            }

            var toAdd = new MemberSerializerStruct(hash, new PropertyWriter<T, TMember>(methods, set, get));
            Add(toAdd);
            return true;
        }

        public void AddProperty<TMember>(int hash, string name)
        {
            var memberType = typeof(TMember);
            var type = typeof(T);

            if (Serializer.TryGetDataSerializer(memberType, out var methods, false) == false)
            {
                _serializer.Logger.Error(
                    $"{type} custom serializer, cannot get serialization methods for field of type {memberType}");
                return;
            }

            var property = type.GetProperty(name);

            if (property == null)
            {
                _serializer.Logger.Error($"{type} custom serializer, cannot get property {name} of type {memberType}");
                return;
            }

            var get = (GetPropertyDelegate<T, TMember>)Delegate.CreateDelegate(typeof(GetPropertyDelegate<T, TMember>),
                null, property.GetGetMethod());
            var set = (SetPropertyDelegate<T, TMember>)Delegate.CreateDelegate(typeof(SetPropertyDelegate<T, TMember>),
                null, property.GetSetMethod());

            var toAdd = new MemberSerializerStruct(hash, new PropertyWriter<T, TMember>(methods, set, get));

            Add(toAdd);
        }

        public bool AddField(int hash, string fieldName)
        {
            if (fieldName == null)
                throw new ArgumentNullException(nameof(fieldName));

            var type = typeof(T);

            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                _serializer.Logger.Error($"{type} custom serializer, cannot get field {fieldName}");
                return false;
            }

            if (Serializer.TryGetDataSerializer(field.FieldType, out var methods, false) == false)
            {
                _serializer.Logger.Error(
                    $"{type} custom serializer, cannot get serialization methods for field of type {field.FieldType}");
                return false;
            }

            var toAdd = new MemberSerializerStruct(hash, new ClassFieldWriter(methods, field));
            Add(toAdd);
            return true;
        }

        public bool AddBackingField(int hash, string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            var type = typeof(T);

            var formattedFieldName = $"<{propertyName}>k__BackingField";

            var field = type.GetField(formattedFieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                _serializer.Logger.Error(
                    $"{type} custom serializer, cannot get backing field for property {propertyName}");
                return false;
            }

            if (Serializer.TryGetDataSerializer(field.FieldType, out var methods, false) == false)
            {
                _serializer.Logger.Error(
                    $"{type} custom serializer, cannot get serialization methods for field of type {field.FieldType}");
                return false;
            }

            var toAdd = new MemberSerializerStruct(hash, new ClassFieldWriter(methods, field));
            Add(toAdd);
            return true;
        }
    }

    public class StructMemberAdder<T> : MemberAdder<T> where T : struct
    {
        public StructMemberAdder(USerializer serializer) : base(serializer)
        {
        }

        public bool AddField<TMember>(int hash, ByRefFieldWriter<T, TMember>.SetDelegate set,
            ByRefFieldWriter<T, TMember>.GetDelegate get)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            if (get == null)
                throw new ArgumentNullException(nameof(get));

            var memberType = typeof(TMember);
            if (Serializer.TryGetDataSerializer(memberType, out var methods, false) == false)
            {
                _serializer.Logger.Error(
                    $"{typeof(T)} custom serializer, cannot get serialization methods for field of type {memberType}");
                return false;
            }

            var toAdd = new MemberSerializerStruct(hash, new ByRefFieldWriter<T, TMember>(methods, set, get));
            Add(toAdd);
            return true;
        }

        public bool AddField(int hash, string fieldName)
        {
            if (fieldName == null)
                throw new ArgumentNullException(nameof(fieldName));

            var type = typeof(T);

            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                _serializer.Logger.Error($"{type} custom serializer, cannot get field {fieldName}");
                return false;
            }

            if (Serializer.TryGetDataSerializer(field.FieldType, out var methods, false) == false)
            {
                _serializer.Logger.Error(
                    $"{type} custom serializer, cannot get serialization methods for field of type {field.FieldType}");
                return false;
            }

            var toAdd = new MemberSerializerStruct(hash, new StructFieldWriter(methods, field));
            Add(toAdd);
            return true;
        }

        public bool AddBackingField(int hash, string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            var type = typeof(T);

            var formattedFieldName = $"<{propertyName}>k__BackingField";

            var field = type.GetField(formattedFieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                _serializer.Logger.Error(
                    $"{type} custom serializer, cannot get backing field for property {propertyName}");
                return false;
            }

            if (Serializer.TryGetDataSerializer(field.FieldType, out var methods, false) == false)
            {
                _serializer.Logger.Error(
                    $"{type} custom serializer, cannot get serialization methods for field of type {field.FieldType}");
                return false;
            }

            var toAdd = new MemberSerializerStruct(hash, new StructFieldWriter(methods, field));
            Add(toAdd);
            return true;
        }
    }

    public delegate void SetPropertyDelegate<in T, in TMember>(T obj, TMember value);

    public delegate TMember GetPropertyDelegate<in T, out TMember>(T obj);

    public sealed class StructFieldWriter : DataSerializer
    {
        private DataSerializer _dataSerializer;

        private int _fieldOffset;

        private int _stackSize;

        private FieldInfo _fieldInfo;

        public override DataType DataType => _dataSerializer.DataType;

        protected override void Initialize(USerializer serializer)
        {
            _dataSerializer.RootInitialize(serializer);
            _fieldOffset = serializer.RuntimeUtils.GetFieldOffset(_fieldInfo);
            _stackSize = serializer.RuntimeUtils.GetStackSize(_fieldInfo.FieldType);
        }

        public StructFieldWriter(DataSerializer dataSerializer, FieldInfo fieldInfo)
        {
            _fieldInfo = fieldInfo;
            _dataSerializer = dataSerializer;
        }
        
        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output)
        {
            _dataSerializer.Write(span.Slice(_fieldOffset, _stackSize), ref output);
        }
        
        public override void Read(Span<byte> span, ref SerializerInput input)
        {
            _dataSerializer.Read(span.Slice(_fieldOffset, _stackSize), ref input);
        }
    }

    public sealed unsafe class ClassFieldWriter : DataSerializer
    {
        private DataSerializer _dataSerializer;

        private int _fieldOffset;

        private int _stackSize;

        private FieldInfo _fieldInfo;

        public override DataType DataType => _dataSerializer.DataType;

        protected override void Initialize(USerializer serializer)
        {
            _dataSerializer.RootInitialize(serializer);
            _fieldOffset = serializer.RuntimeUtils.GetFieldOffset(_fieldInfo);
            _stackSize = serializer.RuntimeUtils.GetStackSize(_fieldInfo.FieldType);
        }

        public ClassFieldWriter(DataSerializer dataSerializer, FieldInfo fieldInfo)
        {
            if (dataSerializer == null)
                throw new ArgumentNullException(nameof(dataSerializer));

            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            if (fieldInfo.DeclaringType.IsClass == false)
                throw new ArgumentException(nameof(fieldInfo));

            _fieldInfo = fieldInfo;
            _dataSerializer = dataSerializer;
        }
        
        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output)
        {
            Debug.Assert(span.Length == IntPtr.Size);
            
            ref var obj = ref Unsafe.As<byte, PinnableObject>(ref MemoryMarshal.GetReference(span));

            fixed (byte* objectAddress = &obj.Pinnable)
            {
                _dataSerializer.Write(new Span<byte>(objectAddress + _fieldOffset, _stackSize), ref output);
            }
        }
        
        public override void Read(Span<byte> span, ref SerializerInput input)
        {
            Debug.Assert(span.Length == IntPtr.Size);
            
            ref var obj = ref Unsafe.As<byte, PinnableObject>(ref MemoryMarshal.GetReference(span));

            fixed (byte* objectAddress = &obj.Pinnable)
            {
                _dataSerializer.Read(new Span<byte>(objectAddress + _fieldOffset, _stackSize), ref input);
            }
        }
    }

    public sealed unsafe class PropertyWriter<T, TMember> : DataSerializer
    {
        private readonly SetPropertyDelegate<T, TMember> _set;
        private readonly GetPropertyDelegate<T, TMember> _get;

        private DataSerializer _dataSerializer;
        public override DataType DataType => _dataSerializer.DataType;

        protected override void Initialize(USerializer serializer)
        {
            _dataSerializer.RootInitialize(serializer);
        }

        public PropertyWriter(DataSerializer dataSerializer, SetPropertyDelegate<T, TMember> set,
            GetPropertyDelegate<T, TMember> get)
        {
            _set = set;
            _get = get;
            _dataSerializer = dataSerializer;
        }
        
        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output)
        {
            Debug.Assert(span.Length == IntPtr.Size);
            
            ref var instance = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span));
            var value = _get(instance);
            var ptr = Unsafe.AsPointer(ref value);
            _dataSerializer.Write(new Span<byte>(ptr, Unsafe.SizeOf<TMember>()), ref output);
        }
        
        public override void Read(Span<byte> span, ref SerializerInput input)
        {
            Debug.Assert(span.Length == IntPtr.Size);
            
            TMember def = default;
            var ptr = Unsafe.AsPointer(ref def);
            _dataSerializer.Read(new Span<byte>(ptr, Unsafe.SizeOf<TMember>()), ref input);
            ref var instance = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span));
            _set(instance, def);
        }
    }

    public sealed unsafe class ByRefFieldWriter<T, TMember> : DataSerializer
    {
        public delegate void SetDelegate(ref T obj, TMember value);

        public delegate TMember GetDelegate(ref T obj);

        private readonly SetDelegate _set;
        private readonly GetDelegate _get;
        private DataSerializer _dataSerializer;
        public override DataType DataType => _dataSerializer.DataType;

        protected override void Initialize(USerializer serializer)
        {
            _dataSerializer.RootInitialize(serializer);
        }

        public ByRefFieldWriter(DataSerializer dataSerializer, SetDelegate set, GetDelegate get)
        {
            _set = set;
            _get = get;
            _dataSerializer = dataSerializer;
        }
        
        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output)
        {
            Debug.Assert(span.Length == Unsafe.SizeOf<T>());
            
            ref var instance = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span));

            var value = _get(ref instance);
            var ptr = Unsafe.AsPointer(ref value);
            _dataSerializer.Write(new Span<byte>(ptr, Unsafe.SizeOf<TMember>()), ref output);
        }
        
        public override void Read(Span<byte> span, ref SerializerInput input)
        {
            Debug.Assert(span.Length == Unsafe.SizeOf<T>());
            
            ref var instance = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span));

            TMember def = default;
            var ptr = Unsafe.AsPointer(ref def);
            _dataSerializer.Read(new Span<byte>(ptr, Unsafe.SizeOf<TMember>()), ref input);
            _set(ref instance, def);
        }
    }

    public sealed unsafe class FieldArrayWriter<T, TElement> : DataSerializer
    {
        private readonly DataSerializer _elementSerializer;
        private readonly GetLengthDelegate _getLengthDelegate;
        private readonly SetLengthDelegate _setLengthDelegate;
        private readonly GetElementDelegate _getElementDelegate;
        private readonly SetElementDelegate _setElementDelegate;

        public delegate TElement GetElementDelegate(ref T obj, int index);

        public delegate void SetElementDelegate(ref T obj, int index, ref TElement element);

        public delegate int GetLengthDelegate(ref T obj);

        public delegate void SetLengthDelegate(ref T obj, int length);

        public override DataType DataType => DataType.Array;

        protected override void Initialize(USerializer serializer)
        {
            _elementSerializer.RootInitialize(serializer);
        }

        public FieldArrayWriter(DataSerializer elementSerializer,
            GetLengthDelegate getLengthDelegate,
            SetLengthDelegate setLengthDelegate,
            GetElementDelegate getElementDelegate,
            SetElementDelegate setElementDelegate)
        {
            _elementSerializer = elementSerializer;
            _getLengthDelegate = getLengthDelegate;
            _setLengthDelegate = setLengthDelegate;
            _getElementDelegate = getElementDelegate;
            _setElementDelegate = setElementDelegate;
        }
        
        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output)
        {
            ref var instance = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span));

            var count = _getLengthDelegate(ref instance);

            if (count == -1)
            {
                output.WriteNull();
                return;
            }

            var sizeTracker = output.BeginSizeTrack();
            {
                output.EnsureNext(6);
                output.WriteByteUnchecked((byte)_elementSerializer.DataType);
                output.Write7BitEncodedIntUnchecked(count);

                for (var index = 0; index < count; index++)
                {
                    var element = _getElementDelegate(ref instance, index);
                    var itemAddress = Unsafe.AsPointer(ref element);
                    _elementSerializer.Write(new Span<byte>(itemAddress, Unsafe.SizeOf<TElement>()), ref output);
                }
            }
            output.WriteSizeTrack(sizeTracker);
        }
        
        public override void Read(Span<byte> span, ref SerializerInput input)
        {
            ref var instance = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span));

            if (input.BeginReadSize(out var end))
            {
                var type = (DataType)input.ReadByte();

                var count = input.Read7BitEncodedInt();

                _setLengthDelegate(ref instance, count);

                if (type == _elementSerializer.DataType)
                {
                    for (var index = 0; index < count; index++)
                    {
                        TElement def = default;
                        var ptr = Unsafe.AsPointer(ref def);
                        _elementSerializer.Read(new Span<byte>(ptr, Unsafe.SizeOf<TElement>()), ref input);
                        _setElementDelegate(ref instance, index, ref def);
                    }
                }
                else
                {
                    input.EndObject(end);
                }
            }
            else
            {
                _setLengthDelegate(ref instance, -1);
            }
        }
    }
}