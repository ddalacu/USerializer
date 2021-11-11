using System;
using System.Reflection;

namespace USerialization
{
    public struct TypeFieldsIterator : IDisposable
    {
        private readonly BindingFlags _flags;

        private NonZeroIntPtrSet _set;

        private FieldInfo[] _array;

        private int _index;

        private Type _cType;

        public TypeFieldsIterator(Type cType, BindingFlags flags)
        {
            _flags = flags;
            _array = default;
            _index = -1;
            _cType = cType;
            _set = new NonZeroIntPtrSet(32);
        }

        public bool MoveNext(out FieldInfo info)
        {
            tryAgain:
            if (_array == null)
            {
                if (_cType == null ||
                    _cType == typeof(object) ||
                    _cType.IsInterface)
                {
                    _array = null;
                    info = default;
                    return false;
                }

                _array = _cType.GetFields(_flags);
                _index = -1;
                _cType = _cType.BaseType;
            }

            FieldInfo temp;

            do
            {
                _index++;

                if (_index == _array.Length)
                {
                    _array = null;
                    goto tryAgain;
                }

                temp = _array[_index];
            } while (_set.Insert(temp.FieldHandle.Value) == false);

            info = temp;
            return true;
        }

        public void Dispose()
        {
            if (_set.IsValid == false)
                return;

            _set.Dispose();
            _set = default;
        }
    }
}