using System;

namespace Omtt.Parser
{
    internal sealed class MarkupLiterals
    {
        internal const Char PipeSeparator = '|';
        
        internal const String OpenSymbol = "<#<";
        internal const String CloseSymbol = "#>";
        internal const String CloseTagSymbol = ">";
        internal const String AssignSymbol = "=";
        internal const String QuoteSymbol = "\"";
        internal const String SpaceSymbol = " ";
        
        internal const String OpenExpression = "{{";
        internal const String CloseExpression = "}}";
    }
}