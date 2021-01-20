using System.Collections.Generic;
using System.Reflection;

namespace USerialization
{
    public class FieldInfoComparer : IEqualityComparer<FieldInfo>
    {
        public static readonly FieldInfoComparer DefaultInstance = new FieldInfoComparer();

        public bool Equals(FieldInfo x, FieldInfo y)
        {
            return x.DeclaringType == y.DeclaringType && x.Name == y.Name;
        }

        public int GetHashCode(FieldInfo obj)
        {
            return obj.Name.GetHashCode() ^ obj.DeclaringType.GetHashCode();
        }
    }
}