//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace USerialization
//{
//    public class DictionarySerializer : ISerializationProvider
//    {
//        private USerializer _serializer;

//        public void Initialize(USerializer serializer)
//        {
//            _serializer = serializer;
//        }

//        public void Start(USerializer serializer)
//        {
            
//        }

//        public bool TryGetSerializationMethods(Type type, out SerializationMethods methods)
//        {
//            if (type.IsConstructedGenericType == false)
//            {
//                methods = default;
//                return false;
//            }

//            if (type.GetGenericTypeDefinition() != typeof(Dictionary<,>))
//            {
//                methods = default;
//                return false;
//            }

//            var arguments = type.GetGenericArguments();
//            var keyType = arguments[0];
//            var valueType = arguments[1];

//            if (_serializer.TryGetSerializationMethods(keyType, out var keySerializer) == false)
//            {
//                methods = default;
//                return false;
//            }

//            if (_serializer.TryGetSerializationMethods(valueType, out var valueSerializer) == false)
//            {
//                methods = default;
//                return false;
//            }

//            Dictionary<>

//            methods = default;
//            return true;
//        }
//    }
//}