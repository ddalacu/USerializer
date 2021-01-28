using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public interface IObject
{
    IReadOnlyList<IObject> Childrens { get; }

    string Name { get; }
    Texture Image { get; }
}

public class TreeView
{
    private readonly ListView _listView;
    private readonly HistoryElement _historyElement;
    private readonly List<IObject> _filteredObjects = new List<IObject>();

    public IObject ActiveFolder => (IObject)_historyElement.LastElement as IObject;

    public Action<IEnumerable<IObject>> SelectionChanged;

    public Action<IObject> Clicked;

    public TreeView(ListView listView, HistoryElement historyElement, IObject root)
    {
        _listView = listView;
        _historyElement = historyElement;

        listView.Refresh();

        listView.itemsSource = _filteredObjects;
        listView.makeItem = MakeListItem;
        listView.bindItem = BindListItem;

#if UNITY_2019_1_OR_NEWER
        listView.onSelectionChanged += SelectionChange;
#else
        listView.onSelectionChange += SelectionChange;
#endif

        _historyElement.LastChanged += LastChanged;

        _historyElement.Add(root, root.Name);
    }

    private void SelectionChange(IEnumerable<object> obj)
    {
        SelectionChanged?.Invoke(obj.Select(o => (IObject)o));
    }

    private void LastChanged(object obj)
    {
        Debug.Assert(obj == _historyElement.LastElement);

        if (obj is IObject s3Object)
        {
            EnterInFolder(s3Object);
        }
        else
        {
            EnterInFolder(null);
        }
    }

    private VisualElement MakeListItem()
    {
        var rootVisualItem = new VisualElement
        {
            style =
            {
                flexGrow = 1,
                flexShrink = 0,
                flexBasis = 0,
                flexDirection = FlexDirection.Row
            }
        };

        var image = new Image
        {
            name = "image",
            style =
            {
                top = 0,
                bottom = 0,
                width = 20,
                marginLeft = 4,
                marginRight = 4
            },
        };

        image.scaleMode = ScaleMode.ScaleAndCrop;
        rootVisualItem.Add(image);


        var label = new Label
        {
            name = "label"
        };

        rootVisualItem.RegisterCallback((PointerDownEvent pointerEvent, VisualElement instance) =>
        {
            if (pointerEvent.clickCount == 2)
            {
                var ob = (IObject)instance.userData;

                if (ob.Childrens != null)
                {
                    _historyElement.Add(ob, ob.Name);
                }
                else
                {
                    Clicked?.Invoke(ob);
                }
            }
        }, rootVisualItem);

        rootVisualItem.Add(label);


        return rootVisualItem;
    }

    private void BindListItem(VisualElement e, int i)
    {
        var label = e.Q<Label>("label");
        var folderImage = e.Q<Image>("image");

        var eUserData = _filteredObjects[i];

        e.userData = eUserData;

        label.text = eUserData.Name;

        if (eUserData.Image != null)
        {
            folderImage.visible = true;
            folderImage.image = eUserData.Image;
            folderImage.SendToBack();
        }
        else
        {
            folderImage.visible = false;
            folderImage.BringToFront();
        }

        e.style.alignItems = Align.Center;
    }

    private void EnterInFolder(IObject s3Object)
    {
        _filteredObjects.Clear();
        _filteredObjects.AddRange(s3Object.Childrens);
        _listView.selectedIndex = -1;
        _listView.Refresh();
    }
}