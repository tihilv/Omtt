using System;
using Omtt.Api.StatementModel;
using Omtt.Statements.Terms;

namespace Omtt.Statements
{
    internal sealed class SingleStatement : IStatement
    {
        private readonly Expression _expression;

        internal SingleStatement(Expression expression)
        {
            _expression = expression;
        }

        public Object? Execute(IStatementContext context)
        {
            return _expression.Calculate(context);
        }
    }
}