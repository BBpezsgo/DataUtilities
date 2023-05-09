using System;
using System.Collections.Generic;

namespace DataUtilities.Serializer
{
    /// <summary>
    /// This class handles serialization to a raw binary data array.
    /// To start the process, create an instance.
    /// You can then call the instance methods like <see cref="Serialize(int)"/> or <see cref="Serialize(string)"/>.
    /// When you're done, you can extract the created byte array from the <see cref="Result"/> property.
    /// </summary>
    public class Serializer
    {
        readonly List<byte> result = new();

        public byte[] Result => result.ToArray();

        readonly Dictionary<Type, Delegate> typeSerializers;

        delegate void TypeSerializer<T>(T v);
        static KeyValuePair<Type, Delegate> GenerateTypeSerializer<T>(TypeSerializer<T> typeSerializer) => new(typeof(T), typeSerializer);

        public Serializer()
        {
            typeSerializers = (new KeyValuePair<Type, Delegate>[]
            {
                GenerateTypeSerializer<byte>(Serialize),
                GenerateTypeSerializer<bool>(Serialize),

                GenerateTypeSerializer<short>(Serialize),
                GenerateTypeSerializer<ushort>(Serialize),
                GenerateTypeSerializer<char>(Serialize),
#if NET5_0_OR_GREATER
                GenerateTypeSerializer<Half>(Serialize),
#endif

                GenerateTypeSerializer<int>(Serialize),
                GenerateTypeSerializer<uint>(Serialize),
                GenerateTypeSerializer<float>(Serialize),

                GenerateTypeSerializer<double>(Serialize),
                GenerateTypeSerializer<long>(Serialize),
                GenerateTypeSerializer<ulong>(Serialize),

                GenerateTypeSerializer<string>(Serialize),

                GenerateTypeSerializer<ReadableFileFormat.Value>(Serialize),
            }).ToDictionary();
        }

        /// <exception cref="NotImplementedException"></exception>
        TypeSerializer<T> GetSerializerForType<T>()
        {
            if (!typeSerializers.TryGetValue(typeof(T), out Delegate method))
            { throw new NotImplementedException($"Serializer for type {typeof(T)} not found"); }
            return (TypeSerializer<T>)method;
        }

        public byte[] Reinitialize()
        {
            byte[] result = this.result.ToArray();
            this.result.Clear();
            return result;
        }

        #region Generic Types

        // --- 1 byte ---

        /// <summary>
        /// Serializes the given <see cref="byte"/> value
        /// </summary>
        public void Serialize(byte v) => result.Add(v);

        /// <summary>
        /// Serializes the given <see cref="bool"/> value (1 byte)
        /// </summary>
        public void Serialize(bool v) => result.Add(v ? (byte)1 : (byte)0);

        // --- 2 bytes ---

        /// <summary>
        /// Serializes the given <see cref="short"/> value (2 bytes)
        /// </summary>
        public void Serialize(short v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }

        /// <summary>
        /// Serializes the given <see cref="ushort"/> value (2 bytes)
        /// </summary>
        public void Serialize(ushort v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }

        /// <summary>
        /// Serializes the given <see cref="char"/> value (2 bytes)
        /// </summary>
        public void Serialize(char v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }

#if NET5_0_OR_GREATER
        /// <summary>
        /// Serializes the given <see cref="Half"/> value (2 bytes)
        /// </summary>
        public void Serialize(Half v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }
#endif

        // --- 4 bytes ---

        /// <summary>
        /// Serializes the given <see cref="int"/> value (4 bytes)
        /// </summary>
        public void Serialize(int v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }

        /// <summary>
        /// Serializes the given <see cref="uint"/> value (4 bytes)
        /// </summary>
        public void Serialize(uint v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }

        /// <summary>
        /// Serializes the given <see cref="float"/> value (4 bytes)
        /// </summary>
        public void Serialize(float v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }

        // --- 8 bytes ---

        /// <summary>
        /// Serializes the given <see cref="long"/> value (8 bytes)
        /// </summary>
        public void Serialize(long v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }

        /// <summary>
        /// Serializes the given <see cref="ulong"/> value (8 bytes)
        /// </summary>
        public void Serialize(ulong v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }

        /// <summary>
        /// Serializes the given <see cref="double"/> value (8 bytes)
        /// </summary>
        public void Serialize(double v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }

        // --- string ---

