using System;
using System.Reflection;
using Traverse;
using UnityEngine;
using Object = UnityEngine.Object;


public class ObjectTreeNavigateExample : MonoBehaviour
{
    public class UnityTraversePolicy : ITraversePolicy
    {
        public bool ShouldTraverse(Type type)
        {
            if (type.IsGenericType)// Type<int>
                return false;

            if (type.IsGenericTypeDefinition)// Type<>
                return false;

            if (type.IsClass)
            {
                if (typeof(Object).IsAssignableFrom(type))
                    return true;

                if (type.GetCustomAttribute<SerializableAttribute>() != null)
                    return true;
            }

            if (type.IsValueType)
            {
                return true;
            }

            return false;
        }

        public bool ShouldTraverse(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsPrivate)
            {
                if (Attribute.IsDefined(fieldInfo, typeof(SerializeField)) == false)//todo cache typeof
                    return false;
            }
            else
            {
                if (Attribute.IsDefined(fieldInfo, typeof(NonSerializedAttribute))) //todo cache typeof
                    return false;
            }

            return true;
        }
    }

    public class CustomClassTraverse : ClassTraverse
    {
        public CustomClassTraverse(ChangeObjectDelegate change) : base(change)
        {
        }

        protected override void Init()
        {
            AddField((ref SpriteRenderer obj, Sprite spr) => obj.sprite = spr,
                (ref SpriteRenderer spriteRenderer) => spriteRenderer.sprite);
        }
    }

    public Object Ref;

    public Object[] List;

    public SpriteRenderer rend;

    [ContextMenu("Test")]
    private void Test()
    {
        var objectTraverse = new CustomClassTraverse(Change);

        var navigator = new ObjectTreeNavigator(new IObjectTraverse[]
        {
            objectTraverse,
            new StructTraverse(),
            new ArrayTraverse(),
            new ListTraverse(),
        }, new UnityTraversePolicy());

        var ob = this;

        var context = new TraverseContext();

        navigator.Traverse(ref ob, context);
    }

    private object Change(object @from, Type fieldType)
    {
        if (@from is Sprite spr)
        {
            //Debug.Log(from);
            Debug.Log(@from);
        }

        return @from;
    }
}
