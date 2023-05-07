using System;
using System.Linq;

namespace DataUtilities.Text
{
    public class TextDeserializer
    {
        protected const int INFINITY = 1500;
        protected const char EOL = '\n';

        string Content;

        uint currentCharacterIndex = 0;
        uint currentColumn = 0;
        uint currentLine = 0;

        protected static readonly char[] SpaceCharacters = new char[] { ' ', '\t' };
        protected static readonly char[] LinebrakCharacters = new char[] { '\r', '\n' };
        protected static readonly char[] WhitespaceCharacters = new char[] { ' ', '\t', '\r', '\n' };


        public char CurrentCharacter => Content.Length == 0 ? '\0' : Content[0];

        public uint CurrentCharacterIndex => currentCharacterIndex;
        public uint CurrentColumn => currentColumn;
        public uint CurrentLine => currentLine;

        public TextDeserializer(string data) => this.Content = data ?? throw new ArgumentNullException(nameof(data));

        /// <exception cref="EndlessLoopException"></exception>
        public void ConsumeCharacters(params char[] chars)
        {
            int endlessSafe = INFINITY;
            while (chars.Contains(CurrentCharacter))
            {
                if (endlessSafe-- <= 0)
                { throw new EndlessLoopException(); }
                ConsumeNext();
            }
        }

        public char ConsumeNext()
        {
            char substring = Content[0];
            Content = Content[1..];

            currentCharacterIndex++;
            currentColumn++;
            if (substring == EOL)
            {
                currentLine++;
                currentColumn = 0;
            }

            return substring;
        }

        public string ConsumeUntil(string until)
        {
            int found = Content.IndexOf(until);
            if (found == -1) return "";
            return ConsumeUntil(found);
        }

        public string ConsumeUntil(params char[] until)
        {
            int found = Content.IndexOfAny(until);
            if (found == -1) return "";
            return ConsumeUntil(found);
        }

        public string ConsumeUntil(int until)
        {
            if (until <= 0) return "";
            string substring = Content[..until];
            Content = Content[(until)..];

            currentCharacterIndex += (uint)substring.Length;
            currentColumn++;
            for (int i = 0; i < substring.Length; i++)
            {
                if (substring[i] == EOL)
                {
                    currentLine += (uint)substring.Length;
                    currentColumn = 0;
                }
            }

            return substring;
        }
    }
}
