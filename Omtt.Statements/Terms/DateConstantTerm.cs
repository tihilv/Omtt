using System;
using Omtt.Api.StatementModel;

namespace Omtt.Statements.Terms
{
    internal sealed class DateConstantTerm : ITerm
    {
        private readonly DateTime _value;

        internal DateConstantTerm(String value)
        {
            _value = DateTime.Parse(value);
        }

        public Object Calculate(IStatementContext context)
        {
            return _value;
        }
    }
}