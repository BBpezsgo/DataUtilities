using System;
using System.Collections.Generic;

namespace DataUtilities.Serializer
{
    public abstract class BaseDeserializer
    {
        public abstract byte[] Pull(uint length);

        public virtual T[] DeserializeArray<T>()
        {
            int length = DeserializeInt32();
            T[] result = new T[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (T)Deserialize<T>();
            }
            return result;
        }
        public virtual T[] DeserializeObjectArray<T>() where T : ISerializable<T>
        {
            int length = DeserializeInt32();
            T[] result = new T[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (T)DeserializeObject<T>();
            }
            return result;
        }
        public virtual T[] DeserializeObjectArray<T>(Func<BaseDeserializer, T> callback)
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
        public virtual object Deserialize<T>()
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
        public abstract int DeserializeInt32();
        /// <summary>
        /// Deserializes the following <see cref="System.Int32"/> data (4 bytes)
        /// </summary>
        public abstract uint DeserializeUInt32();
        /// <summary>
        /// Returns the next byte
        /// </summary>
        public abstract byte DeserializeByte();
        /// <summary>
        /// Deserializes the following <see cref="System.Char"/> data (2 bytes)
        /// </summary>
        public abstract char DeserializeChar();
        /// <summary>
        /// Deserializes the following <see cref="System.Int16"/> data (2 bytes)
        /// </summary>
        public abstract short DeserializeInt16();
        /// <summary>
        /// Deserializes the following <see cref="System.Single"/> data (4 bytes)
        /// </summary>
        public abstract float DeserializeFloat();
        /// <summary>
        /// Deserializes the following <see cref="System.Boolean"/> data (1 bytes)
        /// </summary>
        public abstract bool DeserializeBoolean();
        /// <summary>
        /// Deserializes the following <see cref="System.String"/> data. Length and encoding are obtained automatically.
        /// </summary>
        public virtual string DeserializeString()
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
        /// This creates an instance of <typeparamref name="T"/> and then calls the <see cref="ISerializable{T}.Deserialize(BaseDeserializer)"/> method on the instance.
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
        public T DeserializeObject<T>(Func<BaseDeserializer, T> callback) => callback.Invoke(this);
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

        public abstract long DeserializeLong();
    }

}
