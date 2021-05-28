using System;
using System.Collections.Generic;
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

        private static readonly HashSet<Char> _symbols = new HashSet<Char>
        {
            ExpressionLiterals.BlockBegin[0],
            ExpressionLiterals.BlockEnd[0],
            ExpressionLiterals.OpenBracket[0],
            ExpressionLiterals.CloseBracket[0],
            ExpressionLiterals.ArrayOpenBracket[0],
            ExpressionLiterals.ArrayCloseBracket[0],
            ExpressionLiterals.PropertyAccessor[0], 
            ExpressionLiterals.ParameterSeparator[0], 
            ExpressionLiterals.StatementSeparator[0], 
            ExpressionLiterals.PlusKeyword[0], 
            ExpressionLiterals.MinusKeyword[0], 
            ExpressionLiterals.MultKeyword[0], 
            ExpressionLiterals.DivKeyword[0],
            ExpressionLiterals.AndKeyword[0],
            ExpressionLiterals.OrKeyword[0],
            ExpressionLiterals.LtKeyword[0],
            ExpressionLiterals.GtKeyword[0],
            ExpressionLiterals.EqKeyword[0],
            ExpressionLiterals.NotKeyword[0]
        };

        Boolean IsSymbol(Char c)
        {
            return _symbols.Contains(c);
        }

        private static readonly HashSet<String> _keywords = new HashSet<String>
        {
            ExpressionLiterals.TrueKeyword,
            ExpressionLiterals.FalseKeyword, 
            ExpressionLiterals.NullKeyword,
            ExpressionLiterals.IfKeyword, 
            ExpressionLiterals.ElseKeyword, 
            ExpressionLiterals.LetKeyword
        };

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
                    if (c == ExpressionLiterals.StringBoundary)
                        ProcessCurrentTokenIfExists(ref currentType, ref currentContent, result);
                    else
                        currentContent += c;
                }

                else if (currentType == TokenType.DateConstant)
                {
                    if (c == ExpressionLiterals.DateBoundary)
                        ProcessCurrentTokenIfExists(ref currentType, ref currentContent, result);
                    else
                        currentContent += c;
                }

                else if (currentType == TokenType.IntegerConstant && c == ExpressionLiterals.NumberDot)
                {
                    currentType = TokenType.RealConstant;
                    currentContent += c;
                }
                
                else if (currentType == TokenType.RealConstant && c == ExpressionLiterals.NumberDot)
                {
                    throw new LexicalException("Wrong format");
                }

                else if (IsSymbol(c))
                {
                    ProcessCurrentTokenIfExists(ref currentType, ref currentContent, result);

                    result.Add(new Token(c.ToString(), TokenType.Symbol));
                }

                else if (c == ExpressionLiterals.Space)
                {
                    ProcessCurrentTokenIfExists(ref currentType, ref currentContent, result);
                }
                else
                {
                    if (currentType == null)
                    {
                        if (c == ExpressionLiterals.StringBoundary)
                        {
                            currentType = TokenType.StringConstant;
                        }
                        else if (c == ExpressionLiterals.DateBoundary)
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
