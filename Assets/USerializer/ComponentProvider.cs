//using System;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using Unity.Collections.LowLevel.Unsafe;
//using UnityEngine;
//using USerialization;
//using Object = UnityEngine.Object;


//[assembly: CustomSerializer(typeof(TransformSerializer))]

//namespace USerialization
//{
//    public class TransformSerializer : ICustomSerializer
//    {
//        public Type SerializedType => typeof(Transform);
//        public USerializer Serializer { get; set; }

//        private bool _root = true;

//        public UnityObjectSerializer ObjectSerializer;

//        public unsafe void Write(void* fieldAddress, SerializerOutput output)
//        {
//            var transform = Unsafe.Read<Transform>(fieldAddress);
//            if (transform == null)
//            {
//                output.Null();
//                return;
//            }

//            if (_root == false)
//            {
//                var identifier = ObjectSerializer.GetUniqueId(transform);
//                output.WriteInt(identifier);
//                return;
//            }

//            output.OpenArray();
//            {
//                output.WriteInt(ObjectSerializer.GetUniqueId(transform.gameObject));

//                var parent = transform.parent;

//                var parentId = 0;

//                if (parent != null)
//                {
//                    parentId = ObjectSerializer.GetUniqueId(parent);
//                }

//                output.WriteInt(parentId);

//                _root = false;

//                var childCount = transform.childCount;

//                for (var i = 0; i < childCount; i++)
//                {
//                    var child = transform.GetChild(i);
//                    ObjectSerializer.AddDependency(child.gameObject);
//                }

//                _root = true;
//            }
//            output.CloseArray();
//        }

//        public unsafe void Read(void* fieldAddress, SerializerInput input)
//        {
//            ref var transform = ref Unsafe.AsRef<Transform>(fieldAddress);
//            if (input.CurrentNode.IsNull())
//            {
//                transform = null;
//                return;
//            }

//            Debug.Assert(input.CurrentNode.IsArray());

//            if (input.BeginReadArray(out var count))
//            {
//                var gameObjectId = input.ReadInt();

//                var gameObject = (GameObject)ObjectSerializer.GetInstance(Serializer, input, gameObjectId);
//                transform = gameObject.transform;

//                var parentId = input.ReadInt();

//                if (parentId != 0)
//                {
//                    var parent = (Transform)ObjectSerializer.GetInstance(Serializer, input, parentId);
//                    transform.SetParent(parent);
//                }

//                input.EndArray();
//            }
//        }
//    }


//    public sealed unsafe class ComponentProvider : ISerializationProvider
//    {
//        private Type _objectType;

//        public UnityObjectSerializer ObjectSerializer;

//        private USerializer _uSerializer;

//        public void Initialize(USerializer serializer)
//        {
//            _uSerializer = serializer;
//            _objectType = typeof(Component);
//        }

//        private void WriteFields(Object obj, TypeData typeData, SerializerOutput output)
//        {
//            output.OpenObject();
//            var serializers = typeData.Fields;
//            var fieldsLength = serializers.Length;

//            byte* objectAddress;
//            UnsafeUtility.CopyObjectAddressToPtr(obj, &objectAddress);

//            for (var index = 0; index < fieldsLength; index++)
//            {
//                var serializer = serializers[index];
//                output.OpenField(serializer.FieldInfo.Name);
//                serializer.SerializationMethods.Serialize(objectAddress + serializer.Offset, output);
//                output.CloseField();
//            }

//            output.CloseObject();
//        }

//        private bool _root = true;

//        private void Writer(void* fieldAddress, SerializerOutput output)
//        {
//            var component = Unsafe.Read<Component>(fieldAddress);
//            if (component != null)
//            {
//                if (_root == false)
//                {
//                    var identifier = ObjectSerializer.GetUniqueId(component);
//                    output.WriteInt(identifier);
//                    return;
//                }

//                if (_uSerializer.GetTypeData(component.GetType(), out var typeData) == false)
//                    throw new Exception();

//                _root = false;

//                output.OpenArray();
//                {
//                    output.WriteInt(ObjectSerializer.GetUniqueId(component.gameObject));
//                    WriteFields(component, typeData, output);
//                }
//                output.CloseArray();

//                _root = true;
//            }
//            else
//            {
//                output.Null();
//            }
//        }

//        private void ReadFields(Component instance, Type type, SerializerInput input)
//        {
//            if (input.BeginReadObject())
//            {
//                if (_uSerializer.GetTypeData(type, out var typeData) == false)
//                {
//                    throw new Exception();
//                }

//                byte* objectAddress;
//                UnsafeUtility.CopyObjectAddressToPtr(instance, &objectAddress);

