using System.Collections.Generic;

#nullable enable

namespace DataUtilities.ReadableFileFormat
{
    public readonly struct Location
    {
        internal readonly uint Character;
        internal readonly uint Column;
        internal readonly uint Line;
        readonly bool _isNull;
        internal bool IsNull => _isNull;

        internal static Location Null => new();

        public Location(uint character, uint column, uint line)
        {
            Character = character;
            Column = column;
            Line = line;
            _isNull = false;
        }

        public override string ToString() => IsNull ? string.Empty : $"{Line + 1}:{Column + 1}";
    }

    public interface ISerializableText
    {
        Value SerializeText();
    }
    public interface IDeserializableText
    {
        void DeserializeText(Value data);
    }
    public interface IFullySerializableText : ISerializableText, IDeserializableText
    {

    }

    public static class Extensions
    {
        public static T[] Convert<T>(this Value[] self) where T : IDeserializableText
        {
            T[] result = new T[self.Length];
            for (int i = 0; i < result.Length; i++)
            {
                object instanceObj = System.Activator.CreateInstance(typeof(T)) ?? throw new System.NullReferenceException();
                IDeserializableText instance = (IDeserializableText)instanceObj;
                instance.DeserializeText(self[i]);
                result[i] = (T)instance;
            }
            return result;
        }
        public static T[] Convert<T>(this Value[] self, System.Func<Value, T> converter)
        {
            T[] result = new T[self.Length];
            for (int i = 0; i < result.Length; i++)
            { result[i] = converter.Invoke(self[i]); }
            return result;
        }

        static readonly Dictionary<System.Type, System.Delegate> converters = new()
        {
            { typeof(int), (System.Func<Value, int>)(v => v.Int ?? 0) },
            { typeof(string), (System.Func<Value, string?>)(v => v.String) },
            { typeof(bool), (System.Func<Value, bool>)(v => v.Bool ?? false) },
            { typeof(float), (System.Func<Value, float>)(v => v.Float ?? 0f) },
        };
        public static T[] ConvertPrimitive<T>(this Value[] self)
        {
            if (!converters.TryGetValue(typeof(T), out System.Delegate? _converter))
            { throw new System.NotImplementedException($"Converter for type {typeof(T)} not implemented"); }

            System.Func<Value, T> converter = (System.Func<Value, T>)_converter;
            T[] result = new T[self.Length];
            for (int i = 0; i < self.Length; i++)
            { result[i] = converter(self[i]); }
            return result;
        }
        public static Dictionary<string, T> Convert<T>(this Dictionary<string, Value> self, System.Func<Value, T> converter)
        {
            Dictionary<string, T> result = new();
            foreach (var pair in self)
            { result.Add(pair.Key, converter.Invoke(pair.Value)); }
            return result;
        }
        public static Dictionary<string, T> ConvertNotAll<T>(this Dictionary<string, Value> self, System.Func<Value, T?> converter)
        {
            Dictionary<string, T> result = new();
            foreach (var pair in self)
            {
                T? v = converter.Invoke(pair.Value);
                if (v != null) result.Add(pair.Key, v);
            }
            return result;
        }
    }
}
