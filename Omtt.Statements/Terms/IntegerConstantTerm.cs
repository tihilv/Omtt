using System;
using Omtt.Api.StatementModel;

namespace Omtt.Statements.Terms
{
    internal sealed class IntegerConstantTerm : ITerm
    {
        private readonly Int64 _value;

        internal IntegerConstantTerm(Int64 value)
        {
            _value = value;
        }

        public Object Calculate(IStatementContext context)
        {
            return _value;
        }
    }
}