        /// <summary>
        /// Serializes the given <see cref="string"/>. Both the length and the encoding will be serialized.
        /// </summary>
        public void Serialize(string v) => Serialize(v, INTEGER_TYPE.INT32);
        /// <summary>
        /// Serializes the given <see cref="string"/>. Both the length and the encoding will be serialized.
        /// </summary>
        public void Serialize(string v, INTEGER_TYPE length)
        {
            if (v == null)
            {
                Serialize(-1);
                return;
            }
            SerializeArrayLength(length, v.Length);
            bool isUnicode = false;
            for (int i = 0; i < v.Length; i++)
            {
                if ((ushort)v[i] > byte.MaxValue)
                {
                    isUnicode = true;
                    break;
                }
            }
            Serialize(isUnicode);
            if (isUnicode)
            {
                for (int i = 0; i < v.Length; i++)
                { Serialize(v[i]); }
            }
            else
            {
                for (int i = 0; i < v.Length; i++)
                { Serialize((byte)(ushort)v[i]); }
            }
        }

#endregion

        #region User-Defined Types

        /// <summary>
        /// Serializes the given object <typeparamref name="T"/> with the <see cref="ISerializable{T}.Serialize(Serializer)"/> method.
        /// </summary>
        public void Serialize<T>(ISerializable<T> v) where T : ISerializable<T> => v.Serialize(this);

        /// <summary>
        /// Serializes the given object <typeparamref name="T"/> with the <paramref name="callback"/> function.
        /// </summary>
        public void Serialize<T>(T v, Action<Serializer, T> callback) => callback.Invoke(this, v);

        #endregion

        #region Arrays

