using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Omtt.Api.Exceptions;
using Omtt.Api.StatementModel;
using Omtt.Statements.Terms;

namespace Omtt.Statements
{
    public sealed class StatementParser
    {
        private static readonly ConcurrentDictionary<String, IStatement?> Cache = new ConcurrentDictionary<String, IStatement?>();
        public static IStatement? Parse(String script)
        {
            return Cache.GetOrAdd(script, s =>
            {
                StatementTokenizer statementTokenizer = new StatementTokenizer(s);
                StatementParser parser = new StatementParser(statementTokenizer.Process());
                return parser.Compile();
            });
        }

        readonly List<Token> _tokens;
        private Int32 _currentPosition;

        public StatementParser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        Boolean HasMoreTokens(int shift = 0)
        {
            return _tokens.Count > _currentPosition + shift;
        }

        Token GetToken(Int32 shift = 0)
        {
            return _tokens[_currentPosition + shift];
        }

        Token NextToken()
        {
            var result = GetToken();
            _currentPosition++;
            return result;
        }

        private readonly String[] _keywordConstant = new[] {ExpressionLiterals.TrueKeyword, ExpressionLiterals.FalseKeyword, ExpressionLiterals.NullKeyword};
        private readonly String[] _unaryOp = new[] {ExpressionLiterals.MinusKeyword, ExpressionLiterals.NotKeyword};
        private readonly String[] _op = new[] {ExpressionLiterals.PlusKeyword, ExpressionLiterals.MinusKeyword, ExpressionLiterals.MultKeyword, ExpressionLiterals.DivKeyword, ExpressionLiterals.AndKeyword, ExpressionLiterals.OrKeyword, ExpressionLiterals.LtKeyword, ExpressionLiterals.GtKeyword, ExpressionLiterals.EqKeyword};

        private IStatement? Compile()
        {
            var blockStart = HasMoreTokens() && GetToken().Value == ExpressionLiterals.BlockBegin;
            
            return CompileStatements(!blockStart);
        }

        private IStatement? CompileStatements(Boolean skipBraces = false)
        {
            if (!skipBraces)
                GoToNextToken(TokenType.Symbol, ExpressionLiterals.BlockBegin);

            List<IStatement> statements = new List<IStatement>();

            while (HasMoreTokens() && GetToken().Value != ExpressionLiterals.BlockEnd)
            {
                statements.Add(CompileStatement());
            }

            if (!skipBraces)
                GoToNextToken(TokenType.Symbol, ExpressionLiterals.BlockEnd);

            if (statements.Count > 1)
                return new MultipleStatements(statements);
            
            return statements.FirstOrDefault();
        }

        private IStatement CompileStatement()
        {
            var currentToken = GetToken();

            switch (currentToken.Value)
            {
                case ExpressionLiterals.IfKeyword:
                    return CompieIfStatement();
                case ExpressionLiterals.LetKeyword:
                    return CompileLetStatement();
                default:
                    return CompileComputeStatement();
            }
        }
        
        private IfStatement CompieIfStatement()
        {
            GoToNextToken(TokenType.Keyword, ExpressionLiterals.IfKeyword);
            GoToNextToken(TokenType.Symbol, ExpressionLiterals.OpenBracket);
            var expression = CompileExpression();
            GoToNextToken(TokenType.Symbol, ExpressionLiterals.CloseBracket);
            IStatement? trueStatements = CompileStatements();
            IStatement? falseStatements = null;
            if (HasMoreTokens() && GetToken().Type == TokenType.Keyword && GetToken().Value == ExpressionLiterals.ElseKeyword)
            {
                GoToNextToken(TokenType.Keyword, ExpressionLiterals.ElseKeyword);
                falseStatements = CompileStatements();
            }
            return new IfStatement(expression, trueStatements, falseStatements);
        }

        private LetStatement CompileLetStatement()
        {
            GoToNextToken(TokenType.Keyword, ExpressionLiterals.LetKeyword);
            var variable = CompileVariable();
            GoToNextToken(TokenType.Symbol, ExpressionLiterals.EqKeyword);
            var expression = CompileExpression();
            GoToNextToken(TokenType.Symbol, ExpressionLiterals.StatementSeparator);

            return new LetStatement(variable, expression);
        }

        private SingleStatement CompileComputeStatement()
        {
            var expression = CompileExpression();
            if (HasMoreTokens())
                GoToNextToken(TokenType.Symbol, ExpressionLiterals.StatementSeparator);

            return new SingleStatement(expression);
        }

