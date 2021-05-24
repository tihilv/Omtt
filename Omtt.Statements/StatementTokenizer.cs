using System;
using System.Collections.Generic;
using System.Linq;
using Omtt.Api.Exceptions;

namespace Omtt.Statements
{
    internal sealed class StatementTokenizer
    {
        readonly String _input;

        internal StatementTokenizer(String input)
        {
            _input = input.Replace("\r\n", " ");
        }

        private readonly Char[] _symbols = new[] {'{','}','(', ')', '[', ']', '.', ',', ';', '+', '-', '*', '/', '&', '|', '<', '>', '=', '!'};

        Boolean IsSymbol(Char c)
        {
            return _symbols.Contains(c);
        }

        private readonly String[] _keywords = new[] {LexicalLiterals.TrueKeyword, LexicalLiterals.FalseKeyword, LexicalLiterals.NullKeyword, LexicalLiterals.IfKeyword, LexicalLiterals.ElseKeyword, LexicalLiterals.LetKeyword};

        Boolean IsKeyword(String identifier)
        {
            return _keywords.Contains(identifier);
        }

        internal List<Token> Process()
        {
            var result = new List<Token>();

            TokenType? currentType = null;
            var currentContent = String.Empty;

            foreach (var c in _input)
            {
                if (currentType == TokenType.StringConstant)
                {
                    if (c == LexicalLiterals.StringBoundary)
                        ProcessCurrentTokenIfExists(ref currentType, ref currentContent, result);
                    else
                        currentContent += c;
                }

                else if (currentType == TokenType.DateConstant)
                {
                    if (c == LexicalLiterals.DateBoundary)
                        ProcessCurrentTokenIfExists(ref currentType, ref currentContent, result);
                    else
                        currentContent += c;
                }

                else if (currentType == TokenType.IntegerConstant && c == '.')
                {
                    currentType = TokenType.RealConstant;
                    currentContent += c;
                }
                
                else if (currentType == TokenType.RealConstant && c == '.')
                {
                    throw new LexicalException("Wrong format");
                }

                else if (IsSymbol(c))
                {
                    ProcessCurrentTokenIfExists(ref currentType, ref currentContent, result);

                    result.Add(new Token(c.ToString(), TokenType.Symbol));
                }

                else if (c == LexicalLiterals.Space)
                {
                    ProcessCurrentTokenIfExists(ref currentType, ref currentContent, result);
                }
                else
                {
                    if (currentType == null)
                    {
                        if (c == LexicalLiterals.StringBoundary)
                        {
                            currentType = TokenType.StringConstant;
                        }
                        else if (c == LexicalLiterals.DateBoundary)
                        {
                            currentType = TokenType.DateConstant;
                        }
                        else
                        {
                            if (c >= '0' && c <= '9')
                                currentType = TokenType.IntegerConstant;
                            else
                                currentType = TokenType.Identifier;

                            currentContent += c;
                        }
                    }
                    else
                        currentContent += c;
                }
            }

            ProcessCurrentTokenIfExists(ref currentType, ref currentContent, result);

            return result;
        }

        private void ProcessCurrentTokenIfExists(ref TokenType? currentType, ref String currentContent, List<Token> result)
        {
            if (!String.IsNullOrEmpty(currentContent) && currentType != null)
            {
                if (currentType == TokenType.Identifier && IsKeyword(currentContent))
                    currentType = TokenType.Keyword;

                result.Add(new Token(currentContent, currentType.Value));
            }

            currentContent = String.Empty;
            currentType = null;
        }
    }


    public enum TokenType
    {
        Keyword,
        Symbol,
        IntegerConstant,
        RealConstant,
        StringConstant,
        DateConstant,
        Identifier
    }

    public struct Token
    {
        public readonly String Value;
        public readonly TokenType Type;

        public Token(String value, TokenType type)
        {
            Value = value;
            Type = type;
        }
    }
}
