using System;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace USerialization
{
    public static class BinaryUtility
    {
        private static USerializer _uSerializer;


        [RuntimeInitializeOnLoadMethod]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        private static void AutoInitialize()
        {
            var providers = ProvidersUtils.GetDefaultProviders();
            _uSerializer = new USerializer(new UnitySerializationPolicy(), providers);
        }

        public static bool Serialize([NotNull] object obj, [NotNull] Stream stream, int bufferSize = 4096 * 2)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var output = new SerializerOutput(bufferSize, stream);
            return _uSerializer.Serialize(output, obj);
        }

        public static bool TryDeserialize<T>([NotNull] Stream stream, out T result, int bufferSize = 4096 * 2)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return _uSerializer.TryDeserialize<T>(new SerializerInput(bufferSize, stream), out result);
        }

        public static bool TryPopulateObject<T>([NotNull] Stream stream, ref T ob, int bufferSize = 4096 * 2)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return _uSerializer.TryPopulateObject(new SerializerInput(bufferSize, stream), ref ob);
        }
    }
}