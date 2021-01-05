using System;
using System.Reflection;

namespace Traverse
{
    public interface ITraversePolicy
    {
        bool ShouldTraverse(Type type);

        bool ShouldTraverse(FieldInfo fieldInfo);
    }
}