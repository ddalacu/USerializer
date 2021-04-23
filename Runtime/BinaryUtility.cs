﻿using System;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace USerialization
{
    /// <summary>
    /// Replacement for <see cref="JsonUtility"/>
    /// </summary>
    public static class BinaryUtility
    {
        private static USerializer _uSerializer;

        public static USerializer USerializer => _uSerializer;

        static BinaryUtility()
        {
            var providers = ProvidersUtils.GetDefaultProviders();
            _uSerializer = new USerializer(new UnitySerializationPolicy(), providers, new DataTypesDatabase());
        }

        /// <summary>
        /// Serializes a object fields to a stream
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
        /// <param name="bufferSize">buffer size used to copy to the stream</param>
        /// <returns>false if type is not serializable</returns>
        public static bool Serialize([NotNull] object obj, [NotNull] Stream stream, int bufferSize = 4096 * 2)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var output = new SerializerOutput(bufferSize, stream);
            var serialize = _uSerializer.Serialize(output, obj);
            output.Flush();

            return serialize;
        }

        /// <summary>
        /// Creates a object with data from the stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="result"></param>
        /// <param name="bufferSize"></param>
        /// <returns>false if type is not serializable</returns>
        public static bool TryDeserialize<T>([NotNull] Stream stream, out T result, int bufferSize = 4096 * 2)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var serializerInput = new SerializerInput(bufferSize, stream);
            var tryDeserialize = _uSerializer.TryDeserialize<T>(serializerInput, out result);
            serializerInput.FinishRead();
            return tryDeserialize;
        }

        /// <summary>
        /// Populates a object fields with data from the stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="ob"></param>
        /// <param name="bufferSize"></param>
        /// <returns>false if type is not serializable</returns>
        public static bool TryPopulateObject<T>([NotNull] Stream stream, ref T ob, int bufferSize = 4096 * 2)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var serializerInput = new SerializerInput(bufferSize, stream);
            var tryPopulateObject = _uSerializer.TryPopulateObject(serializerInput, ref ob);
            serializerInput.FinishRead();
            return tryPopulateObject;
        }
    }
}