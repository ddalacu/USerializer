using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

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
            FieldArrayWriter<T, TElement>.GetLengthDelegate getLength, FieldArrayWriter<T, TElement>.GetElementDelegate getElement,
            FieldArrayWriter<T, TElement>.SetLengthDelegate setLength, FieldArrayWriter<T, TElement>.SetElementDelegate setElement)
        {
            var memberType = typeof(TElement);
            if (_serializer.TryGetDataSerializer(memberType, out var elementSerializer) == false)
            {
                _serializer.Logger.Error($"{typeof(T)} custom serializer, cannot get serialization methods for element of type {memberType}");
                return false;
            }

            var toAdd = new MemberSerializerStruct(hash, new FieldArrayWriter<T, TElement>(elementSerializer, getLength, setLength, getElement, setElement, _serializer));
            Add(toAdd);
            return true;
        }
    }

    public class ClassMemberAdder<T> : MemberAdder<T> where T : class
    {
        public ClassMemberAdder(USerializer serializer) : base(serializer)
        {

        }

        public bool AddField<TMember>(int hash, SetPropertyDelegate<T, TMember> set, GetPropertyDelegate<T, TMember> get)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            if (get == null)
                throw new ArgumentNullException(nameof(get));

            var memberType = typeof(TMember);
            if (Serializer.TryGetDataSerializer(memberType, out var methods) == false)
            {
                _serializer.Logger.Error($"{typeof(T)} custom serializer, cannot get serialization methods for field of type {memberType}");
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

            if (Serializer.TryGetDataSerializer(memberType, out var methods) == false)
            {
                _serializer.Logger.Error($"{type} custom serializer, cannot get serialization methods for field of type {memberType}");
                return;
            }

            var property = type.GetProperty(name);

            if (property == null)
            {
                _serializer.Logger.Error($"{type} custom serializer, cannot get property {name} of type {memberType}");
                return;
            }

            var get = (GetPropertyDelegate<T, TMember>)Delegate.CreateDelegate(typeof(GetPropertyDelegate<T, TMember>), null, property.GetGetMethod());
            var set = (SetPropertyDelegate<T, TMember>)Delegate.CreateDelegate(typeof(SetPropertyDelegate<T, TMember>), null, property.GetSetMethod());

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

            if (Serializer.TryGetDataSerializer(field.FieldType, out var methods) == false)
            {
                _serializer.Logger.Error($"{type} custom serializer, cannot get serialization methods for field of type {field.FieldType}");
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

            var field = type.GetField(formattedFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                _serializer.Logger.Error($"{type} custom serializer, cannot get backing field for property {propertyName}");
                return false;
            }

            if (Serializer.TryGetDataSerializer(field.FieldType, out var methods) == false)
            {
                _serializer.Logger.Error($"{type} custom serializer, cannot get serialization methods for field of type {field.FieldType}");
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
            if (Serializer.TryGetDataSerializer(memberType, out var methods) == false)
            {
                _serializer.Logger.Error($"{typeof(T)} custom serializer, cannot get serialization methods for field of type {memberType}");
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

            if (Serializer.TryGetDataSerializer(field.FieldType, out var methods) == false)
            {
                _serializer.Logger.Error($"{type} custom serializer, cannot get serialization methods for field of type {field.FieldType}");
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

            var field = type.GetField(formattedFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                _serializer.Logger.Error($"{type} custom serializer, cannot get backing field for property {propertyName}");
                return false;
            }

            if (Serializer.TryGetDataSerializer(field.FieldType, out var methods) == false)
            {
                _serializer.Logger.Error($"{type} custom serializer, cannot get serialization methods for field of type {field.FieldType}");
                return false;
            }

            var toAdd = new MemberSerializerStruct(hash, new StructFieldWriter(methods, field));
            Add(toAdd);
            return true;
        }
    }


    public delegate void SetPropertyDelegate<in T, in TMember>(T obj, TMember value);

    public delegate TMember GetPropertyDelegate<in T, out TMember>(T obj);

    public sealed unsafe class StructFieldWriter : DataSerializer
    {
        private DataSerializer _dataSerializer;

        private int _fieldOffset;

        public override DataType GetDataType() => _dataSerializer.GetDataType();

        protected override void Initialize(USerializer serializer)
        {
            _dataSerializer.RootInitialize(serializer);
        }

        public StructFieldWriter(DataSerializer dataSerializer, FieldInfo fieldInfo)
        {
            _fieldOffset = UnsafeUtils.GetFieldOffset(fieldInfo);
            _dataSerializer = dataSerializer;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public override void Write(void* fieldAddress, SerializerOutput output)
        {
            var address = (byte*)fieldAddress + _fieldOffset;
            _dataSerializer.Write(address, output);
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public override void Read(void* fieldAddress, SerializerInput input)
        {
            var address = (byte*)fieldAddress + _fieldOffset;
            _dataSerializer.Read(address, input);
        }
    }

    public sealed unsafe class ClassFieldWriter : DataSerializer
    {
        private DataSerializer _dataSerializer;

        private int _fieldOffset;
        public override DataType GetDataType() => _dataSerializer.GetDataType();

        protected override void Initialize(USerializer serializer)
        {
            _dataSerializer.RootInitialize(serializer);
        }

        public ClassFieldWriter(DataSerializer dataSerializer, FieldInfo fieldInfo)
        {
            if (dataSerializer == null)
                throw new ArgumentNullException(nameof(dataSerializer));

            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            if (fieldInfo.DeclaringType.IsClass == false)
                throw new ArgumentException(nameof(fieldInfo));

            _fieldOffset = UnsafeUtils.GetFieldOffset(fieldInfo);
            _dataSerializer = dataSerializer;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public override void Write(void* fieldAddress, SerializerOutput output)
        {
            var obj = Unsafe.Read<object>(fieldAddress);//obj should not be null here

            var pinnable = Unsafe.As<object, PinnableObject>(ref obj);
            fixed (byte* objectAddress = &pinnable.Pinnable)
            {
                _dataSerializer.Write(objectAddress + _fieldOffset, output);
            }
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public override void Read(void* fieldAddress, SerializerInput input)
        {
            var obj = Unsafe.Read<object>(fieldAddress);//obj should not be null here

            var pinnable = Unsafe.As<object, PinnableObject>(ref obj);
            fixed (byte* objectAddress = &pinnable.Pinnable)
            {
                _dataSerializer.Read(objectAddress + _fieldOffset, input);
            }
        }
    }

    public sealed unsafe class PropertyWriter<T, TMember> : DataSerializer
    {
        private readonly SetPropertyDelegate<T, TMember> _set;
        private readonly GetPropertyDelegate<T, TMember> _get;

        private DataSerializer _dataSerializer;
        public override DataType GetDataType() => _dataSerializer.GetDataType();

        protected override void Initialize(USerializer serializer)
        {
            _dataSerializer.RootInitialize(serializer);
        }

        public PropertyWriter(DataSerializer dataSerializer, SetPropertyDelegate<T, TMember> set, GetPropertyDelegate<T, TMember> get)
        {
            _set = set;
            _get = get;
            _dataSerializer = dataSerializer;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public override void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = _get(Unsafe.Read<T>(fieldAddress));
            _dataSerializer.Write(Unsafe.AsPointer(ref value), output);
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public override void Read(void* fieldAddress, SerializerInput input)
        {
            TMember def = default;
            var ptr = Unsafe.AsPointer(ref def);
            _dataSerializer.Read(ptr, input);
            _set(Unsafe.Read<T>(fieldAddress), def);
        }
    }

    public sealed unsafe class ByRefFieldWriter<T, TMember> : DataSerializer
    {
        public delegate void SetDelegate(ref T obj, TMember value);

        public delegate TMember GetDelegate(ref T obj);
        private readonly SetDelegate _set;
        private readonly GetDelegate _get;
        private DataSerializer _dataSerializer;
        public override DataType GetDataType() => _dataSerializer.GetDataType();

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

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public override void Write(void* fieldAddress, SerializerOutput output)
        {
            ref var instance = ref Unsafe.AsRef<T>(fieldAddress);
            var value = _get(ref instance);
            _dataSerializer.Write(Unsafe.AsPointer(ref value), output);
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public override void Read(void* fieldAddress, SerializerInput input)
        {
            ref var instance = ref Unsafe.AsRef<T>(fieldAddress);

            TMember def = default;
            var ptr = Unsafe.AsPointer(ref def);
            _dataSerializer.Read(ptr, input);
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

        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        protected override void Initialize(USerializer serializer)
        {
            _elementSerializer.RootInitialize(serializer);
        }

        public FieldArrayWriter(DataSerializer elementSerializer,
            GetLengthDelegate getLengthDelegate,
            SetLengthDelegate setLengthDelegate,
            GetElementDelegate getElementDelegate,
            SetElementDelegate setElementDelegate, USerializer uSerializer)
        {
            _elementSerializer = elementSerializer;
            _getLengthDelegate = getLengthDelegate;
            _setLengthDelegate = setLengthDelegate;
            _getElementDelegate = getElementDelegate;
            _setElementDelegate = setElementDelegate;

            var typeLogic = uSerializer.DataTypesDatabase;

            if (typeLogic.TryGet(out ArrayDataTypeLogic arrayDataTypeLogic))
                _dataType = arrayDataTypeLogic.Value;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public override void Write(void* fieldAddress, SerializerOutput output)
        {
            ref var instance = ref Unsafe.AsRef<T>(fieldAddress);

            var count = _getLengthDelegate(ref instance);

            if (count == -1)
            {
                output.WriteNull();
                return;
            }

            var sizeTracker = output.BeginSizeTrack();
            {
                output.EnsureNext(6);
                output.WriteByteUnchecked((byte)_elementSerializer.GetDataType());
                output.Write7BitEncodedIntUnchecked(count);

                for (var index = 0; index < count; index++)
                {
                    var element = _getElementDelegate(ref instance, index);
                    var itemAddress = Unsafe.AsPointer(ref element);
                    _elementSerializer.Write(itemAddress, output);
                }
            }
            output.WriteSizeTrack(sizeTracker);
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public override void Read(void* fieldAddress, SerializerInput input)
        {
            ref var instance = ref Unsafe.AsRef<T>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var type = (DataType)input.ReadByte();

                var count = input.Read7BitEncodedInt();

                _setLengthDelegate(ref instance, count);

                if (type == _elementSerializer.GetDataType())
                {
                    for (var index = 0; index < count; index++)
                    {
                        TElement def = default;
                        var ptr = Unsafe.AsPointer(ref def);
                        _elementSerializer.Read(ptr, input);
                        _setElementDelegate(ref instance, index, ref def);
                    }
                }

                input.EndObject(end);
            }
            else
            {
                _setLengthDelegate(ref instance, -1);
            }
        }
    }
}