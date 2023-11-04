using System;
using System.Collections.Generic;

#nullable enable

namespace DataUtilities.Serializer
{
    public static class SerializerStatic
    {

        #region Generic Types

        // --- 1 byte ---

        /// <summary>
        /// Serializes the given <see cref="bool"/> value (1 byte)
        /// </summary>
        public static byte Serialize(bool v) => v ? (byte)1 : (byte)0;

        // --- 2 bytes ---

        /// <summary>
        /// Serializes the given <see cref="short"/> value (2 bytes)
        /// </summary>
        public static byte[] Serialize(short v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Serializes the given <see cref="ushort"/> value (2 bytes)
        /// </summary>
        public static byte[] Serialize(ushort v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Serializes the given <see cref="char"/> value (2 bytes)
        /// </summary>
        public static byte[] Serialize(char v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            return result;
        }

#if NET5_0_OR_GREATER
        /// <summary>
        /// Serializes the given <see cref="Half"/> value (2 bytes)
        /// </summary>
        public static byte[] Serialize(Half v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            return result;
        }
#endif

        // --- 4 bytes ---

        /// <summary>
        /// Serializes the given <see cref="int"/> value (4 bytes)
        /// </summary>
        public static byte[] Serialize(int v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Serializes the given <see cref="uint"/> value (4 bytes)
        /// </summary>
        public static byte[] Serialize(uint v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Serializes the given <see cref="float"/> value (4 bytes)
        /// </summary>
        public static byte[] Serialize(float v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            return result;
        }

        // --- 8 bytes ---

        /// <summary>
        /// Serializes the given <see cref="long"/> value (8 bytes)
        /// </summary>
        public static byte[] Serialize(long v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Serializes the given <see cref="ulong"/> value (8 bytes)
        /// </summary>
        public static byte[] Serialize(ulong v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Serializes the given <see cref="double"/> value (8 bytes)
        /// </summary>
        public static byte[] Serialize(double v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            return result;
        }

        // --- string ---

        /// <summary>
        /// Serializes the given <see cref="string"/>. Both the length and the encoding will be serialized.
        /// </summary>
        public static byte[] Serialize(string v) => Serialize(v, INTEGER_TYPE.INT32);
        /// <summary>
        /// Serializes the given <see cref="string"/>. Both the length and the encoding will be serialized.
        /// </summary>
        public static byte[] Serialize(string v, INTEGER_TYPE length)
        {
            if (v == null)
            {
                return Serialize(-1);
            }
            Serializer s = new();
            s.Serialize(v, length);
            return s.Result;
        }

#endregion

        #region User-Defined Types

        /// <summary>
        /// Serializes the given object <typeparamref name="T"/> with the <see cref="ISerializable{T}.Serialize(Serializer)"/> method.
        /// </summary>
        public static byte[] Serialize<T>(ISerializable<T> v) where T : ISerializable<T>
        {
            Serializer s = new();
            v.Serialize(s);
            return s.Result;
        }

        /// <summary>
        /// Serializes the given object <typeparamref name="T"/> with the <paramref name="callback"/> function.
        /// </summary>
        public static byte[] Serialize<T>(T v, Action<Serializer, T> callback)
        {
            Serializer s = new();
            callback.Invoke(s, v);
            return s.Result;
        }

        #endregion

        #region Arrays

        /// <summary>
        /// Serializes the given array of <typeparamref name="T"/> with the <see cref="Serializer.Serialize{T}(ISerializable{T})"/> method. The length of the array will also be serialized.
        /// </summary>
        public static byte[] Serialize<T>(ISerializable<T>[] v, INTEGER_TYPE length = INTEGER_TYPE.INT32) where T : ISerializable<T>
        {
            Serializer s = new();
            s.Serialize<T>(v, length);
            return s.Result;
        }

        /// <summary>
        /// Serializes the given array of <typeparamref name="T"/> with the <paramref name="callback"/> function. The length of the array will also be serialized.
        /// </summary>
        public static byte[] Serialize<T>(T[] v, Action<Serializer, T> callback)
        {
            Serializer s = new();
            s.Serialize<T>(v, callback);
            return s.Result;
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
        public static byte[] Serialize<T>(T[] v, INTEGER_TYPE length = INTEGER_TYPE.INT32) where T : struct
        {
            Serializer s = new();
            s.Serialize<T>(v, length);
            return s.Result;
        }

        /// <exception cref="TooSmallUnitException"></exception>
        public static byte[] Serialize(string[] v, INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            Serializer s = new();
            s.Serialize(v, length);
            return s.Result;
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
        public static byte[] Serialize<T>(T[][] v, INTEGER_TYPE length = INTEGER_TYPE.INT32) where T : struct
        {
            Serializer s = new();
            s.Serialize<T>(v, length);
            return s.Result;
        }

        /// <exception cref="TooSmallUnitException"></exception>
        public static byte[] Serialize(string[][] v, INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            Serializer s = new();
            s.Serialize(v, length);
            return s.Result;
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
        public static byte[] Serialize<T>(T[][][] v, INTEGER_TYPE length = INTEGER_TYPE.INT32) where T : struct
        {
            Serializer s = new();
            s.Serialize<T>(v, length);
            return s.Result;
        }

        /// <exception cref="TooSmallUnitException"></exception>
        public static byte[] Serialize(string[][][] v, INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            Serializer s = new();
            s.Serialize(v, length);
            return s.Result;
        }

        #endregion

        #region Enumerables

        /// <summary>
        /// Serializes the given array of <typeparamref name="T"/> with the <see cref="Serializer.Serialize{T}(ISerializable{T})"/> method. The length of the array will also be serialized.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public static byte[] Serialize<T>(IEnumerable<ISerializable<T>> v) where T : ISerializable<T>
        {
            Serializer s = new();
            s.Serialize<T>(v);
            return s.Result;
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
        public static byte[] Serialize<T>(IEnumerable<T> v, Action<Serializer, T> callback)
        {
            Serializer s = new();
            s.Serialize<T>(v, callback);
            return s.Result;
        }

        /// <exception cref="NotImplementedException"></exception>
        public static byte[] Serialize<T>(IEnumerable<T> v) where T : struct
        {
            Serializer s = new();
            s.Serialize<T>(v);
            return s.Result;
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
        public static byte[] Serialize<TKey, TValue>(Dictionary<TKey, TValue> v, INTEGER_TYPE length = INTEGER_TYPE.INT32) where TKey : notnull
        {
            if (v.Count == 0) { return Serialize(-1); }

            Serializer s = new();
            s.Serialize<TKey, TValue>(v, length);
            return s.Result;
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
        public static byte[] Serialize<TKey, TValue>(Dictionary<TKey, TValue> v, Action<Serializer, TValue> valueSerializer, INTEGER_TYPE length = INTEGER_TYPE.INT32) where TKey : notnull
        {
            if (v.Count == 0) { return Serialize(-1); }

            Serializer s = new();
            s.Serialize<TKey, TValue>(v, valueSerializer, length);
            return s.Result;
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
        public static byte[] Serialize<TKey, TValue>(Dictionary<TKey, TValue> v, Action<Serializer, TKey> keySerializer, INTEGER_TYPE length = INTEGER_TYPE.INT32) where TKey : notnull
        {
            if (v.Count == 0) { return Serialize(-1); }

            Serializer s = new();
            s.Serialize<TKey, TValue>(v, keySerializer, length);
            return s.Result;
        }

        /// <exception cref="TooSmallUnitException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public static byte[] Serialize<TKey, TValue>(Dictionary<TKey, TValue> v, Action<Serializer, TKey> keySerializer, Action<Serializer, TValue> valueSerializer, INTEGER_TYPE length = INTEGER_TYPE.INT32) where TKey : notnull
        {
            if (v.Count == 0) { return Serialize(-1); }

            Serializer s = new();
            s.Serialize<TKey, TValue>(v, keySerializer, valueSerializer, length);
            return s.Result;
        }

        #endregion

        #region ReadableFileFormat.Value

        /// <exception cref="TooSmallUnitException"></exception>
        public static byte[] Serialize(ReadableFileFormat.Value v)
        {
            Serializer s = new();
            s.Serialize(v);
            return s.Result;
        }

        #endregion

    }
}
