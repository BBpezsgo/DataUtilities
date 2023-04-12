using System;
using System.Collections.Generic;

namespace DataUtilities.Runtime.Serializer
{
    static class Extensions
    {
        internal static T[] Get<T>(this T[] array, int startIndex, int length)
        {
            List<T> result = new();
            for (int i = startIndex; i < length + startIndex; i++)
            { result.Add(array[i]); }
            return result.ToArray();
        }
    }

    public class Deserializer
    {
        readonly byte[] data = Array.Empty<byte>();
        int currentIndex;

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
        public int DeserializeInt32()
        {
            var data = this.data.Get(currentIndex, 4);
            currentIndex += 4;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }
        public byte DeserializeByte()
        {
            var data = this.data.Get(currentIndex, 1);
            currentIndex += 1;
            return data[0];
        }
        public char DeserializeChar()
        {
            var data = this.data.Get(currentIndex, 2);
            currentIndex += 2;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToChar(data, 0);
        }
        public short DeserializeInt16()
        {
            var data = this.data.Get(currentIndex, 2);
            currentIndex += 2;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }
        public float DeserializeFloat()
        {
            var data = this.data.Get(currentIndex, 4);
            currentIndex += 4;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToSingle(data, 0);
        }
        public bool DeserializeBoolean()
        {
            var data = this.data.Get(currentIndex, 1);
            currentIndex++;
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return BitConverter.ToBoolean(data, 0);
        }
        public string DeserializeString()
        {
            int length = DeserializeInt32();
            if (length == -1) return null;
            if (length == 0) return string.Empty;
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = DeserializeChar();
            }
            return new string(result);
        }
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

    public class Serializer
    {
        readonly List<byte> result = new();

        public byte[] Result => result.ToArray();

        public void Serialize(int v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }
        public void Serialize(float v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }
        public void Serialize(bool v)
        {
            result.Add(BitConverter.GetBytes(v)[0]);
        }
        public void Serialize(byte v)
        {
            result.Add(v);
        }
        public void Serialize(short v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }
        public void Serialize(char v)
        {
            var result = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(result);
            this.result.AddRange(result);
        }
        public void Serialize(string v)
        {
            if (v == null)
            {
                Serialize(-1);
                return;
            }
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i]); }
        }
        public void Serialize(short[] v)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i]); }
        }
        public void Serialize(int[] v)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i]); }
        }
        public void Serialize(string[] v)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i]); }
        }
        public void Serialize(char[] v)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i]); }
        }
        public void SerializeObjectArray<T>(ISerializable<T>[] v)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { SerializeObject(v[i]); }
        }
        public void SerializeObjectArray<T>(T[] v, Action<Serializer, T> callback)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { callback.Invoke(this, v[i]); }
        }
        public void SerializeObject<T>(ISerializable<T> v)
        {
            v.Serialize(this);
        }
        public void SerializeObject<T>(T v, Action<Serializer, T> callback)
        {
            callback.Invoke(this, v);
        }
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
