using System;

namespace Omtt.Parser.Lexical
{
    internal sealed class ParsingSource
    {
        private Int32 _index;
        private readonly String _content;
        
        internal ParsingSource(String content)
        {
            _content = content;
            _index = 0;
        }

        private Int32 FindIndex(string pattern)
        {
            return _content.IndexOf(pattern, _index, StringComparison.Ordinal);
        }

        private String GetSubstring(int endIndex)
        {
            return _content.Substring(_index, (endIndex - _index));
        }
       
        internal Boolean Any()
        {
            return _content != null && _content.Length > _index;
        }

        private (String text, String symbol) DetectNext(params String[] symbols)
        {
            Int32 bestIndex = _content.Length;
            var foundSymbol = String.Empty;
            
            foreach (var symbol in symbols)
            {
                var index = FindIndex(symbol);
                if (index >= 0 && index < bestIndex)
                {
                    bestIndex = index;
                    foundSymbol = symbol;
                }
            }

            var text = GetSubstring(bestIndex);
            return (text, foundSymbol);
        }
        
        internal (String text, String symbol) FindNext(params String[] symbols)
        {
            var result = DetectNext(symbols);
            _index += result.text.Length + result.symbol.Length;
            return result;
        }

        internal String FindNextText(params String[] symbols)
        {
            var result = DetectNext(symbols);
            _index += result.text.Length;
            return result.text;
        }
    }
}