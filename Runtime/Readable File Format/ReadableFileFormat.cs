using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY
using Debug = UnityEngine.Debug;
#endif

namespace DataUtilities.ReadableFileFormat
{
    public enum ValueType
    {
        LITERAL,
        REFERENCE,
        OBJECT,
    }

    [System.Diagnostics.DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public struct Value
    {
        public ValueType Type;

        string LiteralValue;
        Dictionary<string, Value> ObjectValue;

        public string String => LiteralValue ?? null;
        public float? Float
        {
            get
            {
                var stringValue = String;
                if (stringValue == null) return null;
                if (!float.TryParse(stringValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result)) return null;
                return result;
            }
        }
        public int? Int
        {
            get
            {
                var stringValue = String;
                if (stringValue == null) return null;
                if (!int.TryParse(stringValue, out int result)) return null;
                return result;
            }
        }
        public bool? Bool
        {
            get
            {
                var stringValue = String;
                if (stringValue == null) return null;
                stringValue = stringValue.Trim().ToLower();

                if (stringValue == "true") return true;
                if (stringValue == "false") return false;
                if (stringValue == "yes") return true;
                if (stringValue == "no") return false;
                if (stringValue == "1") return true;
                if (stringValue == "0") return false;

                return null;
            }
        }
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
        public bool Has(string name) => ObjectValue != null && ObjectValue.ContainsKey(name);
        public Value? TryGetNode(string name) => Has(name) ? ObjectValue[name] : null;

        public static Value Object() => new()
        { Type = ValueType.OBJECT, ObjectValue = new Dictionary<string, Value>(), LiteralValue = null, };
        public static Value Literal(string value) => new()
        { Type = ValueType.LITERAL, ObjectValue = null, LiteralValue = value, };

        string GetDebuggerDisplay() => Type switch
        {
            ValueType.OBJECT => "{ . }",
            ValueType.LITERAL => LiteralValue,
            ValueType.REFERENCE => LiteralValue,
            _ => ToString(),
        };

        public Value SetReference(string referenceName)
        {
            this.Type = ValueType.REFERENCE;
            this.LiteralValue = referenceName;
            return this;
        }
        public Value SetReference() => this.SetReference(this.LiteralValue);

        public T Reference<T>(Dictionary<string, T> map)
        {
            if (String == null) return default;
            if (map.ContainsKey(String)) return map[String];
            Debug.LogWarning($"Reference \"{String}\" not found");
            return default;
        }
    }

    public class Parser
    {
        string Content;

        char CurrentCharacter => Content.Length == 0 ? '\0' : Content[0];

        static readonly char[] SpaceCharacters = new char[] { ' ', '\t' };
        static readonly char[] LinebrakCharacters = new char[] { '\r', '\n' };
        static readonly char[] WhitespaceCharacters = new char[] { ' ', '\t', '\r', '\n' };

        public Parser(string fileContentRaw) => Content = fileContentRaw;

        Value Parse()
        {
            ConsumeCharacters(WhitespaceCharacters);

            Value root = Value.Object();

            int endlessSafe = 500;
            while (CurrentCharacter != '\0')
            {
                if (endlessSafe-- <= 0) { Debug.LogError($"Endless loop!"); break; }

                ConsumeCharacters(WhitespaceCharacters);
                string propertyName = ExpectPropertyName();
                ConsumeCharacters(WhitespaceCharacters);
                Value propertyValue = ExpectValue();
                root[propertyName] = propertyValue;
            }

            return root;
        }

        Value ExpectValue()
        {
            ConsumeCharacters(WhitespaceCharacters);
            if (CurrentCharacter == '{')
            {
                ConsumeNext();
                ConsumeCharacters(WhitespaceCharacters);
                Value objectValue = Value.Object();
                int endlessSafe = 500;
                while (CurrentCharacter != '}')
                {
                    if (endlessSafe-- <= 0) { Debug.LogError($"Endless loop!"); break; }
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
                int endlessSafe = 500;
                int index = 0;
                while (CurrentCharacter != ']')
                {
                    if (endlessSafe-- <= 0) { Debug.LogError($"Endless loop!"); break; }
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

            bool isReference = false;
            if (CurrentCharacter == '&')
            {
                ConsumeNext();
                ConsumeCharacters(WhitespaceCharacters);
                isReference = true;
            }

            if (CurrentCharacter == '"')
            {
                ConsumeNext();
                var literalValue = ConsumeUntil("\"");
                if (isReference) return Value.Literal(literalValue).SetReference();
                return Value.Literal(literalValue);
            }

            var anyValue = ConsumeUntil('{', '\r', '\n', ' ', '\t', '\0', ',');
            if (isReference) return Value.Literal(anyValue).SetReference();
            return Value.Literal(anyValue);
        }

        string ExpectPropertyName()
        {
            ConsumeCharacters(WhitespaceCharacters);
            return ConsumeUntil(":");
        }

        void ConsumeCharacters(params char[] chars)
        {
            int endlessSafe = 500;
            while (chars.Contains(CurrentCharacter))
            {
                if (endlessSafe-- <= 0) { Debug.LogError($"Endless loop!"); break; }
                ConsumeNext();
            }
        }

        char ConsumeNext()
        {
            char substring = Content[0];
            Content = Content[1..];
            return substring;
        }

        string ConsumeUntil(string until)
        {
            int found = Content.IndexOf(until);
            if (found == -1) return "";
            return ConsumeUntil(found);
        }

        string ConsumeUntil(params char[] until)
        {
            int found = Content.IndexOfAny(until);
            if (found == -1) return "";
            return ConsumeUntil(found);
        }

        string ConsumeUntil(int until)
        {
            if (until <= 0) return "";
            string substring = Content[..until];
            Content = Content[(until + 1)..];
            return substring;
        }

        public static Value? LoadFile(string file) => !File.Exists(file) ? null : new Parser(File.ReadAllText(file)).Parse();
        public static bool TryLoadFile(string file, out Value result)
        {
            if (!File.Exists(file))
            {
                result = Value.Object();
                return false;
            }
            else
            {
                result = new Parser(File.ReadAllText(file)).Parse();
                return true;
            }
        }
    }
}
