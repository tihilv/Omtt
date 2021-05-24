using System;

namespace Omtt.Statements
{
    internal sealed class LexicalLiterals
    {
        internal const String BlockBegin = "{";
        internal const String BlockEnd = "}";
        
        internal const Char StringBoundary = '\'';
        internal const Char DateBoundary = '%';
        internal const Char Space = ' ';

        internal const String IfKeyword = "if";
        internal const String ElseKeyword = "else";
        internal const String LetKeyword = "let";
        internal const String TrueKeyword = "true";
        internal const String FalseKeyword = "false";
        internal const String NullKeyword = "null";
    }
}