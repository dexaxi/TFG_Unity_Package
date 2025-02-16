using System;
using System.Linq;

namespace DUJAL.Systems.Utils
{
    /// <summary>
    /// Utility class to efficiently remove whitspaces from node names. Got if from https://stackoverflow.com/a/37368176, benchmarks: https://stackoverflow.com/a/37347881
    /// </summary>
    public static class CharacterRemover
    {
        public static bool IsWhitespace(this char character)
        {
            switch (character)
            {
                case '\u0020':
                case '\u00A0':
                case '\u1680':
                case '\u2000':
                case '\u2001':
                case '\u2002':
                case '\u2003':
                case '\u2004':
                case '\u2005':
                case '\u2006':
                case '\u2007':
                case '\u2008':
                case '\u2009':
                case '\u200A':
                case '\u202F':
                case '\u205F':
                case '\u3000':
                case '\u2028':
                case '\u2029':
                case '\u0009':
                case '\u000A':
                case '\u000B':
                case '\u000C':
                case '\u000D':
                case '\u0085':
                    {
                        return true;
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        public static string RemoveWhitespaces(this string text)
        {
            int textLength = text.Length;

            char[] textCharacters = text.ToCharArray();

            int currentWhitespacelessTextLength = 0;

            for (int currentCharacterIdx = 0; currentCharacterIdx < textLength; ++currentCharacterIdx)
            {
                char currentTextCharacter = textCharacters[currentCharacterIdx];

                if (currentTextCharacter.IsWhitespace())
                {
                    continue;
                }

                textCharacters[currentWhitespacelessTextLength++] = currentTextCharacter;
            }

            return new string(textCharacters, 0, currentWhitespacelessTextLength);
        }

        public static string RemoveSpecialCharacters(this string text)
        {
            int textLength = text.Length;

            char[] textCharacters = text.ToCharArray();

            int currentWhitespacelessTextLength = 0;

            for (int currentCharacterIdx = 0; currentCharacterIdx < textLength; ++currentCharacterIdx)
            {
                char currentTextCharacter = textCharacters[currentCharacterIdx];

                if (!char.IsLetterOrDigit(currentTextCharacter) && !currentTextCharacter.IsWhitespace())
                {
                    continue;
                }

                textCharacters[currentWhitespacelessTextLength++] = currentTextCharacter;
            }

            return new string(textCharacters, 0, currentWhitespacelessTextLength);
        }
    }
}
