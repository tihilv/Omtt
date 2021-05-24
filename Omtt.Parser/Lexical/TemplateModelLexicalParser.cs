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
            LexicalLiterals.OpenExpression, 
            LexicalLiterals.OpenSymbol,
            LexicalLiterals.CloseSymbol
        };
        private static readonly String[] SymbolModeLiterals = new[] {
            LexicalLiterals.AssignSymbol, 
            LexicalLiterals.CloseExpression, 
            LexicalLiterals.CloseSymbol,
            LexicalLiterals.QuoteSymbol, 
            LexicalLiterals.SpaceSymbol, 
            LexicalLiterals.CloseTagSymbol};

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

            ProcessContent(source, result, true, new String[0]);

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

                if (!String.IsNullOrEmpty(symbol) && symbol != LexicalLiterals.SpaceSymbol)
                    result.Add(new Lexem(LexemType.Symbol, symbol));

                foundExitSymbol = exitSymbols.Contains(symbol);
                
                if (symbol == LexicalLiterals.OpenExpression)
                    AddTextAndSymbol(source, result, LexicalLiterals.CloseExpression);

                else if (symbol == LexicalLiterals.QuoteSymbol)
                    AddTextAndSymbol(source, result, LexicalLiterals.QuoteSymbol);

                else if (!foundExitSymbol && symbol != LexicalLiterals.CloseSymbol && inTextMode)
                    ProcessContent(source, result, false, new [] {LexicalLiterals.CloseTagSymbol, LexicalLiterals.CloseExpression});
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