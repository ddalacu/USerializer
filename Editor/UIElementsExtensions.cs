using UnityEngine.UIElements;

public static class UIElementsExtensions
{
    public static bool GetLast(this VisualElement element, out VisualElement last)
    {
        var childCount = element.childCount;
        if (childCount == 0)
        {
            last = default;
            return false;
        }

        last = element[childCount - 1];
        return true;
    }

    public static bool RemoveLast(this VisualElement element, out VisualElement last)
    {
        var childCount = element.childCount;
        if (childCount == 0)
        {
            last = default;
            return false;
        }

        var lastIndex = childCount - 1;
        last = element[lastIndex];
        element.RemoveAt(lastIndex);
        return true;
    }

    public static bool GetLast<T>(this VisualElement element, out T last) where T : VisualElement
    {
        for (int i = element.childCount - 1; i >= 0; i--)
        {
            if (element[i] is T result)
            {
                last = result;
                return true;
            }
        }

        last = default;
        return false;
    }

    public static bool RemoveLast<T>(this VisualElement element, out T last) where T : VisualElement
    {
        for (int i = element.childCount - 1; i >= 0; i--)
        {
            if (element[i] is T result)
            {
                element.RemoveAt(i);

                last = result;
                return true;
            }
        }

        last = default;
        return false;
    }


    public static bool HaveChildrens(this VisualElement element)
    {
        return element.childCount != 0;
    }
}