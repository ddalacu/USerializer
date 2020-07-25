using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace USerialization
{
    public static class TypeExtensions
    {
        private static readonly HashSet<Type> EventSet = new HashSet<Type>
        {
            typeof(Action),//0
            typeof(Action<>),//1
            typeof(Action<,>),//2
            typeof(Action<,,>),//3
            typeof(Action<,,,>),//4
#if NET_4_6
        typeof(Action<,,,,>),//5
        typeof(Action<,,,,,>),//6
        typeof(Action<,,,,,,>),//7
        typeof(Action<,,,,,,,>),//8
        typeof(Action<,,,,,,,,>),//9
        typeof(Action<,,,,,,,,,>),//10
        typeof(Action<,,,,,,,,,,>),//11
        typeof(Action<,,,,,,,,,,,>),//12
        typeof(Action<,,,,,,,,,,,,>),//13
        typeof(Action<,,,,,,,,,,,,,>),//14
        typeof(Action<,,,,,,,,,,,,,,>),//15
        typeof(Action<,,,,,,,,,,,,,,,>),//16
#endif
            typeof(Func<>),//1
            typeof(Func<,>),//2
            typeof(Func<,,>),//3
            typeof(Func<,,,>),//4
            typeof(Func<,,,,>),//5
#if NET_4_6
        typeof(Func<,,,,,>),//6
        typeof(Func<,,,,,,>),//7
        typeof(Func<,,,,,,,>),//8
        typeof(Func<,,,,,,,,>),//9
        typeof(Func<,,,,,,,,,>),//10
        typeof(Func<,,,,,,,,,,>),//11
        typeof(Func<,,,,,,,,,,,>),//12
        typeof(Func<,,,,,,,,,,,,>),//13
        typeof(Func<,,,,,,,,,,,,,>),//14
        typeof(Func<,,,,,,,,,,,,,,>),//15
        typeof(Func<,,,,,,,,,,,,,,,>),//16
        typeof(Func<,,,,,,,,,,,,,,,,>),//17
#endif
        };

        public static string FullNameSafeFormat(this Type type)
        {
            if (!type.IsGenericType)
            {
                string name = type.FullName;
                name = name.Replace('+', '_');
                name = name.Replace('\'', '_');
                return name.Replace('.', '_');
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(type.Name.Substring(0, type.Name.LastIndexOf("`", StringComparison.InvariantCulture)));
            sb.Append(type.GetGenericArguments().Aggregate("_",
                delegate (string aggregate, Type t)
                {
                    return aggregate + "_" + FullNameSafeFormat(t);
                }
            ));
            return sb.ToString();
        }

        public static string FullNameNiceFormat(this Type type)
        {
            string name = type.FullName;
            name = name.Replace('+', '.');
            name = name.Replace('\'', '.');
            return name;
        }

        public static string FullNameNiceFormatAndNamespace(this Type type)
        {
            string name = type.FullName;
            name = name.Replace('+', '.');
            name = name.Replace('\'', '.');
            if (string.IsNullOrEmpty(type.Namespace) == false)
            {
                return type.Namespace + "." + name;
            }

            return name;
        }

        public static bool IsObsolete(this Type info)
        {
            foreach (object ob in info.GetCustomAttributes(false))
            {
                if (ob is ObsoleteAttribute && (ob as ObsoleteAttribute).IsError)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsEvent(this Type type)
        {

            if (typeof(Delegate).IsAssignableFrom(type) || typeof(MulticastDelegate).IsAssignableFrom(type))
            {
                return true;
            }

            if (EventSet.Contains(type) || (type.IsGenericType && EventSet.Contains(type.GetGenericTypeDefinition())))
            {
                return true;
            }

            return false;
        }

        public static bool HaveDefaultConstructor(this Type type)
        {
            return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
        }


        public static bool CanGenerateSerializer(this Type type)
        {
            if (type == null)
            {
                return false;
            }

            if (type.IsPrimitive || type == typeof(string) || type == typeof(void) || type == typeof(TypedReference))
            {
                return false;
            }

            if (type.IsAbstract)
            {
                return false;
            }

            if (type.IsInterface)
            {
                return false;
            }

            if (type.IsVisible == false)
            {
                return false;
            }

            if (type.IsGenericType)
            {
                return false;
            }

            if (type.ContainsGenericParameters)
            {
                return false;
            }

            if (type.IsObsolete())
            {
                return false;
            }

            if (type.IsEvent())
            {
                return false;
            }

            return true;
        }
    }
}