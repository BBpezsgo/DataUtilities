using System;
using System.Collections.Generic;

namespace DataUtilities.Serializer
{
    /// <summary>
    /// This class handles deserialization of a raw binary data array.
    /// To start the process, create an instance with a byte array parameter that you want to deserialize.
    /// You can then call the instance methods like <see cref="DeserializeInt32"/> or <see cref="DeserializeString"/>.
    /// </summary>
    public class Deserializer
    {
        byte[] data = Array.Empty<byte>();
        int currentIndex;

        readonly Dictionary<Type, Delegate> typeDeserializers;

        delegate T TypeDeserializer<T>();
        static KeyValuePair<Type, Delegate> GenerateTypeDeserializer<T>(TypeDeserializer<T> typeSerializer) => new(typeof(T), typeSerializer);

        /// <param name="data">The raw binary data you want to deserialize</param>
        public Deserializer(byte[] data)
        {
            typeDeserializers = (new KeyValuePair<Type, Delegate>[]
            {
                GenerateTypeDeserializer(DeserializeByte),
                GenerateTypeDeserializer(DeserializeBoolean),

                GenerateTypeDeserializer(DeserializeInt16),
                GenerateTypeDeserializer(DeserializeUInt16),
                GenerateTypeDeserializer(DeserializeChar),
#if NET5_0_OR_GREATER
                GenerateTypeDeserializer(DeserializeHalf),
#endif

                GenerateTypeDeserializer(DeserializeInt32),
                GenerateTypeDeserializer(DeserializeUInt32),
                GenerateTypeDeserializer(DeserializeFloat),

                GenerateTypeDeserializer(DeserializeDouble),
                GenerateTypeDeserializer(DeserializeInt64),
                GenerateTypeDeserializer(DeserializeUInt64),

                GenerateTypeDeserializer(DeserializeString),

                GenerateTypeDeserializer(DeserializeSdfValue),
            }).ToDictionary();

            Reinitialize(data);
        }

        public void Reinitialize(byte[] data)
        {
            this.data = data;
            this.currentIndex = 0;
        }

        Func<T> GetDeserializerForType<T>()
        {
            if (!typeDeserializers.TryGetValue(typeof(T), out Delegate method))
            { throw new NotImplementedException($"Deserializer for type {typeof(T)} not found"); }
            return ((TypeDeserializer<T>)method).Invoke;
        }

        bool TryGetDeserializerForType<T>(out Func<T> deserializer)
        {
            if (!typeDeserializers.TryGetValue(typeof(T), out Delegate method))
            {
                deserializer = null;
                return false;
            }

            deserializer = ((TypeDeserializer<T>)method).Invoke;
            return true;
        }

        #region Generic Types

        // --- 1 byte ---

        /// <summary>
        /// Returns the next byte
        /// </summary>
        public byte DeserializeByte()
        {
            var data = this.data.Get(currentIndex, 1);
            currentIndex += 1;
            return data[0];
        }

        /// <summary>
        /// Deserializes the following <see cref="System.Boolean"/> data (1 bytes)
        /// </summary>
        public bool DeserializeBoolean()
        {
            var data = this.data.Get(currentIndex, 1);
            currentIndex++;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToBoolean(data, 0);
        }

        // --- 2 bytes ---

