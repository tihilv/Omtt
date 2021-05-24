using System;
using System.Runtime.CompilerServices;
using Omtt.Api.StatementModel;
using Omtt.Statements.Terms;

[assembly: InternalsVisibleTo("Omtt.Tests")]
namespace Omtt.Statements
{
    internal sealed class IfStatement : IStatement
    {
        private readonly Expression _expression;
        private readonly IStatement? _trueStatements;
        private readonly IStatement? _falseStatements;

        internal IfStatement(Expression expression, IStatement? trueStatements, IStatement? falseStatements)
        {
            _expression = expression;
            _trueStatements = trueStatements;
            _falseStatements = falseStatements;
        }

        public Object? Execute(IStatementContext context)
        {
            var result = _expression.Calculate(context);
            if (result is Boolean b && b)
                return _trueStatements?.Execute(context);

            return _falseStatements?.Execute(context);
        }
    }
}