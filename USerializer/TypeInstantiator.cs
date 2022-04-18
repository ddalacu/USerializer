using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

public unsafe struct TypeInstantiator
{
    public readonly Type Type;
    private void* _ctor;

    public TypeInstantiator(Type type)
    {
        if (type == null)
            throw new ArgumentException("Type is null!");

        Type = type;
        var ctor = type.GetConstructor(Type.EmptyTypes);

        if (ctor != null)
        {
            try
            {
                _ctor = ctor.MethodHandle.GetFunctionPointer().ToPointer();
            }
            catch
                (NotSupportedException) //unity IL2CPP builds throw this exception and we need to use the real handle there 
            {
                _ctor = ctor.MethodHandle.Value.ToPointer();
            }
        }
        else
        {
            _ctor = null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object CreateInstance()
    {
        var inst = FormatterServices.GetUninitializedObject(Type);
        if (_ctor != null)
        {
            ((delegate*<object, void>) _ctor)(inst);
        }

        return inst;
    }
}