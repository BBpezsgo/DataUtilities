using System;
using System.Collections.Generic;

namespace DataUtilities.Serializer
{
    static class Extensions
    {
        /// <summary>
        /// Returns a specified section of the array
        /// </summary>
        /// <typeparam name="T">The type of the elements of the array</typeparam>
        /// <param name="array">The array</param>
        /// <param name="startIndex">The index where the section starts (inclusive)</param>
        /// <param name="length">The length of the section</param>
        internal static T[] Get<T>(this T[] array, int startIndex, int length)
        {
            List<T> result = new();
            for (int i = startIndex; i < length + startIndex; i++)
            { result.Add(array[i]); }
            return result.ToArray();
        }
    }

    /// <summary>
    /// This class handles deserialization of a raw binary data array.
    /// To start the process, create an instance with a byte array parameter that you want to deserialize.
    /// You can then call the instance methods like <see cref="DeserializeInt32"/> or <see cref="DeserializeString"/>.
    /// </summary>
    public class Deserializer
    {
        readonly byte[] data = Array.Empty<byte>();
        int currentIndex;

        /// <param name="data">The raw binary data you want to deserialize</param>
        public Deserializer(byte[] data)
        {
            this.data = data;
            currentIndex = 0;
        }

        public T[] DeserializeArray<T>()
        {
            int length = DeserializeInt32();
            T[] result = new T[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (T)Deserialize<T>();
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
        /// Possible types:
        /// <list type="bullet">
        /// <item><seealso cref="int"/></item>
        /// <item><seealso cref="short"/></item>
        /// <item><seealso cref="char"/></item>
        /// <item><seealso cref="string"/></item>
        /// <item><seealso cref="bool"/></item>
        /// <item><seealso cref="float"/></item>
        /// <item><seealso cref="byte"/></item>
        /// </list>
        /// </typeparam>
        /// <returns>The deserialized data whose type is <typeparamref name="T"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public object Deserialize<T>()
        {
            if (typeof(T) == typeof(int))
            { return DeserializeInt32(); }
            if (typeof(T) == typeof(short))
            { return DeserializeInt16(); }
            if (typeof(T) == typeof(char))
            { return DeserializeChar(); }
            if (typeof(T) == typeof(string))
            { return DeserializeString(); }
            if (typeof(T) == typeof(bool))
            { return DeserializeBoolean(); }
            if (typeof(T) == typeof(float))
            { return DeserializeFloat(); }
            if (typeof(T) == typeof(byte))
            { return DeserializeByte(); }

            throw new NotImplementedException();
        }

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
        ISerializable<T> DeserializeObjectUnsafe<T>()
        {
            var instance = (ISerializable<T>)Activator.CreateInstance(typeof(T));
            instance.Deserialize(this);
            return instance;
        }
        public T DeserializeObject<T>(Func<Deserializer, T> callback)
        {
            return callback.Invoke(this);
        }
        public Dictionary<TKey, TValue> DeserializeDictionary<TKey, TValue>(bool keyIsObj, bool valIsObj)
        {
            int length = DeserializeInt32();
            if (length == -1) return null;
            Dictionary<TKey, TValue> result = new();

            for (int i = 0; i < length; i++)
            {
                var key = keyIsObj ? (TKey)DeserializeObjectUnsafe<TKey>() : (TKey)Deserialize<TKey>();
                var value = valIsObj ? (TValue)DeserializeObjectUnsafe<TValue>() : (TValue)Deserialize<TValue>();
                result.Add(key, value);
            }

            return result;
        }
    }

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

        /// <summary>
        /// Serializes the given <see cref="int"/>
        /// </summary>
        public void Serialize(int v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }
        /// <summary>
        /// Serializes the given <see cref="float"/>
        /// </summary>
        public void Serialize(float v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }
        /// <summary>
        /// Serializes the given <see cref="bool"/>
        /// </summary>
        public void Serialize(bool v)
        {
            result.Add(BitConverter.GetBytes(v)[0]);
        }
        /// <summary>
        /// Serializes the given <see cref="byte"/>
        /// </summary>
        public void Serialize(byte v)
        {
            result.Add(v);
        }
        /// <summary>
        /// Serializes the given <see cref="short"/>
        /// </summary>
        public void Serialize(short v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }
        /// <summary>
        /// Serializes the given <see cref="char"/>
        /// </summary>
        public void Serialize(char v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }
        /// <summary>
        /// Serializes the given <see cref="string"/>. Both the length and the encoding will be serialized.
        /// </summary>
        public void Serialize(string v)
        {
            if (v == null)
            {
                Serialize(-1);
                return;
            }
            Serialize(v.Length);
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
        /// <summary>
        /// Serializes the given array of <see cref="short"/> with the <see cref="Serialize(short)"/> method. The length of the array will also be serialized.
        /// </summary>
        public void Serialize(short[] v)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i]); }
        }
        /// <summary>
        /// Serializes the given array of <see cref="int"/> with the <see cref="Serialize(int)"/> method. The length of the array will also be serialized.
        /// </summary>
        public void Serialize(int[] v)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i]); }
        }
        /// <summary>
        /// Serializes the given array of <see cref="string"/> with the <see cref="Serialize(string)"/> method. The length of the array will also be serialized.
        /// </summary>
        public void Serialize(string[] v)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i]); }
        }
        /// <summary>
        /// Serializes the given array of <see cref="char"/> with the <see cref="Serialize(char)"/> method. The length of the array will also be serialized.
        /// </summary>
        public void Serialize(char[] v)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i]); }
        }
        /// <summary>
        /// Serializes the given array of <typeparamref name="T"/> with the <see cref="SerializeObject{T}(ISerializable{T})"/> method. The length of the array will also be serialized.
        /// </summary>
        public void SerializeObjectArray<T>(ISerializable<T>[] v)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { SerializeObject(v[i]); }
        }
        /// <summary>
        /// Serializes the given array of <typeparamref name="T"/> with the <paramref name="callback"/> function. The length of the array will also be serialized.
        /// </summary>
        public void SerializeObjectArray<T>(T[] v, Action<Serializer, T> callback)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { callback.Invoke(this, v[i]); }
        }
        /// <summary>
        /// Serializes the given object <typeparamref name="T"/> with the <see cref="ISerializable{T}.Serialize(Serializer)"/> method.
        /// </summary>
        public void SerializeObject<T>(ISerializable<T> v) => v.Serialize(this);
        /// <summary>
        /// Serializes the given object <typeparamref name="T"/> with the <paramref name="callback"/> function.
        /// </summary>
        public void SerializeObject<T>(T v, Action<Serializer, T> callback) => callback.Invoke(this, v);
        /// <summary>
        /// Serializes the given value.
        /// Possible types:
        /// <list type="bullet">
        /// <item><seealso cref="int"/></item>
        /// <item><seealso cref="short"/></item>
        /// <item><seealso cref="char"/></item>
        /// <item><seealso cref="string"/></item>
        /// <item><seealso cref="bool"/></item>
        /// <item><seealso cref="float"/></item>
        /// <item><seealso cref="byte"/></item>
        /// </list>
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        void Serialize(object v)
        {
            if (v is short int16)
            { Serialize(int16); }
            else if (v is int int32)
            { Serialize(int32); }
            else if (v is char @char)
            { Serialize(@char); }
            else if (v is string @string)
            { Serialize(@string); }
            else if (v is float single)
            { Serialize(single); }
            else if (v is bool boolean)
            { Serialize(boolean); }
            else if (v is byte @byte)
            { Serialize(@byte); }
            else
            { throw new NotImplementedException(); }
        }
        public void Serialize<TKey, TValue>(Dictionary<TKey, TValue> v, bool keyIsObj, bool valIsObj)
            where TKey : struct, IConvertible
            where TValue : struct, IConvertible
        {
            if (v.Count == 0) { Serialize(-1); return; }
            Serialize(v.Count);

            foreach (var pair in v)
            {
                if (keyIsObj)
                {
                    SerializeObject((ISerializable<TKey>)pair.Key);
                }
                else
                {
                    Serialize(pair.Key);
                }
                if (valIsObj)
                {
                    SerializeObject((ISerializable<TValue>)pair.Value);
                }
                else
                {
                    Serialize(pair.Value);
                }
            }
        }
        public void Serialize<TKey>(Dictionary<TKey, string> v, bool keyIsObj)
            where TKey : struct, IConvertible
        {
            if (v.Count == 0) { Serialize(-1); return; }
            Serialize(v.Count);

            foreach (var pair in v)
            {
                if (keyIsObj)
                {
                    SerializeObject((ISerializable<TKey>)pair.Key);
                }
                else
                {
                    Serialize(pair.Key);
                }
                Serialize(pair.Value);
            }
        }
        public void Serialize<TValue>(Dictionary<string, TValue> v, bool valIsObj)
            where TValue : struct, IConvertible
        {
            if (v.Count == 0) { Serialize(-1); return; }
            Serialize(v.Count);

            foreach (var pair in v)
            {
                Serialize(pair.Key);
                if (valIsObj)
                {
                    SerializeObject((ISerializable<TValue>)pair.Value);
                }
                else
                {
                    Serialize(pair.Value);
                }
            }
        }
    }

    /// <summary>
    /// This interface is responsible for serializing and deserializing custom data types.
    /// </summary>
    /// <typeparam name="T">The type of the class that implements this interface.</typeparam>
    public interface ISerializable<T>
    {
        void Serialize(Serializer serializer);
        void Deserialize(Deserializer deserializer);
    }

