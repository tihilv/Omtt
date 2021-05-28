using System;

namespace Omtt.Statements.Terms
{
    internal enum Operator: Byte
    {
        Plus,
        Minus,
        Mult,
        Div,
        And,
        Or,
        Not,
        Eq,
        Gt,
        Lt
    }

    internal static class OperatorExtensions
    {
        internal static Operator FromString(String op)
        {
            if (op == ExpressionLiterals.PlusKeyword)
                return Operator.Plus;
            if (op == ExpressionLiterals.MinusKeyword)
                return Operator.Minus;
            if (op == ExpressionLiterals.MultKeyword)
                return Operator.Mult;
            if (op == ExpressionLiterals.DivKeyword)
                return Operator.Div;
            if (op == ExpressionLiterals.AndKeyword)
                return Operator.And;
            if (op == ExpressionLiterals.OrKeyword)
                return Operator.Or;
            if (op == ExpressionLiterals.NotKeyword)
                return Operator.Not;
            if (op == ExpressionLiterals.EqKeyword)
                return Operator.Eq;
            if (op == ExpressionLiterals.GtKeyword)
                return Operator.Gt;
            if (op == ExpressionLiterals.LtKeyword)
                return Operator.Lt;
            
            throw new InvalidOperationException($"Undefined operator {op}.");
        }
    }
}