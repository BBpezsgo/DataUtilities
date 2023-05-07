﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY
using Debug = UnityEngine.Debug;
#endif

namespace DataUtilities.ReadableFileFormat
{
    public enum ValueType
    {
        /// <summary>
        /// A simple data type like <see cref="string"/>, <see cref="int"/>, <see cref="float"/> and <see cref="bool"/>.
        /// </summary>
        LITERAL,
        /// <summary>
        /// A complex data type with child fields.
        /// </summary>
        OBJECT,
    }

    [System.Diagnostics.DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
#pragma warning disable CS0659
#pragma warning disable CS0661
    public struct Value
#pragma warning restore CS0661
#pragma warning restore CS0659
    {
        /// <summary>
        /// The type of this node.
        /// </summary>
        public ValueType Type;

        string LiteralValue;
        Dictionary<string, Value> ObjectValue;

        /// <summary>
        /// Returns the raw literal value, or <see langword="null"/> if none.
        /// </summary>
        public string String => LiteralValue ?? null;
        /// <summary>
        /// Returns the <see cref="float"/> literal value, or <see langword="null"/> if the literal value cannot be parsed to a <see cref="float"/> value.
        /// </summary>
        public float? Float
        {
            get
            {
                if (LiteralValue == null) return null;
                if (!float.TryParse(LiteralValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result)) return null;
                return result;
            }
        }
        /// <summary>
        /// Returns the <see cref="int"/> literal value, or <see langword="null"/> if the literal value cannot be parsed to a <see cref="int"/> value.
        /// </summary>
        public int? Int
        {
            get
            {
                if (LiteralValue == null) return null;
                if (!int.TryParse(LiteralValue, out int result)) return null;
                return result;
            }
        }
        /// <summary>
        /// Returns the <see cref="bool"/> literal value, or <see langword="null"/> if the literal value cannot be parsed to a <see cref="bool"/> value.<br/>
        /// Boolean equivalents of the literal value:<br/>
        /// "true" => <see langword="true"/><br/>
        /// "false" => <see langword="false"/><br/>
        /// "yes" => <see langword="true"/><br/>
        /// "no" => <see langword="false"/><br/>
        /// "1" => <see langword="true"/><br/>
        /// "0" => <see langword="false"/>
        /// </summary>
        public bool? Bool
        {
            get
            {
                if (LiteralValue == null) return null;
                string stringValue = LiteralValue.Trim().ToLower();

                if (stringValue == "true") return true;
                if (stringValue == "false") return false;
                if (stringValue == "yes") return true;
                if (stringValue == "no") return false;
                if (stringValue == "1") return true;
                if (stringValue == "0") return false;

                return null;
            }
        }
        /// <summary>
        /// Returns the array equivalent of the object value, or null if the node has no child named "Length", or an IndexOutOfRange exception occur.
        /// </summary>
        public Value[] Array
        {
            get
            {
                if (!TryGetNode("Length", out Value length)) return null;
                var lengthInt = length.Int ?? 0;
                Value[] result = new Value[lengthInt];
                for (int i = 0; i < lengthInt; i++)
                {
                    if (!TryGetNode(i.ToString(), out Value element)) return null;
                    result[i] = element;
                }
                return result;
            }
        }

        public bool IsNull
        {
            get
            {
                if (Type == ValueType.LITERAL) return LiteralValue == null;
                if (Type == ValueType.OBJECT) return ObjectValue == null;
                return true;
            }
        }

        public bool IsEmpty
        {
            get
            {
                if (IsNull) return true;
                if (Type == ValueType.LITERAL) return string.IsNullOrEmpty(LiteralValue);
                if (Type == ValueType.OBJECT) return ObjectValue.Count == 0;
                return true;
            }
        }

        public Value this[string name]
        {
            get
            {
                if (Has(name)) return ObjectValue[name];
                return Value.Literal(null);
            }
            set
            {
                if (Has(name)) ObjectValue[name] = value;
                else ObjectValue.Add(name, value);
            }
        }

        public Value this[params string[] names]
        {
            get
            {
                foreach (var name in names) if (Has(name)) return ObjectValue[name];
                return Value.Literal(null);
            }
        }

        public Value this[int name]
        {
            get => this[name.ToString()];
            set => this[name.ToString()] = value;
        }

        public bool TryGetNode(string name, out Value value)
        {
            if (ObjectValue == null) { value = Value.Literal(null); return false; }
            return ObjectValue.TryGetValue(name, out value);
        }
        /// <summary>
        /// Returns <see langword="true"/> if the object value has a child named <paramref name="name"/>, otherwise <see langword="false"/>.
        /// </summary>
        public bool Has(string name) => ObjectValue != null && ObjectValue.ContainsKey(name);
        public Value? TryGetNode(string name) => Has(name) ? ObjectValue[name] : null;

        /// <summary>
        /// Initializes a <see cref="Value"/> with type of <see cref="ValueType.OBJECT"/>.
        /// </summary>
        public static Value Object() => new()
        { Type = ValueType.OBJECT, ObjectValue = new Dictionary<string, Value>(), LiteralValue = null, };
        public static Value Object(Dictionary<string, Value> v) => new()
        { Type = ValueType.OBJECT, ObjectValue = v, LiteralValue = null, };
        public static Value Object(ISerializableText value) => value.SerializeText();
        public static Value Object(ISerializableText[] value)
        {
            Value result = Object();
            result["Length"] = value.Length;
            for (int i = 0; i < value.Length; i++)
            { result[i] = Object(value[i]); }
            return result;
        }
        public static Value Object<T>(T[] value, System.Func<T, Value> converter)
        {
            Value result = Object();
            result["Length"] = value.Length;
            for (int i = 0; i < value.Length; i++)
            { result[i] = converter.Invoke(value[i]); }
            return result;
        }
        public static Value Object(string[] value)
        {
            Value result = Object();
            result["Length"] = value.Length;
            for (int i = 0; i < value.Length; i++)
            { result[i] = Value.Literal(value[i]); }
            return result;
        }
        public static Value Object(int[] value)
        {
            Value result = Object();
            result["Length"] = value.Length;
            for (int i = 0; i < value.Length; i++)
            { result[i] = value[i]; }
            return result;
        }
        public static Value Object(Dictionary<string, ISerializableText> value)
        {
            Value result = Object();
            foreach (KeyValuePair<string, ISerializableText> pair in value)
            { result[pair.Key] = Object(pair.Value); }
            return result;
        }
        public static Value Object(Dictionary<string, int> value)
        {
            Value result = Object();
            foreach (KeyValuePair<string, int> pair in value)
            { result[pair.Key] = Literal(pair.Value.ToString()); }
            return result;
        }
        public static Value Object(Dictionary<string, string> value)
        {
            Value result = Object();
            foreach (KeyValuePair<string, string> pair in value)
            { result[pair.Key] = Literal(pair.Value.ToString()); }
            return result;
        }
        /// <summary>
        /// Initializes a <see cref="Value"/> with type of <see cref="ValueType.LITERAL"/>.
        /// </summary>
        public static Value Literal(string value) => new()
        { Type = ValueType.LITERAL, ObjectValue = null, LiteralValue = value, };
        /// <summary>
        /// Initializes a <see cref="Value"/> with type of <see cref="ValueType.LITERAL"/>.
        /// </summary>
        public static Value Literal(bool value) => new()
        { Type = ValueType.LITERAL, ObjectValue = null, LiteralValue = value ? "true" : "false", };
        /// <summary>
        /// Initializes a <see cref="Value"/> with type of <see cref="ValueType.LITERAL"/>.
        /// </summary>
        public static Value Literal(float value) => new()
        { Type = ValueType.LITERAL, ObjectValue = null, LiteralValue = value.ToString(System.Globalization.CultureInfo.InvariantCulture), };
        /// <summary>
        /// Initializes a <see cref="Value"/> with type of <see cref="ValueType.LITERAL"/>.
        /// </summary>
        public static Value Literal(int value) => new()
        { Type = ValueType.LITERAL, ObjectValue = null, LiteralValue = value.ToString(), };

        public bool RemoveNode(string name)
        {
            if (Type != ValueType.OBJECT) return false;
            if (ObjectValue == null) return false;
            return ObjectValue.Remove(name);
        }

        string GetDebuggerDisplay() => Type switch
        {
            ValueType.OBJECT => "{ . }",
            ValueType.LITERAL => LiteralValue,
            _ => ToString(),
        };

        public T Reference<T>(Dictionary<string, T> map)
        {
            if (LiteralValue == null) return default;
            if (map.ContainsKey(LiteralValue)) return map[LiteralValue];
            Debug.LogWarning($"Reference \"{LiteralValue}\" not found");
            return default;
        }

        /// <summary>
        /// Converts the object value to a dictionary where the <c>Key</c> is the name of the child node and <c>Value</c> is the child node.
        /// </summary>
        public Dictionary<string, Value> Dictionary()
        {
            Dictionary<string, Value> result = new();
            foreach (var pair in ObjectValue)
            { result[pair.Key] = pair.Value; }
            return result;
        }

        /// <summary>
        /// Converts the node to its text equivalent. The result can be parsed back to <see cref="Value"/> with the <see cref="Parser"/>
        /// </summary>
        public string ToSDF(bool minimal = false) => ToSDF(minimal, 0);
        string ToSDF(bool minimal, int indent)
        {
            string result = "";

            if (Type == ValueType.LITERAL)
            {
                result += $"\"{(LiteralValue ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
            }
            else if (Type == ValueType.OBJECT)
            {
                if (Array == null)
                {
                    if (minimal)
                    {
                        result += "{";
                        foreach (var pair in ObjectValue)
                        { result += $"{pair.Key}:{pair.Value.ToSDF(minimal, indent + 2)}"; }
                        result += "}";
                    }
                    else
                    {
                        result += "{\r\n";
                        foreach (var pair in ObjectValue)
                        { result += "".PadLeft(indent + 2, ' ') + $"{pair.Key}: {pair.Value.ToSDF(minimal, indent + 2)}\r\n"; }
                        result += "".PadLeft(indent, ' ') + "}";
                    }
                }
                else
                {
                    Value[] arrayValue = Array;
                    if (minimal)
                    {
                        result += "[";
                        foreach (var item in arrayValue)
                        { result += $"{item.ToSDF(minimal, indent + 2)}"; }
                        result += "]";
                    }
                    else
                    {
                        result += "[\r\n";
                        foreach (var item in arrayValue)
                        { result += "".PadLeft(indent + 2, ' ') + $"{item.ToSDF(minimal, indent + 2)}\r\n"; }
                        result += "".PadLeft(indent, ' ') + "]";
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Converts the node to json. The result can be parsed back to <see cref="Value"/> with the <see cref="Json.Parser"/>
        /// </summary>
        public string ToJSON(bool minimal = false) => ToJSON(minimal, 0);
        string ToJSON(bool minimal, int indent)
        {
            string result = "";

            if (Type == ValueType.LITERAL)
            {
                if (Float.HasValue)
                {
                    result += $"{Float.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
                }
                else if (Int.HasValue)
                {
                    result += $"{Int.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
                }
                else if (Bool.HasValue)
                {
                    result += Bool.Value ? "true" : "false";
                }
                else
                {
                    result += $"\"{(LiteralValue ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
                }
            }
            else if (Type == ValueType.OBJECT)
            {
                if (Array == null)
                {
                    if (minimal)
                    {
                        result += "{";
                        foreach (var pair in ObjectValue)
                        {
                            result += $"\"{pair.Key}\":{pair.Value.ToJSON(minimal, indent + 2)},";
                        }
                        if (result.EndsWith(',')) result = result[..^1];
                        result += "}";
                    }
                    else
                    {
                        result += "{\r\n";
                        foreach (var pair in ObjectValue)
                        {
                            result += "".PadLeft(indent + 2, ' ') + $"\"{pair.Key}\": {pair.Value.ToJSON(minimal, indent + 2)},\r\n";
                        }
                        if (result.EndsWith(",\r\n")) result = result[..^3] + "\r\n";
                        result += "".PadLeft(indent, ' ') + "}";
                    }
                }
                else
                {
                    Value[] arrayValue = Array;
                    if (minimal)
                    {
                        result += "[";
                        foreach (var element in arrayValue)
                        {
                            result += $"{element.ToJSON(minimal, indent + 2)},";
                        }
                        if (result.EndsWith(',')) result = result[..^1];
                        result += "]";
                    }
                    else
                    {
                        result += "[\r\n";
                        foreach (var element in arrayValue)
                        {
                            result += "".PadLeft(indent + 2, ' ') + $"{element.ToJSON(minimal, indent + 2)},\r\n";
                        }
                        if (result.EndsWith(",\r\n")) result = result[..^3] + "\r\n";
                        result += "".PadLeft(indent, ' ') + "]";
                    }
                }
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Value other) return false;
            return Equals(other);
        }
        public bool Equals(Value obj)
        {
            if (obj.Type != Type) return false;
            switch (Type)
            {
                case ValueType.LITERAL:
                    {
                        return string.Equals(LiteralValue, obj.LiteralValue);
                    }
                case ValueType.OBJECT:
                    {
                        foreach (var pair in ObjectValue)
                        {
                            if (!obj.ObjectValue.TryGetValue(pair.Key, out var objValue)) return false;
                            if (!pair.Value.Equals(objValue)) return false;
                        }
                        foreach (var pair in obj.ObjectValue)
                        {
                            if (!ObjectValue.TryGetValue(pair.Key, out var objValue)) return false;
                            if (!pair.Value.Equals(objValue)) return false;
                        }
                        return true;
                    }
                default: return false;
            };
        }

        public static implicit operator Value(bool v) => Value.Literal(v);
        public static implicit operator Value(int v) => Value.Literal(v);
        public static implicit operator Value(float v) => Value.Literal(v);
        public static implicit operator Value(Value[] v)
        {
            Value result = Value.Object();
            result["Length"] = v.Length;
            for (int i = 0; i < v.Length; i++)
            { result[i] = v[i]; }
            return result;
        }
        public static bool operator ==(Value left, Value right) => left.Equals(right);
        public static bool operator !=(Value left, Value right) => !(left == right);

        [System.Flags]
        public enum CombineOptions : ushort
        {
            OVERRIDE_LITERAL_WITH_OBJECT,
            OVERRIDE_LITERAL_WITH_LITERAL,
            OVERRIDE_OBJECT_WITH_LITERAL,
        }

        public const CombineOptions DefaultCombineOptions =
            CombineOptions.OVERRIDE_LITERAL_WITH_LITERAL;

        /// <summary>
        /// Combines <paramref name="other"/> with <see langword="this"/>.<br/>
        /// <b><see langword="this"/> will be the base object!</b>
        /// </summary>
        public void Combine(Value other, CombineOptions flags = DefaultCombineOptions)
        {
            if (this.Type == ValueType.LITERAL)
            {
                if (other.Type == ValueType.LITERAL)
                {
                    if ((flags & CombineOptions.OVERRIDE_LITERAL_WITH_LITERAL) != 0)
                    {
                        this.LiteralValue = other.LiteralValue;
                    }
                }
                else if (other.Type == ValueType.OBJECT)
                {
                    if ((flags & CombineOptions.OVERRIDE_LITERAL_WITH_OBJECT) != 0)
                    {
                        this.Type = ValueType.OBJECT;
                        this.LiteralValue = null;
                        this.ObjectValue = other.ObjectValue;
                    }
                }
                else throw new System.NotImplementedException();
            }
            else if (this.Type == ValueType.OBJECT)
            {
                if (other.Type == ValueType.LITERAL)
                {
                    if ((flags & CombineOptions.OVERRIDE_OBJECT_WITH_LITERAL) != 0)
                    {
                        this.Type = ValueType.LITERAL;
                        this.LiteralValue = other.LiteralValue;
                        this.ObjectValue = null;
                    }
                }
                else if (other.Type == ValueType.OBJECT)
                {
                    Value.Combine(this.ObjectValue, other.ObjectValue, flags);
                }
                else throw new System.NotImplementedException();
            }
            else throw new System.NotImplementedException();
        }

        /// <summary>
        /// Combines <paramref name="b"/> with <paramref name="a"/>.<br/>
        /// <b><paramref name="a"/> will be the base object!</b>
        /// </summary>
        public static void Combine(Dictionary<string, Value> a, Dictionary<string, Value> b, CombineOptions flags = DefaultCombineOptions)
        {
            foreach (KeyValuePair<string, Value> pair in b)
            {
                if (a.TryGetValue(pair.Key, out Value @this))
                {
                    @this.Combine(pair.Value, flags);
                    a[pair.Key] = @this;
                }
                else
                {
                    a.Add(pair.Key, pair.Value);
                }
            }
        }

        public Location Location
        {
            get => _location;
            internal set => _location = value;
        }
        public IReadOnlyCollection<string> ChildNames
        {
            get
            {
                if (this.Type != ValueType.OBJECT) return System.Array.Empty<string>();
                if (ObjectValue == null) return System.Array.Empty<string>();
                return ObjectValue.Keys;
            }
        }

        Location _location;

        public T Deserialize<T>() where T : IDeserializableText
        {
            IDeserializableText instance = (IDeserializableText)System.Activator.CreateInstance(typeof(T));
            instance.DeserializeText(this);
            return (T)instance;
        }
    }

    public readonly struct Location
    {
        internal readonly uint Character;
        internal readonly uint Column;
        internal readonly uint Line;
        readonly bool _isNull;
        internal bool IsNull => _isNull;

        public Location(uint character, uint column, uint line)
        {
            Character = character;
            Column = column;
            Line = line;
            _isNull = false;
        }

        public override string ToString() => IsNull ? "" : $"{Line + 1}:{Column + 1}";
    }

    /// <summary>
    /// Can parse <see cref="string"/> to <see cref="Value"/>.
    /// To start converting, just call the <see cref="Parse"/> function.
    /// </summary>
    public class Parser : Text.TextDeserializer
    {
        Location CurrentLocation => new(CurrentCharacterIndex, CurrentColumn, CurrentLine);

        public static Value Parse(string data) => new Parser(data)._Parse();

        public Parser(string data) : base(data + ' ') { }

#pragma warning disable IDE1006 // Naming Styles
        Value _Parse()
#pragma warning restore IDE1006 // Naming Styles
        {
            ConsumeCharacters(WhitespaceCharacters);

            Value root = Value.Object();
            root.Location = CurrentLocation;

            bool inParentecieses = CurrentCharacter == '{';
            if (inParentecieses)
            {
                ConsumeNext();
                ConsumeCharacters(WhitespaceCharacters);
            }

            int endlessSafe = INFINITY;
            while (CurrentCharacter != '\0')
            {
                if (endlessSafe-- <= 0)
                { Debug.LogError($"Endless loop!"); break; }

                ConsumeCharacters(WhitespaceCharacters);
                string propertyName = ExpectPropertyName();
                ConsumeCharacters(WhitespaceCharacters);
                Value propertyValue = ExpectValue();
                root[propertyName] = propertyValue;
                ConsumeCharacters(WhitespaceCharacters);

                if (inParentecieses && CurrentCharacter == '}')
                {
                    ConsumeNext();
                    break;
                }
            }

            return root;
        }

        Value ExpectValue()
        {
            ConsumeCharacters(WhitespaceCharacters);
            var loc = CurrentLocation;
            if (CurrentCharacter == '{')
            {
                ConsumeNext();
                ConsumeCharacters(WhitespaceCharacters);
                Value objectValue = Value.Object();
                objectValue.Location = loc;
                int endlessSafe = INFINITY;
                while (CurrentCharacter != '}')
                {
                    if (endlessSafe-- <= 0)
                    { Debug.LogError($"Endless loop!"); break; }
                    ConsumeCharacters(WhitespaceCharacters);
                    string propertyName = ExpectPropertyName();
                    ConsumeCharacters(WhitespaceCharacters);
                    Value? propertyValue = ExpectValue();
                    if (propertyValue.HasValue)
                    { objectValue[propertyName] = propertyValue.Value; }
                    else
                    { Debug.LogError($"Property \"{propertyName}\" does not have a value"); }
                    ConsumeCharacters(WhitespaceCharacters);
                }
                ConsumeNext();
                return objectValue;
            }
            if (CurrentCharacter == '[')
            {
                ConsumeNext();
                ConsumeCharacters(WhitespaceCharacters);
                Value objectValue = Value.Object();
                objectValue.Location = loc;
                int endlessSafe = INFINITY;
                int index = 0;
                while (CurrentCharacter != ']')
                {
                    if (endlessSafe-- <= 0)
                    { Debug.LogError($"Endless loop!"); break; }
                    ConsumeCharacters(WhitespaceCharacters);
                    Value? listItemValue = ExpectValue();
                    if (listItemValue.HasValue)
                    { objectValue[index++] = listItemValue.Value; }
                    else
                    { Debug.LogError($"List has a null item"); }
                    ConsumeCharacters(WhitespaceCharacters);
                }
                ConsumeNext();
                objectValue["Length"] = Value.Literal(index.ToString());
                return objectValue;
            }

            if (CurrentCharacter == '"')
            {
                ConsumeNext();
                int endlessSafe = INFINITY;
                string literalValue = "";
                while (CurrentCharacter != '"')
                {
                    if (endlessSafe-- <= 0)
                    { Debug.LogError($"Endless loop!"); break; }
                    if (CurrentCharacter == '\\')
                    { ConsumeNext(); }
                    literalValue += ConsumeNext();
                }
                ConsumeNext();
                var result = Value.Literal(literalValue);
                result.Location = loc;
                return result;
            }

            {
                var anyValue = ConsumeUntil('{', '\r', '\n', ' ', '\t', '\0');
                var result = Value.Literal(anyValue);
                result.Location = loc;
                return result;
            }
        }

        string ExpectPropertyName()
        {
            ConsumeCharacters(WhitespaceCharacters);
            var result = ConsumeUntil(":");
            ConsumeNext();
            return result;
        }

        public static Value? LoadFile(string file) => !File.Exists(file) ? null : new Parser(File.ReadAllText(file))._Parse();
        public static bool TryLoadFile(string file, out Value result)
        {
            if (!File.Exists(file))
            {
                result = Value.Object();
                return false;
            }
            else
            {
                result = new Parser(File.ReadAllText(file))._Parse();
                return true;
            }
        }
    }

    public interface ISerializableText
    {
        Value SerializeText();
    }
    public interface IDeserializableText
    {
        void DeserializeText(Value data);
    }

    public static class Extensions
    {
        public static T[] Convert<T>(this Value[] self) where T : IDeserializableText
        {
            T[] result = new T[self.Length];
            for (int i = 0; i < result.Length; i++)
            {
                IDeserializableText instance = (IDeserializableText)System.Activator.CreateInstance(typeof(T));
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
            { typeof(string), (System.Func<Value, string>)(v => v.String) },
            { typeof(bool), (System.Func<Value, bool>)(v => v.Bool ?? false) },
            { typeof(float), (System.Func<Value, float>)(v => v.Float ?? 0f) },
        };
        public static T[] ConvertPrimitive<T>(this Value[] self)
        {
            if (!converters.TryGetValue(typeof(T), out System.Delegate _converter))
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
    }
}