#if false
    static class Output
    {
        static void Log(string message) { Console.WriteLine(message); }

        static void Print(int value) { Console.ForegroundColor = ConsoleColor.Green; Console.Write(value); Console.ResetColor(); }
        static void Print(short value) { Console.ForegroundColor = ConsoleColor.Green; Console.Write(value); Console.ResetColor(); }
        static void Print(byte value) { Console.ForegroundColor = ConsoleColor.Green; Console.Write(value); Console.ResetColor(); }
        static void Print(long value) { Console.ForegroundColor = ConsoleColor.Green; Console.Write(value); Console.ResetColor(); }
        static void Print(float value) { Console.ForegroundColor = ConsoleColor.Green; Console.Write(value); Console.ResetColor(); }
        static void Print(double value) { Console.ForegroundColor = ConsoleColor.Green; Console.Write(value); Console.ResetColor(); }

        static void Print(string value) { Console.ForegroundColor = ConsoleColor.Yellow; Console.Write($"\"{value}\""); Console.ResetColor(); }
        static void Print(char value) { Console.ForegroundColor = ConsoleColor.Yellow; Console.Write($"'{value}'"); Console.ResetColor(); }

        static void Print(bool value) { Console.ForegroundColor = ConsoleColor.Blue; Console.Write(value ? "true" : "false"); Console.ResetColor(); }

        static void Print(int[] value)
        {
            Console.Write("[ ");
            for (int i = 0; i < value.Length; i++)
            { if (i > 0) Console.Write(", "); Print(value[i]); }
            Console.Write(" ]");
        }
        static void Print(byte[] value)
        {
            Console.Write("[ ");
            for (int i = 0; i < value.Length; i++)
            { if (i > 0) Console.Write(", "); Print(value[i]); }
            Console.Write(" ]");
        }
        static void Print(short[] value)
        {
            Console.Write("[ ");
            for (int i = 0; i < value.Length; i++)
            { if (i > 0) Console.Write(", "); Print(value[i]); }
            Console.Write(" ]");
        }
        static void Print(long[] value)
        {
            Console.Write("[ ");
            for (int i = 0; i < value.Length; i++)
            { if (i > 0) Console.Write(", "); Print(value[i]); }
            Console.Write(" ]");
        }
        static void Print(float[] value)
        {
            Console.Write("[ ");
            for (int i = 0; i < value.Length; i++)
            { if (i > 0) Console.Write(", "); Print(value[i]); }
            Console.Write(" ]");
        }
        static void Print(double[] value)
        {
            Console.Write("[ ");
            for (int i = 0; i < value.Length; i++)
            { if (i > 0) Console.Write(", "); Print(value[i]); }
            Console.Write(" ]");
        }

        static void Print(string[] value)
        {
            Console.Write("[ ");
            for (int i = 0; i < value.Length; i++)
            { if (i > 0) Console.Write(", "); Print(value[i]); }
            Console.Write(" ]");
        }
        static void Print(char[] value)
        {
            Console.Write("[ ");
            for (int i = 0; i < value.Length; i++)
            { if (i > 0) Console.Write(", "); Print(value[i]); }
            Console.Write(" ]");
        }

        static void Print(bool[] value)
        {
            Console.Write("[ ");
            for (int i = 0; i < value.Length; i++)
            { if (i > 0) Console.Write(", "); Print(value[i]); }
            Console.Write(" ]");
        }

        static void Print(object value)
        {
            {
                if (value is byte) { Print((byte)value); return; }
                if (value is short) { Print((short)value); return; }
                if (value is int) { Print((int)value); return; }
                if (value is long) { Print((long)value); return; }
                if (value is float) { Print((float)value); return; }
                if (value is char) { Print((char)value); return; }
                if (value is string) { Print((string)value); return; }
                if (value is byte[]) { Print((byte[])value); return; }
                if (value is short[]) { Print((short[])value); return; }
                if (value is int[]) { Print((int[])value); return; }
                if (value is long[]) { Print((long[])value); return; }
                if (value is float[]) { Print((float[])value); return; }
                if (value is char[]) { Print((char[])value); return; }
                if (value is string[]) { Print((string[])value); return; }
            }

            var type = value.GetType();
            var fields = type.GetFields();

            Console.Write("{ ");
            for (int i = 0; i < fields.Length; i++)
            {
                System.Reflection.FieldInfo field = fields[i];
                Console.Write(field.Name + ": ");
                Print(field.GetValue(value));
                Console.Write("; ");
            }

            Console.Write("}");
        }
    }
#endif
}
