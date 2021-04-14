using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Unity.Collections;
using UnityEditor;

namespace USerialization
{
    public readonly struct FieldInfoReference
    {
        private readonly RuntimeFieldHandle FieldHandle;
        private readonly RuntimeTypeHandle TypeHandle;

        public FieldInfoReference(RuntimeFieldHandle fieldHandle, RuntimeTypeHandle typeHandle)
        {
            FieldHandle = fieldHandle;
            TypeHandle = typeHandle;
        }

        public FieldInfo GetFieldInfo()
        {
            return FieldInfo.GetFieldFromHandle(FieldHandle, TypeHandle);
        }
    }

    public struct TypeFieldsIterator : IDisposable
    {
        private readonly Allocator _allocator;

        private NativeArray<FieldInfoReference> _references;

        private NonZeroIntPtrSet _set;

        public bool IsValid => _references.IsCreated;

        public FieldInfo this[int index] => _references[index].GetFieldInfo();

        private static Type _objectType = typeof(object);

        public TypeFieldsIterator(Allocator allocator)
        {
            _allocator = allocator;
            _references = new NativeArray<FieldInfoReference>(8, _allocator);
            _set = default;
        }

        public int Fill(Type type, BindingFlags bindingFlags)
        {
            if (type == _objectType)
                return 0;

            if (type.IsInterface)
                return 0;

            if (type.BaseType == _objectType)
            {
                var fields = type.GetFields(bindingFlags);
                var length = fields.Length;

                if (length > _references.Length)
                {
                    _references.Dispose();
                    _references = new NativeArray<FieldInfoReference>(length, _allocator);
                }

                for (var index = 0; index < length; index++)
                    _references[index] = new FieldInfoReference(fields[index].FieldHandle, type.TypeHandle);

                return length;
            }

            if (_set.IsValid == false)
                _set = new NonZeroIntPtrSet(16, _allocator);
            else
                _set.Clear();

            var currentType = type;

            int size = 0;

            while (currentType != _objectType)
            {
                var fieldInfos = currentType.GetFields(bindingFlags);
                var declaringTypeHandle = currentType.TypeHandle;

                var fieldInfosLength = fieldInfos.Length;

                for (var index = 0; index < fieldInfosLength; index++)
                {
                    var fieldInfo = fieldInfos[index];
                    if (_set.Insert(fieldInfo.FieldHandle.Value) == false)
                        continue;

                    if (size == _references.Length)
                    {
                        var expanded = new NativeArray<FieldInfoReference>(_references.Length * 2, _allocator);
                        NativeArray<FieldInfoReference>.Copy(_references, expanded, _references.Length);
                        _references.Dispose();
                        _references = expanded;
                    }

                    _references[size] = new FieldInfoReference(fieldInfo.FieldHandle, declaringTypeHandle);
                    size++;
                }

                currentType = currentType.BaseType;
            }

            return size;
        }

        public void Dispose()
        {
            if (_set.IsValid)
                _set.Dispose();

            _references.Dispose();
        }
    }
}