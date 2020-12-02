using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class HistoryElement : VisualElement
{
    public new class UxmlFactory : UxmlFactory<HistoryElement, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get
            {
                yield break;
            }
        }
    }

    public object LastElement
    {
        get
        {
            if (this.GetLast<HistoryItem>(out var last))
                return last.userData;
            return null;
        }
    }
    
    public Action<object> LastChanged;

    public HistoryElement()
    {
        AddToClassList(ToolbarBreadcrumbs.ussClassName);
    }

    private class HistoryItem : ToolbarButton
    {
        public HistoryItem()
        {
            RemoveFromClassList(ussClassName);
            AddToClassList(ToolbarBreadcrumbs.itemClassName);
        }
    }

    public void Add(object obj, string displayName)
    {
        var element = new HistoryItem();
        element.text = displayName;
        element.userData = obj;

        if (childCount == 0)
        {
            element.EnableInClassList(ToolbarBreadcrumbs.firstItemClassName, this.childCount == 0);
        }

        element.AddToClassList(ToolbarBreadcrumbs.itemClassName);
        element.EnableInClassList(ToolbarBreadcrumbs.itemClassName,true);

        this.RemoveFromClassList(ToolbarButton.ussClassName);
        this.AddToClassList(ToolbarBreadcrumbs.itemClassName);

        element.clicked += () =>
        {
            if (this.GetLast<HistoryItem>(out var lastCurrent) && lastCurrent != element)
                GoToStack(element);
        };

        ////todo this
        //element.RegisterCallback((ContextClickEvent evt, VisualElement caller) =>
        //{
        //    if (this.GetLast<HistoryItem>(out var lastCurrent) && lastCurrent != caller)
        //        GoToStack(caller);
        //}, element);

        if (this.GetLast<HistoryItem>(out var last))
            SetElementClickable(last, true);

        SetElementClickable(element, false);

        Add(element);

        LastChanged?.Invoke(obj);
    }

    public void SetElementClickable(VisualElement element, bool state)
    {
        if (state)
            element.style.unityFontStyleAndWeight = FontStyle.Normal;
        else
            element.style.unityFontStyleAndWeight = FontStyle.Bold;
    }

    private void GoBack()
    {
        if (this.RemoveLast<HistoryItem>(out var last))
        {
            if (this.GetLast<HistoryItem>(out var newLast))
            {
                SetElementClickable(newLast, false);
                LastChanged?.Invoke(newLast.userData);
            }
            else
            {
                LastChanged?.Invoke(null);
            }
        }
    }

    private void GoToStack(VisualElement element)
    {
        var buttonIndex = IndexOf(element);

        for (int i = childCount - 1; i > buttonIndex; i--)
            RemoveAt(i);

        this.GetLast<HistoryItem>(out var last);
        SetElementClickable(last, false);
        LastChanged?.Invoke(last.userData);
    }

    public void ClearHistory()
    {
        Clear();
        LastChanged?.Invoke(null);
    }
}