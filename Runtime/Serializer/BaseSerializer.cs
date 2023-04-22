using System;
using System.Collections.Generic;

namespace DataUtilities.Serializer
{
    public abstract class BaseSerializer
    {
        public abstract void Push(params byte[] bytes);

        /// <summary>
        /// Serializes the given <see cref="int"/>
        /// </summary>
        public abstract void Serialize(int v);
        /// <summary>
        /// Serializes the given <see cref="uint"/>
        /// </summary>
        public abstract void Serialize(uint v);
        /// <summary>
        /// Serializes the given <see cref="float"/>
        /// </summary>
        public abstract void Serialize(float v);
        /// <summary>
        /// Serializes the given <see cref="long"/>
        /// </summary>
        public abstract void Serialize(long v);
        /// <summary>
        /// Serializes the given <see cref="bool"/>
        /// </summary>
        public abstract void Serialize(bool v);
        /// <summary>
        /// Serializes the given <see cref="byte"/>
        /// </summary>
        public abstract void Serialize(byte v);
        /// <summary>
        /// Serializes the given <see cref="short"/>
        /// </summary>
        public abstract void Serialize(short v);
        /// <summary>
        /// Serializes the given <see cref="char"/>
        /// </summary>
        public abstract void Serialize(char v);
        /// <summary>
        /// Serializes the given <see cref="string"/>. Both the length and the encoding will be serialized.
        /// </summary>
        public virtual void Serialize(string v)
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
        public virtual void Serialize(short[] v)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i]); }
        }
        /// <summary>
        /// Serializes the given array of <see cref="int"/> with the <see cref="Serialize(int)"/> method. The length of the array will also be serialized.
        /// </summary>
        public virtual void Serialize(int[] v)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i]); }
        }
        /// <summary>
        /// Serializes the given array of <see cref="string"/> with the <see cref="Serialize(string)"/> method. The length of the array will also be serialized.
        /// </summary>
        public virtual void Serialize(string[] v)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i]); }
        }
        /// <summary>
        /// Serializes the given array of <see cref="char"/> with the <see cref="Serialize(char)"/> method. The length of the array will also be serialized.
        /// </summary>
        public virtual void Serialize(char[] v)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { Serialize(v[i]); }
        }
        /// <summary>
        /// Serializes the given array of <typeparamref name="T"/> with the <see cref="SerializeObject{T}(ISerializable{T})"/> method. The length of the array will also be serialized.
        /// </summary>
        public virtual void SerializeObjectArray<T>(ISerializable<T>[] v)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { SerializeObject(v[i]); }
        }
        /// <summary>
        /// Serializes the given array of <typeparamref name="T"/> with the <paramref name="callback"/> function. The length of the array will also be serialized.
        /// </summary>
        public virtual void SerializeObjectArray<T>(T[] v, Action<BaseSerializer, T> callback)
        {
            Serialize(v.Length);
            for (int i = 0; i < v.Length; i++)
            { callback.Invoke(this, v[i]); }
        }
        /// <summary>
        /// Serializes the given object <typeparamref name="T"/> with the <see cref="ISerializable{T}.Serialize(BaseSerializer)"/> method.
        /// </summary>
        public void SerializeObject<T>(ISerializable<T> v) => v.Serialize(this);
        /// <summary>
        /// Serializes the given object <typeparamref name="T"/> with the <paramref name="callback"/> function.
        /// </summary>
        public void SerializeObject<T>(T v, Action<BaseSerializer, T> callback) => callback.Invoke(this, v);
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
        protected virtual void Serialize(object v)
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
        public virtual void Serialize<TKey, TValue>(Dictionary<TKey, TValue> v, bool keyIsObj, bool valIsObj)
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
        public virtual void Serialize<TKey>(Dictionary<TKey, string> v, bool keyIsObj)
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
        public virtual void Serialize<TValue>(Dictionary<string, TValue> v, bool valIsObj)
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
}
