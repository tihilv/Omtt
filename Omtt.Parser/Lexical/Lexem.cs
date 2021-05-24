using System;

namespace Omtt.Parser.Lexical
{
    internal struct Lexem
    {
        internal readonly LexemType Type;
        internal readonly String Content;

        internal Lexem(LexemType type, String content)
        {
            Type = type;
            Content = content;
        }

        public override string ToString()
        {
            return $"{Type}: {Content}";
        }
    }
}