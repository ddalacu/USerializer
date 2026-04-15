using System;
using System.Reflection;

namespace USerialization
{
    public interface IRuntimeUtils
    {
        int GetFieldOffset(FieldInfo fi);
        int GetStackSize(Type type);
        int GetClassHeapSize(Type type);
    }
}
