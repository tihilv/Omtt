using System;
using Omtt.Api.StatementModel;

namespace Omtt.Statements.Terms
{
    internal sealed class StringConstantTerm : ITerm
    {
        private readonly String _value;

        internal StringConstantTerm(String value)
        {
            _value = value;
        }

        public Object Calculate(IStatementContext context)
        {
            return _value;
        }
    }
}