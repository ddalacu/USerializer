using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using USerialization;
using Object = UnityEngine.Object;

public class GenerationWindow : EditorWindow
{
    [MenuItem("Tools/GenerateSerialization")]
    private static void Init()
    {
        var window = (GenerationWindow)EditorWindow.GetWindow(typeof(GenerationWindow));
        window.Show();
    }

    private Type _selectedType;

    private List<PropertyInfo> _props = new List<PropertyInfo>();

    private List<bool> _enabledProps = new List<bool>();

    private ListView _propertiesView;

    private Label _generatedLabel;
    private Button _saveButton;

    private void OnEnable()
    {
        var selectTypeButton = new Button()
        {
            text = "Select type"
        };
        selectTypeButton.clickable.clickedWithEventInfo += (inf) =>
        {
            TypeSelectionWindow window = EditorWindow.CreateInstance<TypeSelectionWindow>();

            var rec = this.position;
            var target = ((VisualElement)inf.currentTarget);

            window.ShowAsDropDown(new Rect(rec.position, target.worldBound.position + target.worldBound.size), new Vector2(target.worldBound.size.x, 200));


            window.SetTypes(TypeCache.GetTypesDerivedFrom<Object>());
            window.TypeSelected += (type) =>
            {
                selectTypeButton.text = type.FullNameNiceFormat();
                SetType(type);
            };
        };

        rootVisualElement.Add(selectTypeButton);

        _propertiesView = new ListView();
        _propertiesView.makeItem = () =>
        {
            var visualElement = new VisualElement();
            visualElement.style.flexDirection = FlexDirection.Row;

            visualElement.Add(new Label());
            var element = new Toggle();
            element.RegisterValueChangedCallback(evt =>
            {
                _enabledProps[(int)((Toggle)evt.currentTarget).userData] = evt.newValue;
                Generate();
            });
            visualElement.Add(element);
            return visualElement;
        };

        _propertiesView.bindItem = (element, index) =>
        {
            element.Q<Label>().text = _props[index].Name;
            element.Q<Toggle>().SetValueWithoutNotify(_enabledProps[index]);
            element.Q<Toggle>().userData = index;
        };

        _propertiesView.itemsSource = _props;

        _propertiesView.style.flexGrow = 1;
        _propertiesView.itemHeight = 20;

        var bottomPart = new VisualElement();
        bottomPart.style.flexGrow = 1;
        bottomPart.style.flexDirection = FlexDirection.Row;

        bottomPart.Add(_propertiesView);

        _generatedLabel = new Label();
        _generatedLabel.style.flexGrow = 1;
        bottomPart.Add(_generatedLabel);

        rootVisualElement.Add(bottomPart);


        _saveButton = new Button()
        {
            text = "Save"
        };
        _saveButton.visible = false;
        _saveButton.clicked += () =>
        {
            var ownerType = _selectedType.FullName.Replace('+', '_').Replace('.', '_') + "Serializer";

            var dir = EditorPrefs.GetString("GenerateSaveFilePanel", "Assets");
            var result = EditorUtility.SaveFilePanel("File", dir, ownerType + ".cs", "cs");

            if (string.IsNullOrEmpty(result))
                return;

            EditorPrefs.SetString("GenerateSaveFilePanel", Path.GetDirectoryName(result));

            File.WriteAllText(result, _generatedLabel.text);
            AssetDatabase.Refresh();
        };

        rootVisualElement.Add(_saveButton);
    }

    private void SetType(Type type)
    {
        _selectedType = type;
        _props.Clear();
        _enabledProps.Clear();

        var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        foreach (var propertyInfo in props)
        {
            if (propertyInfo.GetCustomAttribute<ObsoleteAttribute>() != null)
                continue;

            if (propertyInfo.CanRead == false || propertyInfo.CanWrite == false)
                continue;
            _props.Add(propertyInfo);
            _enabledProps.Add(true);
        }

        _propertiesView.Refresh();
        Generate();
    }

    private void Generate()
    {

        var ownerType = _selectedType.FullName.Replace('+', '.');

        var builder = new StringBuilder();
        builder.Append("using USerialization;\n");
        builder.Append(
            "public class " + ownerType.Replace('.', '_') + "Serializer : CustomSerializerBase<" + ownerType + ">\r\n{\r\n    public override void LocalInit()\r\n    {\n");


        for (var index = 0; index < _props.Count; index++)
        {
            if (_enabledProps[index] == false)
                continue;

            var propertyInfo = _props[index];
            var memberType = propertyInfo.PropertyType.FullName.Replace('+', '.');

            var hash = propertyInfo.Name.GetInt32Hash();

            var str = $"        AddField({hash}, (ref {ownerType} obj, {memberType} val) => obj.{propertyInfo.Name} = val, (ref {ownerType} obj) => obj.{propertyInfo.Name});\n";
            builder.Append(str);
        }

        builder.Append("    }\r\n}");

        _generatedLabel.text = builder.ToString();

        _saveButton.visible = true;
    }

    private void OnDisable()
    {
        rootVisualElement.Clear();
    }
}