using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using USerialization;


//[assembly: CustomSerializer(typeof(Vector2Serializer), typeof(Vector2))]
//public sealed class Vector2Serializer : ICustomSerializer
//{
//    private FieldInfo _xField;
//    private FieldInfo _yField;


//    public Vector2Serializer()
//    {
//        _xField = typeof(Vector2).GetField("x");
//        _yField = typeof(Vector2).GetField("y");
//    }

//    public unsafe void Write(void* fieldAddress, SerializerOutput output)
//    {
//        ref var obj = ref Unsafe.AsRef<Vector2>(fieldAddress);
//        Debug.Log(obj);
//        output.BeginReadObject();

//        output.BeginReadField(_xField);
//        output.Write(obj.x.ToString(CultureInfo.InvariantCulture));
//        output.CloseField();

//        output.BeginReadField(_yField);
//        output.Write(obj.y.ToString(CultureInfo.InvariantCulture));
//        output.CloseField();

//        output.CloseObject();
//    }
//}