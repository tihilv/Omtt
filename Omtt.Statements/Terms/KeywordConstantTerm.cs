using System;
using Omtt.Api.StatementModel;

namespace Omtt.Statements.Terms
{
    internal sealed class KeywordConstantTerm: ITerm
    {
        private Object? _value;
        internal KeywordConstantTerm(String identifier)
        {
            if (identifier == "true")
                _value = true;
            else if (identifier == "false")
                _value = false;
            else if (identifier == "null")
                _value = null;
        }

        public Object? Calculate(IStatementContext context)
        {
            return _value;
        }
    }
}