//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using UnityEngine;
//using Object = System.Object;

//namespace USerialization
//{
//    public class UnitySerializationPolicy : ISerializationPolicy
//    {
//        private readonly Type _serializeFieldAttributeType = typeof(SerializeField);

//        private readonly Type _nonSerializedAttributeType = typeof(NonSerializedAttribute);

//        private readonly Type _unityObjectType = typeof(UnityEngine.Object);

//        private readonly Type _serializableAttributeType = typeof(SerializableAttribute);

//        public Func<FieldInfo, bool> ShouldSerializeField;

//        private HashSet<Type> _unitySerializedTypes = new HashSet<Type>()
//        {
//            typeof(AnimationCurve),
//            typeof(Vector2),
//            typeof(Vector3),
//            typeof(Vector4),
//            typeof(Quaternion),
//            typeof(Color),
//            typeof(Color32),
//        };

//        public bool ShouldSerialize(Type type)
//        {
//            if (type.IsAbstract)
//                return false;

//            if (type.IsGenericType)// Type<int>
//                return false;

//            if (type.IsGenericTypeDefinition)// Type<>
//                return false;

//            if (type.IsValueType)
//                return true;

//            if (type.IsClass)
//            {
//                if (_unityObjectType.IsAssignableFrom(type))
//                    return true;

//                if (type.GetCustomAttribute(_serializableAttributeType) != null)
//                    return true;

//                if (_unitySerializedTypes.Contains(type))
//                    return true;
//            }

//            return false;
//        }

//        public bool ShouldSerialize(FieldInfo fieldInfo)
//        {
//            if (fieldInfo.IsPrivate)
//            {
//                if (Attribute.IsDefined(fieldInfo, _serializeFieldAttributeType) == false)
//                    return false;
//            }
//            else
//            {
//                if (Attribute.IsDefined(fieldInfo, _nonSerializedAttributeType))
//                    return false;
//            }

//            if (ShouldSerializeField != null &&
//                ShouldSerializeField(fieldInfo) == false)
//                return false;

//            return true;
//        }
//    }
//}