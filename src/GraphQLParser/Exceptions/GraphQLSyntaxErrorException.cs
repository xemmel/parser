using System;
using System.Linq;
using System.Text;

namespace GraphQLParser.Exceptions
{
    public class GraphQLSyntaxErrorException : Exception
    {
        public string Description { get; private set; }

        public int Line { get; private set; }

        public int Column { get; private set; }

        public GraphQLSyntaxErrorException(string description, ISource source, int location)
            : this(description, source, new Location(source, location))
        {
        }

        private GraphQLSyntaxErrorException(string description, ISource source, Location location)
            : base(ComposeMessage(description, source, location))
        {
            Description = description;
            Line = location.Line;
            Column = location.Column;
        }

        private static string ComposeMessage(string description, ISource source, Location location)
        {
            return $"Syntax Error GraphQL ({location.Line}:{location.Column}) {description}" +
                "\n" + HighlightSourceAtLocation(source, location);
        }

        private static string HighlightSourceAtLocation(ISource source, Location location)
        {
            int line = location.Line;
            string prevLineNum = (line - 1).ToString();
            string lineNum = line.ToString();
            string nextLineNum = (line + 1).ToString();
            int padLen = nextLineNum.Length;
            string[] lines = source.Body
                .Split(new string[] { "\n" }, StringSplitOptions.None)
                .Select(e => ReplaceWithUnicodeRepresentation(e))
                .ToArray();

            return
                (line >= 2 ? LeftPad(padLen, prevLineNum) + ": " + lines[line - 2] + "\n" : string.Empty) +
                LeftPad(padLen, lineNum) + ": " + lines[line - 1] + "\n" +
                LeftPad(1 + padLen + location.Column, string.Empty) + "^" + "\n" +
                (line < lines.Length ? LeftPad(padLen, nextLineNum) + ": " + lines[line] + "\n" : string.Empty);
        }

        private static string LeftPad(int length, string str)
        {
            string pad = string.Empty;

            for (int i = 0; i < length - str.Length; i++)
                pad += " ";

            return pad + str;
        }

        private static string ReplaceWithUnicodeRepresentation(string str)
        {
            if (!HasReplacementCharacter(str))
                return str;

            var buffer = new StringBuilder(str.Length);

            foreach (char code in str)
            {
                if (IsReplacementCharacter(code))
                {
                    buffer.Append(GetUnicodeRepresentation(code));
                }
                else
                {
                    buffer.Append(code);
                }
            }

            return buffer.ToString();
        }

        private static bool HasReplacementCharacter(string str)
        {
            foreach (char code in str)
            {
                if (IsReplacementCharacter(code))
                    return true;
            }

            return false;
        }

        private static bool IsReplacementCharacter(char code) => code < 0x0020 && code != 0x0009 && code != 0x000A && code != 0x000D;

        private static string GetUnicodeRepresentation(char code) => code switch
        {
            '\0' => "\\u0000",
            _ => "\\u" + ((int)code).ToString("D4")
        };
    }
}
