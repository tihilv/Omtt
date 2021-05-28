using System;

namespace Omtt.Statements
{
    internal sealed class ExpressionLiterals
    {
        internal const String BlockBegin = "{";
        internal const String BlockEnd = "}";

        internal const String OpenBracket = "(";
        internal const String CloseBracket = ")";

        internal const String ArrayOpenBracket = "[";
        internal const String ArrayCloseBracket = "]";

        internal const String PropertyAccessor = ".";
        internal const String ParameterSeparator = ",";
        internal const String StatementSeparator = ";";
        internal const Char NumberDot = '.';
        internal const Char StringBoundary = '\'';
        internal const Char DateBoundary = '%';
        internal const Char Space = ' ';

        internal const String IfKeyword = "if";
        internal const String ElseKeyword = "else";
        internal const String LetKeyword = "let";
        internal const String TrueKeyword = "true";
        internal const String FalseKeyword = "false";
        internal const String NullKeyword = "null";
        
        internal const String PlusKeyword = "+";
        internal const String MinusKeyword = "-";
        internal const String MultKeyword = "*";
        internal const String DivKeyword = "/";
        internal const String AndKeyword = "&";
        internal const String OrKeyword = "|";
        internal const String NotKeyword = "!";
        internal const String EqKeyword = "=";
        internal const String LtKeyword = "<";
        internal const String GtKeyword = ">";
    }
}