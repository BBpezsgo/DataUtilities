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
        readonly byte[] data = Array.Empty<byte>();
        int currentIndex;

        readonly Dictionary<Type, Delegate> typeDeserializers;

        delegate T TypeDeserializer<T>();
        static KeyValuePair<Type, Delegate> GenerateTypeDeserializer<T>(TypeDeserializer<T> typeSerializer) => new(typeof(T), typeSerializer);

        /// <param name="data">The raw binary data you want to deserialize</param>
        public Deserializer(byte[] data)
        {
            this.data = data;
            this.currentIndex = 0;

            typeDeserializers = (new KeyValuePair<Type, Delegate>[]
            {
                GenerateTypeDeserializer(DeserializeInt32),
                GenerateTypeDeserializer(DeserializeFloat),
                GenerateTypeDeserializer(DeserializeBoolean),
                GenerateTypeDeserializer(DeserializeByte),
                GenerateTypeDeserializer(DeserializeChar),
                GenerateTypeDeserializer(DeserializeString),
                GenerateTypeDeserializer(DeserializeDouble),
                GenerateTypeDeserializer(DeserializeSdfValue),
                GenerateTypeDeserializer(DeserializeInt16),
            }).ToDictionary();
        }

        Func<T> GetDeserializerForType<T>()
        {
            if (!typeDeserializers.TryGetValue(typeof(T), out Delegate method))
            { throw new NotImplementedException($"Deserializer for type {typeof(T)} not found"); }
            return (Func<T>)method;
        }

        int DeserializeArrayLength(INTEGER_TYPE length)
        {
            switch (length)
            {
                case INTEGER_TYPE.INT8:
                    return (int)DeserializeByte();
                case INTEGER_TYPE.INT16:
                    return (int)DeserializeInt16();
                case INTEGER_TYPE.INT32:
                default:
                    return (int)DeserializeInt32();
            }
        }

        public T[] DeserializeArray<T>(INTEGER_TYPE length = INTEGER_TYPE.INT32)
        {
            int _length = DeserializeArrayLength(length);
            T[] result = new T[_length];
            for (int i = 0; i < _length; i++)
            {
                result[i] = (T)Deserialize<T>();
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
        public T[] DeserializeObjectArray<T>() where T : ISerializable<T>
        {
            int length = DeserializeInt32();
            T[] result = new T[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (T)DeserializeObject<T>();
            }
            return result;
        }
        public T[] DeserializeObjectArray<T>(Func<Deserializer, T> callback)
        {
            int length = DeserializeInt32();
            T[] result = new T[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = callback.Invoke(this);
            }
            return result;
        }

        /// <summary>
        /// Deserializes the following data
        /// </summary>
        /// <typeparam name="T">
        /// The following data type.
        /// </typeparam>
        /// <returns>The deserialized data whose type is <typeparamref name="T"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public T Deserialize<T>() => GetDeserializerForType<T>().Invoke();

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
        /// Returns the next byte
        /// </summary>
        public byte DeserializeByte()
        {
            var data = this.data.Get(currentIndex, 1);
            currentIndex += 1;
            return data[0];
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
        /// Deserializes the following <see cref="System.Single"/> data (4 bytes)
        /// </summary>
        public float DeserializeFloat()
        {
            var data = this.data.Get(currentIndex, 4);
            currentIndex += 4;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToSingle(data, 0);
        }
        /// <summary>
        /// Deserializes the following <see cref="System.Single"/> data (4 bytes)
        /// </summary>
        public double DeserializeDouble()
        {
            var data = this.data.Get(currentIndex, 8);
            currentIndex += 4;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToDouble(data, 0);
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
        /// <summary>
        /// Deserializes the following <see cref="System.String"/> data. Length and encoding are obtained automatically.
        /// </summary>
        public string DeserializeString()
        {
            int length = DeserializeInt32();
            if (length == -1) return null;
            byte type = DeserializeByte();
            if (length == 0) return string.Empty;
            char[] result = new char[length];
            switch (type)
            {
                case 0:
                    for (int i = 0; i < length; i++)
                    {
                        result[i] = (char)DeserializeByte();
                    }
                    return new string(result);

                case 1:
                    for (int i = 0; i < length; i++)
                    {
                        result[i] = DeserializeChar();
                    }
                    return new string(result);

                default: throw new Exception($"Unknown encoding index {type}");
            }
        }
        public ReadableFileFormat.Value DeserializeSdfValue()
        {
            ReadableFileFormat.ValueType type = (ReadableFileFormat.ValueType)DeserializeInt32();
            return type switch
            {
                ReadableFileFormat.ValueType.LITERAL => ReadableFileFormat.Value.Literal(DeserializeString()),
                ReadableFileFormat.ValueType.OBJECT => ReadableFileFormat.Value.Object(DeserializeDictionary<string, ReadableFileFormat.Value>()),
                _ => throw new Exception("WTF"),
            };
        }
        /// <summary>
        /// Deserializes the following <typeparamref name="T"/> data.<br/>
        /// This creates an instance of <typeparamref name="T"/> and then calls the <see cref="ISerializable.Deserialize(Deserializer)"/> method on the instance.
        /// </summary>
        public ISerializable<T> DeserializeObject<T>() where T : ISerializable<T>
        {
            var instance = (ISerializable<T>)Activator.CreateInstance(typeof(T));
            instance.Deserialize(this);
            return instance;
        }
        public T DeserializeObject<T>(Func<Deserializer, T> callback)
        {
            return callback.Invoke(this);
        }
        public T[] DeserializeArray<T>(Func<Deserializer, T> callback)
        {
            int length = DeserializeInt32();
            T[] result = new T[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = callback.Invoke(this);
            }
            return result;
        }
        public Dictionary<TKey, TValue> DeserializeDictionary<TKey, TValue>()
        {
            int length = DeserializeInt32();
            if (length == -1) return null;
            Dictionary<TKey, TValue> result = new();

            for (int i = 0; i < length; i++)
            {
                TKey key = Deserialize<TKey>();
                TValue value = Deserialize<TValue>();
                result.Add(key, value);
            }

            return result;
        }
    }
}