        /// <summary>
        /// Deserializes the following <see cref="System.Int16"/> data (2 bytes)
        /// </summary>
        public short DeserializeInt16()
        {
            var data = this.data.Get(currentIndex, 2);
            currentIndex += 2;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        /// <summary>
        /// Deserializes the following <see cref="System.UInt16"/> data (2 bytes)
        /// </summary>
        public ushort DeserializeUInt16()
        {
            var data = this.data.Get(currentIndex, 2);
            currentIndex += 2;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }

        /// <summary>
        /// Deserializes the following <see cref="System.Char"/> data (2 bytes)
        /// </summary>
        public char DeserializeChar()
        {
            var data = this.data.Get(currentIndex, 2);
            currentIndex += 2;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToChar(data, 0);
        }

#if NET5_0_OR_GREATER
        /// <summary>
        /// Deserializes the following <see cref="Half"/> data (2 bytes)
        /// </summary>
        public Half DeserializeHalf()
        {
            var data = this.data.Get(currentIndex, 2);
            currentIndex += 2;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToHalf(data, 0);
        }
#endif

        // --- 4 bytes ---

        /// <summary>
        /// Deserializes the following <see cref="System.Int32"/> data (4 bytes)
        /// </summary>
        public int DeserializeInt32()
        {
            var data = this.data.Get(currentIndex, 4);
            currentIndex += 4;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }

        /// <summary>
        /// Deserializes the following <see cref="System.UInt32"/> data (4 bytes)
        /// </summary>
        public uint DeserializeUInt32()
        {
            var data = this.data.Get(currentIndex, 4);
            currentIndex += 4;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }

        /// <summary>
        /// Deserializes the following <see cref="System.Single"/> data (4 bytes)
        /// </summary>
        public float DeserializeFloat()
        {
            var data = this.data.Get(currentIndex, 4);
            currentIndex += 4;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToSingle(data, 0);
        }

        // --- 8 bytes ---

        /// <summary>
        /// Deserializes the following <see cref="System.Single"/> data (8 bytes)
        /// </summary>
        public double DeserializeDouble()
        {
            var data = this.data.Get(currentIndex, 8);
            currentIndex += 4;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToDouble(data, 0);
        }

        /// <summary>
        /// Deserializes the following <see cref="System.Int64"/> data (8 bytes)
        /// </summary>
        public long DeserializeInt64()
        {
            var data = this.data.Get(currentIndex, 8);
            currentIndex += 8;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }

        /// <summary>
        /// Deserializes the following <see cref="System.UInt64"/> data (8 bytes)
        /// </summary>
        public ulong DeserializeUInt64()
        {
            var data = this.data.Get(currentIndex, 8);
            currentIndex += 8;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToUInt64(data, 0);
        }

        // --- string ---

        /// <summary>
        /// Deserializes the following <see cref="System.String"/> data. Length and encoding are obtained automatically.
        /// </summary>
        public string DeserializeString() => DeserializeString(INTEGER_TYPE.INT32);
        /// <summary>
        /// Deserializes the following <see cref="System.String"/> data. Length and encoding are obtained automatically.
        /// </summary>
        public string DeserializeString(INTEGER_TYPE length)
        {
            int _length = DeserializeArrayLength(length);
            if (_length == -1) return null;
            byte type = DeserializeByte();
            if (_length == 0) return string.Empty;
            char[] result = new char[_length];
            switch (type)
            {
                case 0:
                    for (int i = 0; i < _length; i++)
                    {
                        result[i] = (char)DeserializeByte();
                    }
                    return new string(result);

                case 1:
                    for (int i = 0; i < _length; i++)
                    {
                        result[i] = DeserializeChar();
                    }
                    return new string(result);

                default: throw new Exception($"Unknown encoding index {type}");
            }
        }

        #endregion

        #region User-Defined Types

        /// <summary>
        /// Deserializes the following <typeparamref name="T"/> data.<br/>
        /// This creates an instance of <typeparamref name="T"/> and then calls the <see cref="ISerializable{T}.Deserialize(Deserializer)"/> method on the instance.
        /// </summary>
        public T DeserializeObject<T>() where T : ISerializable<T>
        {
            var instance = (ISerializable<T>)Activator.CreateInstance(typeof(T));
            instance.Deserialize(this);
            return (T)instance;
        }

        public T DeserializeObject<T>(Func<Deserializer, T> callback)
        {
            return callback.Invoke(this);
        }

        #endregion

        #region Arrays

        public T[] DeserializeArray<T>(INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            int _length = DeserializeArrayLength(length);
            T[] result = new T[_length];
            for (int i = 0; i < _length; i++)
            {
                result[i] = Deserialize<T>();
            }
            return result;
        }

        public T[][] DeserializeArray2D<T>(INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            int _length = DeserializeArrayLength(length);
            T[][] result = new T[_length][];
            for (int i = 0; i < _length; i++)
            {
                result[i] = DeserializeArray<T>(length);
            }
            return result;
        }

        public T[][][] DeserializeArray3D<T>(INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            int _length = DeserializeArrayLength(length);
            T[][][] result = new T[_length][][];
            for (int i = 0; i < _length; i++)
            {
                result[i] = DeserializeArray2D<T>(length);
            }
            return result;
        }

        public T[] DeserializeObjectArray<T>(INTEGER_TYPE length = INTEGER_TYPE.INT32) where T : ISerializable<T>
        {
            int _length = DeserializeArrayLength(length);
            T[] result = new T[_length];
            for (int i = 0; i < _length; i++)
            {
                result[i] = (T)DeserializeObject<T>();
            }
            return result;
        }

        public T[] DeserializeArray<T>(Func<Deserializer, T> callback, INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            int _length = DeserializeArrayLength(length);
            T[] result = new T[_length];
            for (int i = 0; i < _length; i++)
            {
                result[i] = callback.Invoke(this);
            }
            return result;
        }

        int DeserializeArrayLength(INTEGER_TYPE length) => length switch
        {
            INTEGER_TYPE.INT8 => (int)DeserializeByte(),
            INTEGER_TYPE.INT16 => (int)DeserializeInt16(),
            _ => DeserializeInt32(),
        };

        #endregion

        #region Dictionaries

        public Dictionary<TKey, TValue> DeserializeDictionary<TKey, TValue>(INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            int _length = DeserializeArrayLength(length);
            if (_length == -1) return null;
            Dictionary<TKey, TValue> result = new();

            for (int i = 0; i < _length; i++)
            {
                TKey key = Deserialize<TKey>();
                TValue value = Deserialize<TValue>();
                result.Add(key, value);
            }

            return result;
        }

        public Dictionary<TKey, TValue> DeserializeDictionary<TKey, TValue>(Func<Deserializer, TKey> keyDeserializer, INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            int _length = DeserializeArrayLength(length);
            if (_length == -1) return null;
            Dictionary<TKey, TValue> result = new();

            for (int i = 0; i < _length; i++)
            {
                TKey key = keyDeserializer.Invoke(this);
                TValue value = Deserialize<TValue>();
                result.Add(key, value);
            }

            return result;
        }

        public Dictionary<TKey, TValue> DeserializeDictionary<TKey, TValue>(Func<Deserializer, TValue> valueDeserializer, INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            int _length = DeserializeArrayLength(length);
            if (_length == -1) return null;
            Dictionary<TKey, TValue> result = new();

            for (int i = 0; i < _length; i++)
            {
                TKey key = Deserialize<TKey>();
                TValue value = valueDeserializer.Invoke(this);
                result.Add(key, value);
            }

            return result;
        }

        public Dictionary<TKey, TValue> DeserializeDictionary<TKey, TValue>(Func<Deserializer, TKey> keyDeserializer, Func<Deserializer, TValue> valueDeserializer, INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            int _length = DeserializeArrayLength(length);
            if (_length == -1) return null;
            Dictionary<TKey, TValue> result = new();

            for (int i = 0; i < _length; i++)
            {
                TKey key = keyDeserializer.Invoke(this);
                TValue value = valueDeserializer.Invoke(this);
                result.Add(key, value);
            }

            return result;
        }

        #endregion

        #region ReadableFileFormat.Value

        public ReadableFileFormat.Value DeserializeSdfValue()
        {
            ReadableFileFormat.ValueType type = (ReadableFileFormat.ValueType)DeserializeByte();
            return type switch
            {
                ReadableFileFormat.ValueType.LITERAL => ReadableFileFormat.Value.Literal(DeserializeString()),
                ReadableFileFormat.ValueType.OBJECT => ReadableFileFormat.Value.Object(DeserializeDictionary<string, ReadableFileFormat.Value>(s => s.DeserializeString(INTEGER_TYPE.INT16))),
                _ => throw new Exception("WTF"),
            };
        }

        #endregion

        /// <summary>
        /// Deserializes the following data
        /// </summary>
        /// <typeparam name="T">
        /// The following data type.<br/>
        /// This must be one of the following:<br/>
        /// <see cref="System.Byte"/>,
        /// <see cref="System.Boolean"/>,
        /// <see cref="System.Int16"/>,
        /// <see cref="System.UInt16"/>,
        /// <see cref="System.Char"/>,
        /// <see cref="System.Int32"/>,
        /// <see cref="System.UInt32"/>,
        /// <see cref="System.Int64"/>,
        /// <see cref="System.UInt64"/>,
        /// 
        /// <see cref="System.Half"/>,
        /// <see cref="System.Single"/>,
        /// <see cref="System.Double"/>,
        /// 
        /// <see cref="System.String"/>,
        /// <see cref="ReadableFileFormat.Value"/>
        /// </typeparam>
        /// <returns>The deserialized data whose type is <typeparamref name="T"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public T Deserialize<T>() => GetDeserializerForType<T>().Invoke();
    }
}
