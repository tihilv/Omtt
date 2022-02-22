using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Omtt.Parser.Lexical
{
    internal sealed class TemplateModelLexicalParser
    {
        private static readonly String[] TextModeLiterals = new[]
        {
            MarkupLiterals.OpenExpression, 
            MarkupLiterals.OpenSymbol,
            MarkupLiterals.CloseSymbol
        };
        private static readonly String[] SymbolModeLiterals = new[] {
            MarkupLiterals.AssignSymbol, 
            MarkupLiterals.CloseExpression, 
            MarkupLiterals.CloseSymbol,
            MarkupLiterals.QuoteSymbol, 
            MarkupLiterals.SpaceSymbol, 
            MarkupLiterals.CloseTagSymbol};

        private static readonly String[] CloseTagLiterals = new[]
        {
            MarkupLiterals.CloseTagSymbol,
            MarkupLiterals.CloseExpression
        };

        internal async Task<List<Lexem>> ParseAsync(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                var content = await streamReader.ReadToEndAsync();
                return Parse(content);
            }
        }
        
        internal List<Lexem> Parse(string str)
        {
            var source = new ParsingSource(str);
                
            List<Lexem> result = new List<Lexem>();

            ProcessContent(source, result, true, Array.Empty<String>());

            return result;
        }

        private void ProcessContent(ParsingSource source, List<Lexem> result, bool inTextMode, String[] exitSymbols)
        {
            var symbols = (inTextMode?TextModeLiterals:SymbolModeLiterals).Union(exitSymbols).ToArray();
            
            Boolean foundExitSymbol = false;
            while (source.Any() && !foundExitSymbol)
            {
                var (text, symbol) = source.FindNext(symbols);
                if (!String.IsNullOrEmpty(text))
                    result.Add(new Lexem(LexemType.Text, text));

                if (!String.IsNullOrEmpty(symbol) && symbol != MarkupLiterals.SpaceSymbol)
                    result.Add(new Lexem(LexemType.Symbol, symbol));

                foundExitSymbol = exitSymbols.Contains(symbol);
                
                if (symbol == MarkupLiterals.OpenExpression)
                    AddTextAndSymbol(source, result, MarkupLiterals.CloseExpression);

                else if (symbol == MarkupLiterals.QuoteSymbol)
                    AddTextAndSymbol(source, result, MarkupLiterals.QuoteSymbol);

                else if (!foundExitSymbol && symbol != MarkupLiterals.CloseSymbol && inTextMode)
                    ProcessContent(source, result, false, CloseTagLiterals);
            }
        }

        private void AddTextAndSymbol(ParsingSource source, List<Lexem> result, params String[] symbols)
        {
            var (exprText, exprSymbol) = source.FindNext(symbols);
            result.Add(new Lexem(LexemType.Text, exprText));
            result.Add(new Lexem(LexemType.Symbol, exprSymbol));
        }
    }
}