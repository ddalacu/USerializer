﻿using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace USerialization
{
    public unsafe class ArraySerializer : ISerializationProvider
    {
        private USerializer _serializer;

        public void Initialize(USerializer serializer)
        {
            _serializer = serializer;
        }

        public void Start(USerializer serializer)
        {
            
        }

        public bool TryGetSerializationMethods(Type type, out SerializationMethods serializationMethods)
        {
            if (type.IsArray == false)
            {
                serializationMethods = default;
                return false;
            }

            if (type.GetArrayRank() > 1)
            {
                serializationMethods = default;
                return false;
            }

            var elementType = type.GetElementType();

            if (_serializer.TryGetSerializationMethods(elementType, out var elementSerializationMethods) == false)
            {
                serializationMethods = default;
                return false;
            }

            if (elementType.IsValueType)
            {
                var writer = GetValueTypeWriter(elementType, elementSerializationMethods.Serialize, elementSerializationMethods.DataType);
                var reader = GetValueTypeReader(elementType, elementSerializationMethods.Deserialize, elementSerializationMethods.DataType);

                serializationMethods = new SerializationMethods(writer, reader, DataType.Array);
            }
            else
            {
                var writer = GetReferenceTypeWriter(elementSerializationMethods.Serialize, elementSerializationMethods.DataType);
                var reader = GetReferenceTypeReader(elementType, elementSerializationMethods.Deserialize, elementSerializationMethods.DataType);

                serializationMethods = new SerializationMethods(writer, reader, DataType.Array);
            }

            return true;
        }

        private static WriteDelegate GetValueTypeWriter(Type elementType, WriteDelegate serializeElement, DataType dataType)
        {
            var size = UnsafeUtility.SizeOf(elementType);

            return delegate (void* fieldAddress, SerializerOutput output)
             {
                 var array = Unsafe.Read<Array>(fieldAddress);

                 if (array != null)
                 {
                     var count = array.Length;

                     var sizeTracker = output.BeginSizeTrack();
                     {
                         output.WriteByte((byte)dataType);
                         output.WriteInt(array.Length);

                         var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                         for (var index = 0; index < count; index++)
                         {
                             serializeElement(address, output);
                             address += size;
                         }

                         UnsafeUtility.ReleaseGCObject(handle);
                     }
                     output.WriteSizeTrack(sizeTracker);
                 }
                 else
                 {
                     output.WriteNull();
                 }
             };
        }

        private static ReadDelegate GetValueTypeReader(Type elementType, ReadDelegate elementReader, DataType dataType)
        {
            var size = UnsafeUtility.SizeOf(elementType);

            return delegate (void* fieldAddress, SerializerInput input)
            {
                ref var array = ref Unsafe.AsRef<Array>(fieldAddress);

                if (input.BeginReadSize(out var end))
                {
                    var type = (DataType)input.ReadByte();

                    var count = input.ReadInt();
                    array = Array.CreateInstance(elementType, count);

                    if (type == dataType)
                    {
                        var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                        for (var i = 0; i < count; i++)
                        {
                            elementReader(address, input);
                            address += size;
                        }

                        UnsafeUtility.ReleaseGCObject(handle);
                    }

                    input.EndObject(end);
                }
                else
                {
                    array = null;
                }
            };
        }

        private static WriteDelegate GetReferenceTypeWriter(WriteDelegate serializeElement, DataType dataType)
        {
            return delegate (void* fieldAddress, SerializerOutput output)
            {
                var array = Unsafe.Read<object[]>(fieldAddress);

                if (array != null)
                {
                    var count = array.Length;

                    var sizeTracker = output.BeginSizeTrack();
                    {
                        output.WriteByte((byte)dataType);
                        output.WriteInt(count);

                        var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                        for (var index = 0; index < count; index++)
                        {
                            var o = array[index];
                            if (o != null)
                            {
                                serializeElement(address, output);
                            }
                            else
                            {
                                output.WriteNull();
                            }

                            address += sizeof(void*);
                        }

                        UnsafeUtility.ReleaseGCObject(handle);
                    }
                    output.WriteSizeTrack(sizeTracker);
                }
                else
                {
                    output.WriteNull();
                }
            };
        }

        private static ReadDelegate GetReferenceTypeReader(Type elementType, ReadDelegate elementReader, DataType dataType)
        {
            return delegate (void* fieldAddress, SerializerInput input)
            {
                ref var array = ref Unsafe.AsRef<object[]>(fieldAddress);

                if (input.BeginReadSize(out var end))
                {
                    var type = (DataType)input.ReadByte();
                    var count = input.ReadInt();
                    array = (object[])Array.CreateInstance(elementType, count);

                    if (type == dataType)
                    {
                        var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                        for (var i = 0; i < count; i++)
                        {
                            elementReader(address, input);
                            address += sizeof(void*);
                        }

                        UnsafeUtility.ReleaseGCObject(handle);
                    }

                    input.EndObject(end);
                }
                else
                {
                    array = null;
                }
            };
        }

    }
}