        private Variable CompileVariable()
        {
            bool firstTime = true;
            var pairs = new List<VariablePathPair>();
            do
            {
                if (!firstTime)
                    GoToNextToken(TokenType.Symbol, ExpressionLiterals.PropertyAccessor);
                
                var name = GetToken().Value;
                GoToNextToken(TokenType.Identifier);
                Expression? arrayExpression = null;
                if (HasMoreTokens() && GetToken().Value == ExpressionLiterals.ArrayOpenBracket)
                {
                    GoToNextToken(TokenType.Symbol, ExpressionLiterals.ArrayOpenBracket);
                    arrayExpression = CompileExpression();
                    GoToNextToken(TokenType.Symbol, ExpressionLiterals.ArrayCloseBracket);
                }

                pairs.Add(new VariablePathPair(name, arrayExpression));

                firstTime = false;
                
            } while (HasMoreTokens() && GetToken().Value == ExpressionLiterals.PropertyAccessor);

            return new Variable(pairs);
        }

        private Expression CompileExpression()
        {
            var terms = new List<ITerm>();
            var operators = new List<Operator>();

            Boolean first = true;
            do
            {
                if (!first)
                {
                    operators.Add(OperatorExtensions.FromString(GetToken().Value));
                    GoToNextToken(TokenType.Symbol, _op);
                }
                first = false;

                terms.Add(CompileTerm());
            } while (HasMoreTokens() && _op.Contains(GetToken().Value));

            return new Expression(terms, operators);
        }

        private ITerm CompileTerm()
        {
            var currentToken = GetToken();

            if (currentToken.Type == TokenType.IntegerConstant)
            {
                GoToNextToken(TokenType.IntegerConstant);
                return new IntegerConstantTerm(Int32.Parse(currentToken.Value));
            }

            if (currentToken.Type == TokenType.RealConstant)
            {
                GoToNextToken(TokenType.RealConstant);
                return new DecimalConstantTerm(Decimal.Parse(currentToken.Value, CultureInfo.InvariantCulture));
            }

            
            if (currentToken.Type == TokenType.StringConstant)
            {
                GoToNextToken(TokenType.StringConstant);
                return new StringConstantTerm(currentToken.Value);
            }

            if (currentToken.Type == TokenType.DateConstant)
            {
                GoToNextToken(TokenType.DateConstant);
                return new DateConstantTerm(currentToken.Value);
            }

            if (currentToken.Type == TokenType.Keyword && _keywordConstant.Contains(currentToken.Value))
            {
                GoToNextToken(TokenType.Keyword, _keywordConstant);
                return new KeywordConstantTerm(currentToken.Value);
            }

            if (currentToken.Type == TokenType.Identifier)
            {
                if (HasMoreTokens(1) && GetToken(1).Value == ExpressionLiterals.OpenBracket)
                    return CompileSubroutineCall();
                else
                    return CompileVariable();
            }

            if (currentToken.Type == TokenType.Symbol && _unaryOp.Contains(currentToken.Value))
            {
                GoToNextToken(TokenType.Symbol, _unaryOp);
                var op = currentToken.Value;
                var term = CompileTerm();
                return new UnaryOperatorTerm(OperatorExtensions.FromString(op), term);
            }

            if (currentToken.Type == TokenType.Symbol && currentToken.Value == ExpressionLiterals.OpenBracket)
            {
                GoToNextToken(TokenType.Symbol, ExpressionLiterals.OpenBracket);
                var result = CompileExpression();
                GoToNextToken(TokenType.Symbol, ExpressionLiterals.CloseBracket);

                return result;
            }

            throw new LexicalException("Unknown term.");
        }

        private SubroutineTerm CompileSubroutineCall()
        {
            var name = GetToken().Value;
            GoToNextToken(TokenType.Identifier);

            GoToNextToken(TokenType.Symbol, ExpressionLiterals.OpenBracket);

            var arguments = CompileExpressionList();

            GoToNextToken(TokenType.Symbol, ExpressionLiterals.CloseBracket);

            return new SubroutineTerm(name, arguments);
        }

        private IEnumerable<Expression> CompileExpressionList()
        {
            var result = new List<Expression>();

            Boolean first = true;
            while (GetToken().Value != ExpressionLiterals.CloseBracket)
            {
                if (!first)
                    GoToNextToken(TokenType.Symbol, ExpressionLiterals.ParameterSeparator);
                first = false;

                result.Add(CompileExpression());
            }

            return result;
        }

        void GoToNextToken(TokenType type, params String[] values)
        {
            var token = NextToken();

            if (token.Type != type || (values.Length > 0 && !values.Contains(token.Value)))
                throw new LexicalException($"'{String.Join(ExpressionLiterals.ParameterSeparator, values)}' {type} expected.");
        }
    }
}
