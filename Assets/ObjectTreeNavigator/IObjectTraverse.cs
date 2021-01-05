using System;

namespace Traverse
{
    public interface IObjectTraverse
    {
        void Initialize(ObjectTreeNavigator navigator);

        void Start(ObjectTreeNavigator navigator);

        bool TryGetTraverseMethod(Type fieldType, out TraverseDelegate methods);
    }
}