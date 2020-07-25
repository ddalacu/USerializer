//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using USerialization;
//using Object = UnityEngine.Object;

//public class UnityObjectSerializer
//{
//    public struct ObjectAndId
//    {
//        public Object Obj;
//        public int Identifier;

//        public ObjectAndId(Object obj, int identifier)
//        {
//            Obj = obj;
//            Identifier = identifier;
//        }
//    }

//    private Queue<ObjectAndId> _toSerialize = new Queue<ObjectAndId>();
//    private HashSet<int> _toBeSerialized = new HashSet<int>();
//    private readonly List<Component> _componentsBuffer = new List<Component>(64);
//    private readonly Type _componentType = typeof(Component);

//    public int GetUniqueId(Object o)
//    {
//        return o.GetInstanceID();
//    }

//    public int AddDependency(Object obj)
//    {
//        var uniqueId = GetUniqueId(obj);

//        if (_toBeSerialized.Add(uniqueId))
//        {
//            _toSerialize.Enqueue(new ObjectAndId(obj, uniqueId));
//        }

//        return uniqueId;
//    }

//    private void AddRoots(Transform transform)
//    {
//        var gameObject = transform.gameObject;

//        if ((gameObject.hideFlags & HideFlags.DontSave) == HideFlags.DontSave)
//            return;

//        int goId = GetUniqueId(gameObject);
//        _toBeSerialized.Add(goId);
//        _toSerialize.Enqueue(new ObjectAndId(gameObject, goId));

//        var transformChildCount = transform.childCount;
//        for (int i = 0; i < transformChildCount; i++)
//            AddRoots(transform.GetChild(i));

//        var buff = _componentsBuffer;
//        gameObject.GetComponents(_componentType, buff);
//        var bufferCount = buff.Count;
//        for (int i = 0; i < bufferCount; i++)
//        {
//            Component comp = buff[i];

//            if (comp == null)
//                continue;

//            if ((comp.hideFlags & HideFlags.DontSave) == HideFlags.DontSave)
//                continue;

//            int runtimeIdentifier = GetUniqueId(comp);
//            _toBeSerialized.Add(runtimeIdentifier);
//            _toSerialize.Enqueue(new ObjectAndId(comp, runtimeIdentifier));
//        }
//    }


//    public void Serialize(USerializer serializer, SerializerOutput output, GameObject o)
//    {
//        _toSerialize.Clear();
//        _toBeSerialized.Clear();

//        //AddRoots(o.transform);

//        AddDependency(o);

//        var unityObjectProvider = serializer.GetProvider<ComponentProvider>();
//        unityObjectProvider.ObjectSerializer = this;

//        var gameObjectSerializer = serializer.GetProvider<GameObjectSerializer>();
//        gameObjectSerializer.ObjectSerializer = this;

//        var customSerializer = serializer.GetProvider<CustomSerializerProvider>();
//        customSerializer.TryGetInstance(typeof(Transform), out TransformSerializer transformSerializer);
//        transformSerializer.ObjectSerializer = this;


//        output.OpenArray();
//        while (_toSerialize.Count > 0)
//        {
//            var item = _toSerialize.Dequeue();
//            output.WriteString(item.Obj.GetType().AssemblyQualifiedName);
//            output.WriteInt(item.Identifier);

//            serializer.Serialize(output, item.Obj);
//        }
//        output.CloseArray();
//    }

//    private class DeserializeData
//    {
//        public Type Type;
//        public SerializerInput.Node Node;

//        private Object _instance;
//        private bool _deserialized;

//        public Object GetInstance(USerializer serializer, SerializerInput input)
//        {
//            if (_deserialized)
//                return _instance;
//            _deserialized = true;

//            //input.CurrentNodeId = Node;

//            throw new NotImplementedException();

//            serializer.DeserializeObject(input, Type, ref _instance);

//            return _instance;
//        }
//    }

//    public Object GetInstance(USerializer serializer, SerializerInput input, int id)
//    {
//        if (_nodes.TryGetValue(id, out var data))
//        {
//            return data.GetInstance(serializer, input);
//        }
//        Debug.Log("wtf");
//        return null;
//    }

//    private Dictionary<int, DeserializeData> _nodes = new Dictionary<int, DeserializeData>();

//    public void Deserialize(USerializer serializer, SerializerInput input)
//    {
//        _nodes = new Dictionary<int, DeserializeData>();

//        if (input.BeginReadArray(out var count))
//        {
//            for (int i = 0; i < count; i += 3)
//            {
//                var type = input.ReadString();
//                var identifier = input.ReadInt();
//                var nodeRef = input.CurrentNode;

//                _nodes.Add(identifier, new DeserializeData()
//                {
//                    Type = Type.GetType(type),
//                    Node = nodeRef
//                });
//            }

//            input.EndArray();
//        }

//        foreach (var entry in _nodes.Values)
//        {
//            var result = entry.GetInstance(serializer, input);

//            //Debug.Log(result);
//        }
//    }

//}