using System;
using Omtt.Api.StatementModel;

namespace Omtt.Statements.Terms
{
    internal sealed class DecimalConstantTerm : ITerm
    {
        private readonly Decimal _value;

        internal DecimalConstantTerm(Decimal value)
        {
            _value = value;
        }

        public Object Calculate(IStatementContext context)
        {
            return _value;
        }
    }
}