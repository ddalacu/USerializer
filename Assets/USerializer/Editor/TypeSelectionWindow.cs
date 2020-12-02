using System;
using System.Collections.Generic;
using System.Linq;
using EditorUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TypeSelectionWindow : EditorWindow
{
    private TemplateContainer _loaded;

    public event Action<Type> TypeSelected;

    private static IEnumerable<IObject> Get(IEnumerable<Type> types)
    {
        var collection = new Dictionary<string, List<Type>>(2048);
        foreach (var type in types)
        {
            var names = type.Namespace;

            if (string.IsNullOrEmpty(names))
                names = "Root";

            if (collection.TryGetValue(names, out var list) == false)
            {
                list = new List<Type>();
                collection.Add(names, list);
            }

            list.Add(type);
        }

        foreach (var entry in collection)
        {
            if (entry.Key != "Root")
            {
                var childrens = new List<IObject>();

                foreach (var type in entry.Value)
                {
                    childrens.Add(new TypeNavType(type));
                }

                var obj = new TypeNavNamespace(entry.Key, childrens);
                yield return obj;
            }
            else
            {
                foreach (var type in entry.Value)
                {
                    yield return new TypeNavType(type);
                }
            }
        }
    }

    public void SetTypes(IEnumerable<Type> types)
    {
        var buildTypesDictionary = Get(types).ToArray();

        var root = new TypeNavNamespace("Root", new List<IObject>(buildTypesDictionary));

        root.Build();

        var v = new Tree(_loaded.Q<ListView>(), _loaded.Q<HistoryElement>(), root);
        v.Clicked += (item) =>
        {
            TypeSelected?.Invoke(((TypeNavType)item).Type);
            Close();
        };
    }


    private void OnEnable()
    {
        var historyElement = new HistoryElement();
        historyElement.style.flexGrow = 1;
        historyElement.style.flexDirection = FlexDirection.Row;

        var generator = ObjectFinder<VisualTreeAsset>.FindObject("TypeSelection");

        _loaded = generator.CloneTree();
        _loaded.style.flexGrow = 1;
        _loaded.Q("HistoryElement").Add(historyElement);
        rootVisualElement.Add(_loaded);
    }

    public class TypeNavNamespace : IObject
    {
        private readonly List<IObject> _childrens;
        public IReadOnlyList<IObject> Childrens => _childrens;
        public string Name { get; }
        public Texture Image => (Texture)EditorGUIUtility.Load("Folder Icon");

        public TypeNavNamespace(string name, List<IObject> childrens)
        {
            Name = name;
            _childrens = childrens;
        }

        public void Build()
        {
            _childrens.Sort((a, b) =>
            {
                if (a.Name.Length < b.Name.Length)
                    return -1;
                if (a.Name.Length > b.Name.Length)
                    return 1;
                return 0;
            });

            for (int i = 0; i < _childrens.Count; i++)
            {
                var test = _childrens[i];
                if (test is TypeNavNamespace nms)
                    for (var j = _childrens.Count - 1; j > i; j--)
                    {
                        var children = _childrens[j];

                        if (children is TypeNavNamespace ch)
                            if (ch.Name.StartsWith(nms.Name))
                            {
                                _childrens.RemoveAt(j);
                                nms._childrens.Add(ch);
                            }
                    }
            }

            for (int i = 0; i < _childrens.Count; i++)
            {
                var test = _childrens[i];
                if (test is TypeNavNamespace nms)
                    nms.Build();
            }
        }
    }

    public class TypeNavType : IObject
    {
        public IReadOnlyList<IObject> Childrens => null;
        public string Name { get; }

        public Texture Image => EditorGUIUtility.ObjectContent(null, Type).image;
        public Type Type { get; }

        public TypeNavType(Type type)
        {
            Type = type;
            Name = type.GetFriendlyName();
        }

    }

    private void OnDisable()
    {
        rootVisualElement.Clear();
    }
}
