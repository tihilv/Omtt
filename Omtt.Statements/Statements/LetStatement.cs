using System;
using Omtt.Api.StatementModel;
using Omtt.Statements.Terms;

namespace Omtt.Statements
{
    internal sealed class LetStatement : IStatement
    {
        private readonly Variable _variable;
        private readonly Expression _expression;

        internal LetStatement(Variable variable, Expression expression)
        {
            _variable = variable;
            _expression = expression;
        }

        public Object? Execute(IStatementContext context)
        {
            var value = _expression.Calculate(context);

            _variable.Set(context, value);

            return value;
        }
    }
}