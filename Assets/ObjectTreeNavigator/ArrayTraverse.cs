using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Traverse
{
    public unsafe class ArrayTraverse : IObjectTraverse
    {
        private ObjectTreeNavigator _navigator;

        public void Initialize(ObjectTreeNavigator serializer)
        {
            _navigator = serializer;
        }

        public void Start(ObjectTreeNavigator serializer)
        {

        }

        public bool TryGetTraverseMethod(Type fieldType, out TraverseDelegate del)
        {
            if (fieldType.IsArray == false)
            {
                del = default;
                return false;
            }

            if (fieldType.GetArrayRank() > 1)
            {
                del = default;
                return false;
            }

            var elementType = fieldType.GetElementType();

            if (_navigator.TryGetSerializationMethods(elementType, out var traverseDelegate) == false)
            {
                del = default;
                return false;
            }

            if (elementType.IsValueType)
            {
                del = GetValueTypeWriter(elementType, traverseDelegate);
            }
            else
            {
                del = GetReferenceTypeWriter(traverseDelegate);
            }

            return true;
        }

        private static TraverseDelegate GetValueTypeWriter(Type elementType, TraverseDelegate serializeElement)
        {
            var size = UnsafeUtility.SizeOf(elementType);

            return delegate (void* fieldAddress, TraverseContext context)
            {
                var array = Unsafe.Read<Array>(fieldAddress);

                if (array != null)
                {
                    var count = array.Length;

                    var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                    for (var index = 0; index < count; index++)
                    {
                        serializeElement(address, context);
                        address += size;
                    }

                    UnsafeUtility.ReleaseGCObject(handle);
                }
                else
                {

                }
            };
        }

        private static TraverseDelegate GetReferenceTypeWriter(TraverseDelegate serializeElement)
        {
            return delegate (void* fieldAddress, TraverseContext context)
            {
                var array = Unsafe.Read<object[]>(fieldAddress);

                if (array != null)
                {
                    var count = array.Length;

                    var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                    for (var index = 0; index < count; index++)
                    {
                        var o = array[index];
                        if (o != null)
                        {
                            serializeElement(address, context);
                        }
                        else
                        {

                        }

                        address += sizeof(void*);
                    }

                    UnsafeUtility.ReleaseGCObject(handle);


                }
                else
                {

                }
            };
        }
    }
}