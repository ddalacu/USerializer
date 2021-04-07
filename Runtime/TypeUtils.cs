using System;
using System.Collections.Generic;
using System.Reflection;

namespace USerialization
{
    public static class TypeUtils
    {
        public static ICollection<FieldInfo> GetAllFields(Type type, BindingFlags bindingFlags)
        {
            var fieldInfos = type.GetFields(bindingFlags);

            var objectType = typeof(object);

            if (type.BaseType == objectType)
                return fieldInfos;

            var currentType = type;
            var fieldInfoList = new HashSet<FieldInfo>(fieldInfos, FieldInfoComparer.DefaultInstance);

            while (currentType != objectType)
            {
                fieldInfos = currentType.GetFields(bindingFlags);
                fieldInfoList.UnionWith(fieldInfos);
                currentType = currentType.BaseType;
            }
            return fieldInfoList;
        }
    }
}