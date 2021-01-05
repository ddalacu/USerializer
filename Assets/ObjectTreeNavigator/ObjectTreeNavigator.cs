using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Traverse
{
    public class FieldInfoComparer : IEqualityComparer<FieldInfo>
    {
        public bool Equals(FieldInfo x, FieldInfo y)
        {
            return x.DeclaringType == y.DeclaringType && x.Name == y.Name;
        }

        public int GetHashCode(FieldInfo obj)
        {
            return obj.Name.GetHashCode() ^ obj.DeclaringType.GetHashCode();
        }
    }

    public class TraverseContext
    {
        private HashSet<object> _traversedObjects = new HashSet<object>();

        public bool AddTraversedObject(object o)
        {
            return _traversedObjects.Add(o);
        }
    }

    public unsafe delegate void TraverseDelegate(void* fieldAddress, TraverseContext context);

    [StructLayout(LayoutKind.Auto)]
    public readonly struct FieldTraverseData
    {
        public readonly TraverseDelegate TraverseDelegate;
        public readonly ushort Offset;

        public FieldTraverseData(TraverseDelegate traverseDelegate, ushort offset)
        {
            TraverseDelegate = traverseDelegate;
            Offset = offset;
        }
    }

    public class ObjectTreeNavigator
    {
        private Dictionary<Type, TraverseDelegate> _methods = new Dictionary<Type, TraverseDelegate>(1024);

        private readonly Dictionary<Type, FieldTraverseData[]> _datas = new Dictionary<Type, FieldTraverseData[]>(1024);

        private readonly IObjectTraverse[] _providers;

        private readonly ITraversePolicy _traversePolicy;

        public ObjectTreeNavigator(IObjectTraverse[] providers, ITraversePolicy traversePolicy)
        {
            _traversePolicy = traversePolicy;
            _providers = providers;

            foreach (var serializationProvider in _providers)
                serializationProvider.Initialize(this);

            foreach (var serializationProvider in _providers)
                serializationProvider.Start(this);
        }

        public static ICollection<FieldInfo> GetAllFields(Type type, BindingFlags bindingFlags)
        {
            FieldInfo[] fieldInfos = type.GetFields(bindingFlags);

            // If this class doesn't have a base, don't waste any time
            if (type.BaseType == typeof(object))
            {
                return fieldInfos;
            }

            var currentType = type;
            var fieldComparer = new FieldInfoComparer();
            var fieldInfoList = new HashSet<FieldInfo>(fieldInfos, fieldComparer);

            while (currentType != typeof(object))
            {
                fieldInfos = currentType.GetFields(bindingFlags);
                fieldInfoList.UnionWith(fieldInfos);
                currentType = currentType.BaseType;
            }

            return fieldInfoList;
        }

        public List<FieldTraverseData> GetFields(Type type) //todo use a pooled list for field data
        {
            var allFields = GetAllFields(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var length = allFields.Count;

            var fields = new List<FieldTraverseData>(length);

            foreach (var fieldInfo in allFields)
            {
                if (_traversePolicy.ShouldTraverse(fieldInfo) == false)
                    continue;

                if (TryGetSerializationMethods(fieldInfo.FieldType, out var serializationMethods))
                {
                    var fieldOffset = UnsafeUtility.GetFieldOffset(fieldInfo);
                    fields.Add(new FieldTraverseData(serializationMethods, (ushort)fieldOffset));
                }
            }

            return fields;
        }

        public bool GetTypeData(Type type, out FieldTraverseData[] result)
        {
            if (_datas.TryGetValue(type, out result))
                return result != null;

            if (_traversePolicy.ShouldTraverse(type))
            {
                _datas.Add(type, Array.Empty<FieldTraverseData>()); //to prevent recursion when GetFields
                result = GetFields(type).ToArray();
                _datas[type] = result;

                return true;
            }

            _datas.Add(type, default);

            return false;
        }

        public bool TryGetSerializationMethods(Type type, out TraverseDelegate method)
        {
            if (_methods.TryGetValue(type, out method))
                return method != null;

            foreach (var provider in _providers)
            {
                if (provider.TryGetTraverseMethod(type, out method) == false)
                    continue;

                _methods.Add(type, method);
                return true;
            }

            method = default;
            _methods.Add(type, method);
            return false;
        }

        public unsafe bool Traverse<T>(ref T o, TraverseContext context)
        {
            var type = o.GetType();

            if (TryGetSerializationMethods(type, out var serializationMethods))
            {
                serializationMethods(Unsafe.AsPointer(ref o), context);
            }

            return true;
        }

    }
}