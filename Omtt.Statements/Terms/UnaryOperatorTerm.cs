using System;
using Omtt.Api.DataModel;
using Omtt.Api.StatementModel;

namespace Omtt.Statements.Terms
{
    internal sealed class UnaryOperatorTerm : ITerm
    {
        private readonly String _op;
        private readonly ITerm _term;

        internal UnaryOperatorTerm(String op, ITerm term)
        {
            _op = op;
            _term = term;
        }


        public Object Calculate(IStatementContext context)
        {
            var termValue = _term.Calculate(context);

            if (termValue is SourceScheme)
                return termValue;

            if (_op == "!" && termValue is Boolean booleanValue)
                return !booleanValue;

            if (_op == "-" && termValue != null)
            {
                if (termValue is Int32 intValue)
                    return -intValue;
                if (termValue is Int64 int64Value)
                    return -int64Value;
                if (termValue is Double doubleValue)
                    return -doubleValue;
                if (termValue is Decimal decimalValue)
                    return -decimalValue;
                if (termValue is Byte byteValue)
                    return -byteValue;
                
                var method = termValue.GetType().GetMethod("op_Subtraction");
                if (method != null)
                    return method.Invoke(null, new[] {Activator.CreateInstance(termValue.GetType()), termValue});
            }

            throw new InvalidOperationException($"Unknown operation {_op} for the given operands.");
        }
    }
}
