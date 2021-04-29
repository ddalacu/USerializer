using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

    public unsafe struct UnsafeArray<T> : IDisposable where T : unmanaged
    {
        public readonly int Length;

        private T* _ptr;

        public bool IsCreated => _ptr != null;

        public T this[int index]
        {
            get => _ptr[index];
            set => _ptr[index] = value;
        }

        public UnsafeArray(int length)
        {
            Length = length;
            var size = Unsafe.SizeOf<T>();
            _ptr = (T*)Marshal.AllocHGlobal(size * length);
        }

        public void Dispose()
        {
            if (_ptr != null)
            {
                Marshal.FreeHGlobal(new IntPtr(_ptr));
                _ptr = null;
            }
        }

        public static void Copy(UnsafeArray<T> source, int srcIndex, UnsafeArray<T> destination, int dstIndex, int length)
        {
            if (source.IsCreated == false)
                throw new ArgumentException($"{nameof(source)} is not created!");

            if (destination.IsCreated == false)
                throw new ArgumentException($"{nameof(destination)} is not created!");

            var sizeOf = Unsafe.SizeOf<T>();
            Unsafe.CopyBlock((void*)((IntPtr)destination._ptr + dstIndex * sizeOf), (void*)((IntPtr)source._ptr + srcIndex * sizeOf), (uint)(length * sizeOf));
        }

    }

    public struct TypeFieldsIterator : IDisposable
    {
        private UnsafeArray<FieldInfoReference> _references;

        private NonZeroIntPtrSet _set;

        public bool IsValid => _references.IsCreated;

        public FieldInfo this[int index] => _references[index].GetFieldInfo();


        private static Type _objectType = typeof(object);

        public TypeFieldsIterator(int capacity)
        {
            _references = new UnsafeArray<FieldInfoReference>(capacity);
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
                    _references = new UnsafeArray<FieldInfoReference>(length);
                }

                for (var index = 0; index < length; index++)
                    _references[index] = new FieldInfoReference(fields[index].FieldHandle, type.TypeHandle);

                return length;
            }

            if (_set.IsValid == false)
                _set = new NonZeroIntPtrSet(16);
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
                        var expanded = new UnsafeArray<FieldInfoReference>(_references.Length * 2);
                        UnsafeArray<FieldInfoReference>.Copy(_references, 0, expanded, 0, _references.Length);
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