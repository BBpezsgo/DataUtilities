using DataUtilities.ReadableFileFormat;

using System;
using System.IO;
using System.Linq;

#if UNITY
using Debug = UnityEngine.Debug;
#endif

namespace DataUtilities.Json
{

    [Serializable]
    public class JsonSyntaxException : Exception
    {
        public JsonSyntaxException() { }
        public JsonSyntaxException(string message) : base(message) { }
        public JsonSyntaxException(string message, Exception inner) : base(message, inner) { }
        protected JsonSyntaxException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class Parser : Text.TextDeserializer
    {
        public static Value Parse(string data) => new Parser(data).ExpectRootValue();

        public Parser(string data) : base(data) { }

        Value ExpectRootValue() => ExpectValue();

        Value ExpectValue()
        {
            ConsumeCharacters(WhitespaceCharacters);
            if (CurrentCharacter == '{')
            {
                ConsumeNext();
                ConsumeCharacters(WhitespaceCharacters);
                Value objectValue = Value.Object();
                int endlessSafe = INFINITY;
                while (CurrentCharacter != '}')
                {
                    if (endlessSafe-- <= 0)
                    { throw new EndlessLoopException(); }
                    ConsumeCharacters(WhitespaceCharacters);
                    string propertyName = ExpectPropertyName();
                    ConsumeCharacters(WhitespaceCharacters);
                    if (CurrentCharacter == ':') ConsumeNext();
                    else throw new JsonSyntaxException($"Expected ':' after property name, got '{CurrentCharacter}'");
                    ConsumeCharacters(WhitespaceCharacters);
                    Value? propertyValue = ExpectValue();
                    if (propertyValue.HasValue)
                    { objectValue[propertyName] = propertyValue.Value; }
                    else
                    { Debug.LogError($"Property \"{propertyName}\" does not have a value"); }
                    ConsumeCharacters(WhitespaceCharacters);
                    if (CurrentCharacter == ',') continue;
                    else break;
                }

                if (CurrentCharacter == '}') ConsumeNext();
                else throw new JsonSyntaxException($"Expected '}}' after object value, got '{CurrentCharacter}'");

                return objectValue;
            }
            if (CurrentCharacter == '[')
            {
                ConsumeNext();
                ConsumeCharacters(WhitespaceCharacters);
                Value listValue = Value.Object();
                int endlessSafe = INFINITY;
                int index = 0;
                while (CurrentCharacter != ']')
                {
                    if (endlessSafe-- <= 0)
                    { throw new EndlessLoopException(); }
                    ConsumeCharacters(WhitespaceCharacters);
                    Value? listItemValue = ExpectValue();
                    if (listItemValue.HasValue)
                    { listValue[index++] = listItemValue.Value; }
                    else
                    { Debug.LogError($"List has a null item"); }
                    ConsumeCharacters(WhitespaceCharacters);
                    if (CurrentCharacter == ',') continue;
                    else break;
                }

                if (CurrentCharacter == ']') ConsumeNext();
                else throw new JsonSyntaxException($"Expected ']' after list value, got '{CurrentCharacter}'");

                listValue["Length"] = Value.Literal(index.ToString());
                return listValue;
            }

            if (CurrentCharacter == '"')
            {
                ConsumeNext();
                int endlessSafe = INFINITY;
                string literalValue = string.Empty;
                while (CurrentCharacter != '"')
                {
                    if (endlessSafe-- <= 0)
                    { throw new EndlessLoopException(); }
                    if (CurrentCharacter == '\\')
                    { ConsumeNext(); }
                    literalValue += ConsumeNext();
                }
                ConsumeNext();
                return Value.Literal(literalValue);
            }

            Value? potentialNumber = ExpectNumber();
            if (potentialNumber.HasValue)
            { return potentialNumber.Value; }

            throw new JsonSyntaxException($"Unexpected character '{CurrentCharacter}'");
        }

        Value? ExpectNumber()
        {
            ConsumeCharacters(WhitespaceCharacters);
            char[] digits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            if (CurrentCharacter != '-' && !digits.Contains(CurrentCharacter)) return null;
            bool isNegative = (CurrentCharacter == '-');
            if (isNegative) ConsumeNext();
            bool isReal = false;
            string raw = string.Empty;
            int endlessSafe = 100;
            while (digits.Contains(CurrentCharacter) || CurrentCharacter == '.' || CurrentCharacter == 'e')
            {
                if (endlessSafe-- <= 0) throw new EndlessLoopException();
                if (CurrentCharacter == '.')
                {
                    if (raw.Contains(CurrentCharacter)) throw new JsonSyntaxException($"Unexpected character '{CurrentCharacter}'");
                    raw += CurrentCharacter;
                    ConsumeNext();
                    isReal = true;
                    continue;
                }
                if (CurrentCharacter == 'e')
                {
                    if (raw.Contains(CurrentCharacter)) throw new JsonSyntaxException($"Unexpected character '{CurrentCharacter}'");
                    raw += CurrentCharacter;
                    ConsumeNext();
                    isReal = true;
                    continue;
                }
                raw += CurrentCharacter;
                ConsumeNext();
            }
            if (isNegative) raw = '-' + raw;
            if (isReal)
            {
                if (float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result)) return Value.Literal(result);
            }
            else
            {
                if (int.TryParse(raw, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int result)) return Value.Literal(result);
            }
            throw new JsonSyntaxException($"Failed to parse '{raw}' to float");
        }

        string ExpectPropertyName()
        {
            ConsumeCharacters(WhitespaceCharacters);
            if (CurrentCharacter == '"')
            {
                ConsumeNext();
                int endlessSafe = INFINITY;
                string propertyName = string.Empty;
                while (CurrentCharacter != '"')
                {
                    if (endlessSafe-- <= 0)
                    { throw new EndlessLoopException(); }
                    if (CurrentCharacter == '\\')
                    { ConsumeNext(); }
                    propertyName += ConsumeNext();
                }
                ConsumeNext();
                return propertyName;
            }

            throw new JsonSyntaxException($"Unexpected character '{CurrentCharacter}'; expected property name");
        }

        public static bool TryLoadFile(string file, out Value result)
        {
            if (!File.Exists(file))
            {
                result = Value.Object();
                return false;
            }
            else
            {
                result = new Parser(File.ReadAllText(file)).ExpectRootValue();
                return true;
            }
        }
    }
}
