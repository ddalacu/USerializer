using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace USerialization
{
    public readonly unsafe struct InstanceWriteMethodPointer
    {
        private readonly object _instance;
        private readonly void* _address;

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _address != null;
        }

        public InstanceWriteMethodPointer(IntPtr address, object instance)
        {
            _instance = instance;
            _address = address.ToPointer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(void* fieldAddress, SerializerOutput output)
        {
            //if (_address == null)
                //throw new Exception("Address is null");

            var del = (delegate* managed<object, void*, SerializerOutput, void>)_address;
            del(_instance, fieldAddress, output);
        }
    }

    public readonly unsafe struct InstanceReadMethodPointer
    {
        private readonly object _instance;
        private readonly void* _address;


        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _address != null;
        }

        public InstanceReadMethodPointer(IntPtr address, object instance)
        {
            _instance = instance;
            _address = address.ToPointer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(void* fieldAddress, SerializerInput input)
        {
           // if (_address == null)
               // throw new Exception("Address is null");

            var del = (delegate* managed<object, void*, SerializerInput, void>)_address;
            del(_instance, fieldAddress, input);
        }
    }

    public abstract unsafe class DataSerializer
    {
        private bool _initialized;

        public abstract DataType GetDataType();

        protected abstract void Write(void* fieldAddress, SerializerOutput output);

        protected abstract void Read(void* fieldAddress, SerializerInput input);

        public InstanceWriteMethodPointer WriteMethod { get; protected set; }

        public InstanceReadMethodPointer ReadMethod { get; protected set; }

        //public void LocalWrite(void* fieldAddress, SerializerOutput output)
        //{
        //    Console.WriteLine($"Using base write {this}");
        //    Write(fieldAddress, output);
        //}

        //public void LocalRead(void* fieldAddress, SerializerInput input)
        //{
        //    Console.WriteLine($"Using base read {this}");
        //    Read(fieldAddress, input);
        //}

        private  IntPtr GetAddress(string methodName)
        {
            var type = GetType();
            var methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (methodInfo == null)
                throw new Exception($"Could not find method {methodName} on {type}");

            return methodInfo.MethodHandle.GetFunctionPointer();
        }

        public void RootInitialize(USerializer serializer)
        {
            if (_initialized)
                return;

            _initialized = true;


            var topWriteAddress = GetAddress(nameof(Write));
            WriteMethod = new InstanceWriteMethodPointer(topWriteAddress, this);

            var topReadAddress = GetAddress(nameof(Read));
            ReadMethod = new InstanceReadMethodPointer(topReadAddress, this);

            //var writeAddress = ILUtils.GetAddress<DataSerializer>(nameof(LocalWrite));
            //WriteMethod = new InstanceWriteMethodPointer(writeAddress, this);

            //var readAddress = ILUtils.GetAddress<DataSerializer>(nameof(LocalRead));
            //ReadMethod = new InstanceReadMethodPointer(readAddress, this);

            Initialize(serializer);

            var dataType = GetDataType();

            if (dataType == DataType.None)
                serializer.Logger.Error($"Data type is none {this}, something went wrong!");
        }

        protected abstract void Initialize(USerializer serializer);
    }

}