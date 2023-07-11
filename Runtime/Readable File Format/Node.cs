using System.Collections.Generic;
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

        internal string[] path;

        public string Path => path == null ? null : string.Join('/', path);

        internal Value(string[] path)
        {
            this.LiteralValue = null;
            this.Type = ValueType.LITERAL;
            this.ObjectValue = null;
            this._location = Location.Null;
            this.path = path;
        }

        /// <summary>
        /// Returns the raw literal value, or <see langword="null"/> if none.
        /// </summary>
        public readonly string String => LiteralValue ?? null;
        /// <summary>
        /// Returns the <see cref="float"/> literal value, or <see langword="null"/> if the literal value cannot be parsed to a <see cref="float"/> value.
        /// </summary>
        public readonly float? Float
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
        public readonly int? Int
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
        public readonly bool? Bool
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
        /// Returns the array equivalent of the object value.
        /// </summary>
        public readonly Value[] Array
        {
            get
            {
                Value[] result = new Value[ObjectValue.Count];
                var childNames = ObjectValue.Keys.ToArray();
                int j = 0;
                for (int i = 0; i < childNames.Length; i++)
                {
                    if (!int.TryParse(childNames[i], out int targetI)) return null;
                    if (targetI < 0 || targetI >= result.Length) return null;
                    result[targetI] = ObjectValue[childNames[i]];
                    j++;
                }
                if (j != result.Length) return null;
                return result;
            }
        }

        public readonly bool IsNull
        {
            get
            {
                if (Type == ValueType.LITERAL) return LiteralValue == null;
                if (Type == ValueType.OBJECT) return ObjectValue == null;
                return true;
            }
        }

        public readonly bool IsEmpty
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
            readonly get
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

        public readonly Value this[params string[] names]
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

        public readonly bool TryGetNode(string name, out Value value)
        {
            if (ObjectValue == null) { value = Value.Literal(null); return false; }
            return ObjectValue.TryGetValue(name, out value);
        }
        /// <summary>
        /// Returns <see langword="true"/> if the object value has a child named <paramref name="name"/>, otherwise <see langword="false"/>.
        /// </summary>
        public readonly bool Has(string name) => ObjectValue != null && ObjectValue.ContainsKey(name);

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

        readonly string GetDebuggerDisplay() => Type switch
        {
            ValueType.OBJECT => "{ . }",
            ValueType.LITERAL => LiteralValue,
            _ => ToString(),
        };

        public readonly T Reference<T>(Dictionary<string, T> map)
        {
            if (LiteralValue == null) return default;
            if (map.ContainsKey(LiteralValue)) return map[LiteralValue];
            Debug.LogWarning($"Reference \"{LiteralValue}\" not found");
            return default;
        }

        /// <summary>
        /// Converts the object value to a dictionary where the <c>Key</c> is the name of the child node and <c>Value</c> is the child node.
        /// </summary>
        public readonly Dictionary<string, Value> Dictionary()
        {
            Dictionary<string, Value> result = new();
            foreach (var pair in ObjectValue)
            { result[pair.Key] = pair.Value; }
            return result;
        }

        /// <summary>
        /// Converts the node to its text equivalent. The result can be parsed back to <see cref="Value"/> with the <see cref="Parser"/>
        /// </summary>
        public readonly string ToSDF(bool minimal = false) => ToSDF(minimal, 0, true);
        readonly string ToSDF(bool minimal, int indent, bool isRoot)
        {
            string result = "";

            if (Type == ValueType.LITERAL)
            {
                if (Float.HasValue)
                {
                    result += Float.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (Bool.HasValue)
                {
                    result += Bool.Value ? "true" : "false";
                }
                else
                {
                    result += $"\"{(LiteralValue ?? "").Replace(@"\", @"\\").Replace("\"", "\\\"")}\"";
                }
            }
            else if (Type == ValueType.OBJECT)
            {
                if (Array == null)
                {
                    if (minimal)
                    {
                        if (!isRoot) result += "{";
                        foreach (var pair in ObjectValue)
                        { result += $"{pair.Key}:{pair.Value.ToSDF(minimal, indent + 2, false)}"; }
                        if (!isRoot) result += "}";
                    }
                    else
                    {
                        if (ObjectValue.Count == 0)
                        {
                            if (!isRoot) result += "{ }";
                        }
                        else
                        {
                            if (isRoot)
                            {
                                foreach (var pair in ObjectValue)
                                {
                                    result += $"{pair.Key}: {pair.Value.ToSDF(minimal, indent, false)}\r\n";
                                }
                            }
                            else
                            {
                                result += "{\r\n";
                                foreach (var pair in ObjectValue)
                                {
                                    result += "".PadLeft(indent + 2, ' ') + $"{pair.Key}: {pair.Value.ToSDF(minimal, indent + 2, false)}\r\n";
                                }
                                result += "".PadLeft(indent, ' ') + "}";
                            }
                        }
                    }
                }
                else
                {
                    Value[] arrayValue = Array;
                    if (minimal)
                    {
                        result += "[";
                        foreach (var item in arrayValue)
                        { result += $"{item.ToSDF(minimal, indent + 2, false)}"; }
                        result += "]";
                    }
                    else
                    {
                        if (arrayValue.Length == 0)
                        {
                            result += "[ ]";
                        }
                        else
                        {
                            result += "[\r\n";
                            foreach (var item in arrayValue)
                            { result += "".PadLeft(indent + 2, ' ') + $"{item.ToSDF(minimal, indent + 2, false)}\r\n"; }
                            result += "".PadLeft(indent, ' ') + "]";
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Converts the node to json. The result can be parsed back to <see cref="Value"/> with the <see cref="Json.Parser"/>
        /// </summary>
        public readonly string ToJSON(bool minimal = false) => ToJSON(minimal, 0);
        readonly string ToJSON(bool minimal, int indent)
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

        public override readonly bool Equals(object obj)
        {
            if (obj is not Value other) return false;
            return Equals(other);
        }
        readonly public bool Equals(Value other)
        {
            if (other.Type != Type) return false;
            switch (Type)
            {
                case ValueType.LITERAL:
                    {
                        return string.Equals(LiteralValue, other.LiteralValue);
                    }
                case ValueType.OBJECT:
                    {
                        foreach (var pair in ObjectValue)
                        {
                            if (!other.ObjectValue.TryGetValue(pair.Key, out var objValue)) return false;
                            if (!pair.Value.Equals(objValue)) return false;
                        }
                        foreach (var pair in other.ObjectValue)
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
        public static implicit operator Value(string v) => Value.Literal(v);
        public static implicit operator Value(Value[] v)
        {
            Value result = Value.Object();
            result["Length"] = v.Length;
            for (int i = 0; i < v.Length; i++)
            { result[i] = v[i]; }
            return result;
        }

        public static implicit operator string(Value v) => v.String;
        public static implicit operator bool?(Value v) => v.Bool;
        public static implicit operator int?(Value v) => v.Int;
        public static implicit operator float?(Value v) => v.Float;

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
            readonly get => _location;
            internal set => _location = value;
        }
        public readonly IReadOnlyCollection<string> ChildNames
        {
            get
            {
                if (this.Type != ValueType.OBJECT) return System.Array.Empty<string>();
                if (ObjectValue == null) return System.Array.Empty<string>();
                return ObjectValue.Keys;
            }
        }

        Location _location;

        public readonly T Deserialize<T>() where T : IDeserializableText
        {
            IDeserializableText instance = (IDeserializableText)System.Activator.CreateInstance(typeof(T));
            instance.DeserializeText(this);
            return (T)instance;
        }
    }

}