//                var fieldDatas = typeData.Fields;
//                var fieldsLength = fieldDatas.Length;

//                while (input.BeginReadField(out var field))
//                {
//                    for (var index = 0; index < fieldsLength; index++)
//                    {
//                        var fieldData = fieldDatas[index];

//                        if (field == fieldData.FieldInfo.Name)
//                        {
//                            fieldData.SerializationMethods.Deserialize(objectAddress + fieldData.Offset, input);
//                            break;
//                        }
//                    }

//                    input.CloseField();
//                }

//                input.CloseObject();
//            }
//            else
//            {
//                instance = null;
//            }

//        }


//        public bool TryGetSerializationMethods(Type type, out SerializationMethods writer)
//        {
//            if (_objectType.IsAssignableFrom(type))
//            {
//                void Deserialize(void* fieldAddress, SerializerInput input)
//                {
//                    ref var instance = ref Unsafe.AsRef<Component>(fieldAddress);

//                    input.BeginReadArray(out var arrayCount);
//                    {
//                        var gameObjectId = input.ReadInt();

//                        var gameObject = (GameObject)ObjectSerializer.GetInstance(_uSerializer, input, gameObjectId);

//                        if (input.BeginReadObject())
//                        {
//                            instance = gameObject.GetComponent(type);
//                            if (instance == null)
//                                instance = gameObject.AddComponent(type);

//                            ReadFields(instance, type, input);

//                            input.CloseObject();
//                        }
//                        else
//                        {
//                            instance = null;
//                        }
//                    }
//                    input.EndArray();
//                }

//                writer = new SerializationMethods(Writer, Deserialize);
//                return true;
//            }

//            writer = default;
//            return false;
//        }
//    }

//    public sealed unsafe class GameObjectSerializer : ISerializationProvider
//    {
//        private Type _objectType;

//        public UnityObjectSerializer ObjectSerializer;

//        private bool _root = true;

//        private List<int> _componentsIds;

//        private int _itemsFieldOffset;

//        private int _sizeFieldOffset;

//        private USerializer _uSerializer;

//        public void Initialize(USerializer serializer)
//        {
//            _uSerializer = serializer;
//            _objectType = typeof(GameObject);
//            _componentsIds = new List<int>(12);
//            ListUtilities.GetListOffsets(typeof(List<int>), out _itemsFieldOffset, out _sizeFieldOffset);
//        }

//        public void Writer(void* fieldAddress, SerializerOutput output)
//        {
//            var gameObject = Unsafe.Read<GameObject>(fieldAddress);
//            if (gameObject != null)
//            {
//                if ((gameObject.hideFlags & HideFlags.DontSave) == HideFlags.DontSave)
//                {
//                    output.Null();
//                    return;
//                }

//                if (_root == false)
//                {
//                    var identifier = ObjectSerializer.GetUniqueId(gameObject);
//                    output.WriteInt(identifier);
//                    return;
//                }

//                foreach (var component in gameObject.GetComponents<Component>())
//                {
//                    if ((component.hideFlags & HideFlags.DontSave) == HideFlags.DontSave)
//                        continue;

//                    var id = ObjectSerializer.AddDependency(component);

//                    _componentsIds.Add(id);
//                }

//                var array = _listHelper.GetArray(_componentsIds, out var count);

//                //output.WriteString(gameObject.name);
//                output.WriteIntArray(array, count);

//                _componentsIds.Clear();
//            }
//            else
//            {
//                output.Null();
//            }
//        }

//        private ListHelper<int> _listHelper = new ListHelper<int>();

//        public void Reader(void* fieldAddress, SerializerInput input)
//        {
//            ref var value = ref Unsafe.AsRef<Object>(fieldAddress);

//            if (input.CurrentNode.IsNull())
//            {
//                value = null;
//            }
//            else
//            {
//                if (input.CurrentNode.IsNumber())
//                {
//                    var id = input.ReadInt();
//                    value = ObjectSerializer.GetInstance(_uSerializer, input, id);
//                }
//                else
//                if (input.CurrentNode.IsArray())
//                {
//                    // var name = input.ReadString();
//                    value = new GameObject();

//                    var components = input.ReadIntArray();

//                    foreach (var id in components)
//                    {
//                        ObjectSerializer.GetInstance(_uSerializer, input, id);
//                    }
//                }
//            }
//        }

//        public bool TryGetSerializationMethods(Type type, out SerializationMethods methods)
//        {
//            if (_objectType.IsAssignableFrom(type))
//            {
//                methods = new SerializationMethods(Writer, Reader);
//                return true;
//            }

//            methods = default;
//            return false;
//        }
//    }

//}