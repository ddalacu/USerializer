using System;

namespace Traverse
{
    public unsafe class StructTraverse : IObjectTraverse
    {
        private ObjectTreeNavigator _navigator;

        public void Initialize(ObjectTreeNavigator navigator)
        {
            _navigator = navigator;
        }

        public void Start(ObjectTreeNavigator serializer)
        {

        }

        public bool TryGetTraverseMethod(Type fieldType, out TraverseDelegate del)
        {
            if (fieldType.IsArray)
            {
                del = default;
                return false;
            }

            if (fieldType.IsValueType == false)
            {
                del = default;
                return false;
            }

            if (fieldType.IsPrimitive)
            {
                del = default;
                return false;
            }

            if (_navigator.GetTypeData(fieldType, out var typeData) == false)
            {
                del = default;
                return false;
            }

            del = GetWriter(typeData);
            return true;
        }

        private static TraverseDelegate GetWriter(FieldTraverseData[] fields)
        {
            return delegate (void* fieldAddress, TraverseContext context)
            {
                byte* address = (byte*)fieldAddress;
                var fieldsCount = fields.Length;

                for (var index = 0; index < fieldsCount; index++)
                {
                    var fieldData = fields[index];
                    fieldData.TraverseDelegate(address + fieldData.Offset, context);
                }
            };
        }
    }
}