        /// <summary>
        /// Serializes the given array of <typeparamref name="T"/> with the <see cref="Serialize{T}(ISerializable{T})"/> method. The length of the array will also be serialized.
        /// </summary>
        public void Serialize<T>(ISerializable<T>[] v, INTEGER_TYPE length = INTEGER_TYPE.INT32) where T : ISerializable<T>
        {
            SerializeArrayLength(length, v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i]); }
        }

        /// <summary>
        /// Serializes the given array of <typeparamref name="T"/> with the <paramref name="callback"/> function. The length of the array will also be serialized.
        /// </summary>
        public void Serialize<T>(T[] v, Action<Serializer, T> callback, INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            SerializeArrayLength(length, v.Length);
            for (int i = 0; i < v.Length; i++)
            { callback.Invoke(this, v[i]); }
        }

        /// <exception cref="TooSmallUnitException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        /// <typeparam name="T">
        /// This must be one of the following:<br/>
        /// <see cref="byte"/>,
        /// <see cref="bool"/>,
        /// <see cref="short"/>,
        /// <see cref="ushort"/>,
        /// <see cref="char"/>,
        /// <see cref="Half"/>,
        /// <see cref="int"/>,
        /// <see cref="uint"/>,
        /// <see cref="float"/>,
        /// <see cref="double"/>,
        /// <see cref="long"/>,
        /// <see cref="ulong"/>,
        /// <see cref="string"/>,
        /// <see cref="ReadableFileFormat.Value"/>
        /// </typeparam>
        public void Serialize<T>(T[] v, INTEGER_TYPE length = INTEGER_TYPE.INT32) where T : struct
        {
            TypeSerializer<T> method = GetSerializerForType<T>();
            SerializeArrayLength(length, v.Length);
            for (int i = 0; i < v.Length; i++)
            { method.Invoke(v[i]); }
        }

        /// <exception cref="TooSmallUnitException"></exception>
        public void Serialize(string[] v, INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            SerializeArrayLength(length, v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i]); }
        }

        /// <exception cref="TooSmallUnitException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        /// <typeparam name="T">
        /// This must be one of the following:<br/>
        /// <see cref="byte"/>,
        /// <see cref="bool"/>,
        /// <see cref="short"/>,
        /// <see cref="ushort"/>,
        /// <see cref="char"/>,
        /// <see cref="Half"/>,
        /// <see cref="int"/>,
        /// <see cref="uint"/>,
        /// <see cref="float"/>,
        /// <see cref="double"/>,
        /// <see cref="long"/>,
        /// <see cref="ulong"/>,
        /// <see cref="string"/>,
        /// <see cref="ReadableFileFormat.Value"/>
        /// </typeparam>
        public void Serialize<T>(T[][] v, INTEGER_TYPE length = INTEGER_TYPE.INT32) where T : struct
        {
            SerializeArrayLength(length, v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i], length); }
        }

        /// <exception cref="TooSmallUnitException"></exception>
        public void Serialize(string[][] v, INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            SerializeArrayLength(length, v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i], length); }
        }

        /// <exception cref="TooSmallUnitException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        /// <typeparam name="T">
        /// This must be one of the following:<br/>
        /// <see cref="byte"/>,
        /// <see cref="bool"/>,
        /// <see cref="short"/>,
        /// <see cref="ushort"/>,
        /// <see cref="char"/>,
        /// <see cref="Half"/>,
        /// <see cref="int"/>,
        /// <see cref="uint"/>,
        /// <see cref="float"/>,
        /// <see cref="double"/>,
        /// <see cref="long"/>,
        /// <see cref="ulong"/>,
        /// <see cref="string"/>,
        /// <see cref="ReadableFileFormat.Value"/>
        /// </typeparam>
        public void Serialize<T>(T[][][] v, INTEGER_TYPE length = INTEGER_TYPE.INT32) where T : struct
        {
            SerializeArrayLength(length, v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i], length); }
        }

        /// <exception cref="TooSmallUnitException"></exception>
        public void Serialize(string[][][] v, INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            SerializeArrayLength(length, v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i], length); }
        }

        /// <exception cref="TooSmallUnitException"></exception>
        void SerializeArrayLength(INTEGER_TYPE type, int length)
        {
            switch (type)
            {
                case INTEGER_TYPE.INT8:
                    if (length < byte.MinValue || length > byte.MaxValue) throw new TooSmallUnitException($"The specified array length unit {type} is too small for the value {length}");
                    Serialize((byte)length);
                    break;
                case INTEGER_TYPE.INT16:
                    if (length < short.MinValue || length > short.MaxValue) throw new TooSmallUnitException($"The specified array length unit {type} is too small for the value {length}");
                    Serialize((short)length);
                    break;
                case INTEGER_TYPE.INT32:
                default:
                    Serialize(length);
                    break;
            }
        }

        #endregion

        #region Enumerables

        /// <summary>
        /// Serializes the given array of <typeparamref name="T"/> with the <see cref="Serialize{T}(ISerializable{T})"/> method. The length of the array will also be serialized.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Serialize<T>(IEnumerable<ISerializable<T>> v) where T : ISerializable<T>
        {
            TypeSerializer<T> method = GetSerializerForType<T>();
            Serialize(v, (s, item) =>
            {
                item.Serialize(s);
            });
        }

        /// <summary>
        /// Serializes the given array of <typeparamref name="T"/> with the <paramref name="callback"/> function. The length of the array will also be serialized.
        /// </summary>
        /// <typeparam name="T">
        /// This must be one of the following:<br/>
        /// <see cref="byte"/>,
        /// <see cref="bool"/>,
        /// <see cref="short"/>,
        /// <see cref="ushort"/>,
        /// <see cref="char"/>,
        /// <see cref="Half"/>,
        /// <see cref="int"/>,
        /// <see cref="uint"/>,
        /// <see cref="float"/>,
        /// <see cref="double"/>,
        /// <see cref="long"/>,
        /// <see cref="ulong"/>,
        /// <see cref="string"/>,
        /// <see cref="ReadableFileFormat.Value"/>
        /// </typeparam>
        /// <exception cref="NotImplementedException"></exception>
        public void Serialize<T>(IEnumerable<T> v, Action<Serializer, T> callback)
        {
            int length = 0;
            int lengthIndex = this.result.Count;
            Serialize(length);

            foreach (T item in v)
            {
                callback.Invoke(this, item);
                length++;
            }

            var lengthBytes = BitConverter.GetBytes(length);
            if (BitConverter.IsLittleEndian) Array.Reverse(lengthBytes);
            this.result[lengthIndex + 0] = lengthBytes[0];
            this.result[lengthIndex + 1] = lengthBytes[1];
            this.result[lengthIndex + 2] = lengthBytes[2];
            this.result[lengthIndex + 3] = lengthBytes[3];
        }

        /// <exception cref="NotImplementedException"></exception>
        public void Serialize<T>(IEnumerable<T> v) where T : struct
        {
            TypeSerializer<T> method = GetSerializerForType<T>();
            Serialize(v, (s, item) =>
            {
                method.Invoke(item);
            });
        }

        #endregion

        #region Dictionaries

        /// <typeparam name="TKey">
        /// This must be one of the following:<br/>
        /// <see cref="byte"/>,
        /// <see cref="bool"/>,
        /// <see cref="short"/>,
        /// <see cref="ushort"/>,
        /// <see cref="char"/>,
        /// <see cref="Half"/>,
        /// <see cref="int"/>,
        /// <see cref="uint"/>,
        /// <see cref="float"/>,
        /// <see cref="double"/>,
        /// <see cref="long"/>,
        /// <see cref="ulong"/>,
        /// <see cref="string"/>,
        /// <see cref="ReadableFileFormat.Value"/>
        /// </typeparam>
        /// <typeparam name="TValue">
        /// This must be one of the following:<br/>
        /// <see cref="byte"/>,
        /// <see cref="bool"/>,
        /// <see cref="short"/>,
        /// <see cref="ushort"/>,
        /// <see cref="char"/>,
        /// <see cref="Half"/>,
        /// <see cref="int"/>,
        /// <see cref="uint"/>,
        /// <see cref="float"/>,
        /// <see cref="double"/>,
        /// <see cref="long"/>,
        /// <see cref="ulong"/>,
        /// <see cref="string"/>,
        /// <see cref="ReadableFileFormat.Value"/>
        /// </typeparam>
        /// <exception cref="TooSmallUnitException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public void Serialize<TKey, TValue>(Dictionary<TKey, TValue> v, INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            if (v.Count == 0) { Serialize(-1); return; }

            TypeSerializer<TKey> keySerializer = GetSerializerForType<TKey>();
            TypeSerializer<TValue> valueSerializer = GetSerializerForType<TValue>();

            SerializeArrayLength(length, v.Count);

            foreach (var pair in v)
            {
                keySerializer.Invoke(pair.Key);
                valueSerializer.Invoke(pair.Value);
            }
        }

        /// <typeparam name="TKey">
        /// This must be one of the following:<br/>
        /// <see cref="byte"/>,
        /// <see cref="bool"/>,
        /// <see cref="short"/>,
        /// <see cref="ushort"/>,
        /// <see cref="char"/>,
        /// <see cref="Half"/>,
        /// <see cref="int"/>,
        /// <see cref="uint"/>,
        /// <see cref="float"/>,
        /// <see cref="double"/>,
        /// <see cref="long"/>,
        /// <see cref="ulong"/>,
        /// <see cref="string"/>,
        /// <see cref="ReadableFileFormat.Value"/>
        /// </typeparam>
        /// <exception cref="TooSmallUnitException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public void Serialize<TKey, TValue>(Dictionary<TKey, TValue> v, Action<Serializer, TValue> valueSerializer, INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            if (v.Count == 0) { Serialize(-1); return; }

            TypeSerializer<TKey> keySerializer = GetSerializerForType<TKey>();

            SerializeArrayLength(length, v.Count);

            foreach (var pair in v)
            {
                keySerializer.Invoke(pair.Key);
                valueSerializer.Invoke(this, pair.Value);
            }
        }

        /// <typeparam name="TValue">
        /// This must be one of the following:<br/>
        /// <see cref="byte"/>,
        /// <see cref="bool"/>,
        /// <see cref="short"/>,
        /// <see cref="ushort"/>,
        /// <see cref="char"/>,
        /// <see cref="Half"/>,
        /// <see cref="int"/>,
        /// <see cref="uint"/>,
        /// <see cref="float"/>,
        /// <see cref="double"/>,
        /// <see cref="long"/>,
        /// <see cref="ulong"/>,
        /// <see cref="string"/>,
        /// <see cref="ReadableFileFormat.Value"/>
        /// </typeparam>
        /// <exception cref="TooSmallUnitException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public void Serialize<TKey, TValue>(Dictionary<TKey, TValue> v, Action<Serializer, TKey> keySerializer, INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            if (v.Count == 0) { Serialize(-1); return; }

            TypeSerializer<TValue> valueSerializer = GetSerializerForType<TValue>();

            SerializeArrayLength(length, v.Count);

            foreach (var pair in v)
            {
                keySerializer.Invoke(this, pair.Key);
                valueSerializer.Invoke(pair.Value);
            }
        }

        /// <exception cref="TooSmallUnitException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public void Serialize<TKey, TValue>(Dictionary<TKey, TValue> v, Action<Serializer, TKey> keySerializer, Action<Serializer, TValue> valueSerializer, INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            if (v.Count == 0) { Serialize(-1); return; }

            SerializeArrayLength(length, v.Count);

            foreach (var pair in v)
            {
                keySerializer.Invoke(this, pair.Key);
                valueSerializer.Invoke(this, pair.Value);
            }
        }

        #endregion

        #region ReadableFileFormat.Value

        /// <exception cref="TooSmallUnitException"></exception>
        public void Serialize(ReadableFileFormat.Value v)
        {
            Serialize((byte)v.Type);
            switch (v.Type)
            {
                case ReadableFileFormat.ValueType.LITERAL:
                    Serialize(v.String);
                    break;
                case ReadableFileFormat.ValueType.OBJECT:
                    Serialize(v.Dictionary(), (s, key) => s.Serialize(key, INTEGER_TYPE.INT16), INTEGER_TYPE.INT32);
                    break;
                default:
                    break;
            }
        }

        #endregion

    }
}
