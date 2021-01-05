using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Traverse
{
    public delegate object ChangeObjectDelegate(object fieldValue, Type fieldType);

    public delegate void SetDelegate<TMember, TOwner>(ref TOwner obj, TMember value);

    public delegate TMember GetDelegate<TMember, TOwner>(ref TOwner obj);

    public unsafe class ClassTraverse : IObjectTraverse
    {
        private ObjectTreeNavigator _navigator;

        private ChangeObjectDelegate _changeObject;

        private Dictionary<Type, List<TraverseDelegate>> _typeNavigators = new Dictionary<Type, List<TraverseDelegate>>();

        protected void AddField<TMember, TOwner>(SetDelegate<TMember, TOwner> set, GetDelegate<TMember, TOwner> get)
        {
            _navigator.TryGetSerializationMethods(typeof(TMember), out var methods);
            var eqComparer = EqualityComparer<TMember>.Default;

            void Navigate(void* fieldAddress, TraverseContext context)
            {
                ref var instance = ref Unsafe.AsRef<TOwner>(fieldAddress);

                var value = get(ref instance);
                var result = value;

                methods(Unsafe.AsPointer(ref result), context);

                if (eqComparer.Equals(result, value) == false)
                    set(ref instance, result);
            }

            if (_typeNavigators.TryGetValue(typeof(TOwner), out var list) == false)
            {
                list = new List<TraverseDelegate>();
                _typeNavigators.Add(typeof(TOwner), list);
            }

            list.Add(Navigate);
        }

        public ClassTraverse(ChangeObjectDelegate change)
        {
            _changeObject = change;
        }

        public void Initialize(ObjectTreeNavigator navigator)
        {
            _navigator = navigator;
        }

        public void Start(ObjectTreeNavigator navigator)
        {
            Init();
        }

        protected virtual void Init()
        {

        }

        public bool TryGetTraverseMethod(Type fieldType, out TraverseDelegate del)
        {
            if (fieldType.IsArray)
            {
                del = default;
                return false;
            }

            if (fieldType.IsValueType)
            {
                del = default;
                return false;
            }

            if (fieldType.IsPrimitive)
            {
                del = default;
                return false;
            }

            del = (fieldAddress, context) =>
            {
                ref var obj = ref Unsafe.AsRef<object>(fieldAddress);

                obj = _changeObject(obj, fieldType);

                if (obj == null)
                    return;

                if (context.AddTraversedObject(obj) == false)
                    return;

                var objType = obj.GetType();

                if (_typeNavigators.TryGetValue(objType, out var list))
                {
                    var count = list.Count;
                    for (var i = 0; i < count; i++)
                        list[i](fieldAddress, context);
                }
                else
                if (_navigator.GetTypeData(objType, out var fieldDatas))
                {
                    byte* objectAddress;
                    UnsafeUtility.CopyObjectAddressToPtr(obj, &objectAddress);

                    var fieldsCount = fieldDatas.Length;

                    for (var index = 0; index < fieldsCount; index++)
                    {
                        var fieldData = fieldDatas[index];
                        fieldData.TraverseDelegate(objectAddress + fieldData.Offset, context);
                    }
                }
                //else
                //{
                //    Debug.Log("Wtf");
                //}
            };

            return true;
        }
    }
}