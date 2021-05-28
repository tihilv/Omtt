using System;
using Omtt.Api.StatementModel;

namespace Omtt.Statements.Terms
{
    internal sealed class KeywordConstantTerm: ITerm
    {
        private readonly Object? _value;
        internal KeywordConstantTerm(String identifier)
        {
            if (identifier == ExpressionLiterals.TrueKeyword)
                _value = true;
            else if (identifier == ExpressionLiterals.FalseKeyword)
                _value = false;
            else if (identifier == ExpressionLiterals.NullKeyword)
                _value = null;
        }

        public Object? Calculate(IStatementContext context)
        {
            return _value;
        }
    }
}