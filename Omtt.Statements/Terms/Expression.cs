using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Omtt.Api.DataModel;
using Omtt.Api.StatementModel;

namespace Omtt.Statements.Terms
{
    internal sealed class Expression: ITerm
    {
        private readonly ITerm[] _terms;
        private readonly Operator[] _operators;

        internal Expression(IEnumerable<ITerm> terms, IEnumerable<Operator> operators)
        {
            _terms = terms.ToArray();
            _operators = operators.ToArray();
        }

        public Object? Calculate(IStatementContext context)
        {
            var current = _terms[0].Calculate(context);

            for (Int32 i = 0; i < _operators.Length; i++)
            {
                var second = _terms[i + 1].Calculate(context);
                var op = _operators[i];

                if (current is SourceScheme || second is SourceScheme)
                    continue;

                switch (op)
                {
                    case Operator.Plus:
                        current = ProcessPlus(current, second);
                        break;
                    case Operator.Minus:
                        current = ProcessMinus(current, second);
                        break;
                    case Operator.Mult:
                        current = ProcessMult(current, second);
                        break;
                    case Operator.Div:
                        current = ProcessDiv(current, second);
                        break;
                    case Operator.And:
                        current = ProcessAnd(current, second, op);
                        break;
                    case Operator.Or:
                        current = ProcessOr(current, second, op);
                        break;
                    case Operator.Eq:
                        current = ProcessEq(current, second);
                        break;
                    case Operator.Gt:
                        current = ProcessGt(current, second);
                        break;
                    case Operator.Lt:
                        current = ProcessLt(current, second);
                        break;
                    default:
                        throw new InvalidOperationException($"Undefined operator {op}.");
                }
            }

            return current;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Boolean ProcessGt(Object? current, Object? second)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));

            var comparable = (IComparable)current;
            var secondConverted = Convert.ChangeType(second, current.GetType());
            return comparable.CompareTo(secondConverted) > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Boolean ProcessLt(Object? current, Object? second)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));
            
            var comparable = (IComparable)current;
            var secondConverted = Convert.ChangeType(second, current.GetType());
            return comparable.CompareTo(secondConverted) < 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Object ProcessEq(Object? current, Object? second)
        {
            if (current == null && second == null)
                return true;

            if (current is IComparable comparable)
                return comparable.CompareTo(second) == 0;
            
            return current?.ToString() == second?.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Object ProcessOr(Object? current, Object? second, Operator op)
        {
            if (current is Boolean a && second is Boolean b)
                return a | b;

            throw new InvalidOperationException($"Undefined operator {op} for type {current?.GetType()}");
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Object ProcessAnd(Object? current, Object? second, Operator op)
        {
            if (current is Boolean a && second is Boolean b)
                return a & b;

            throw new InvalidOperationException($"Undefined operator {op} for type {current?.GetType()}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Object ProcessDiv(Object? current, Object? second)
        {
            if (current is Int32 int32Value)
                return int32Value / Convert.ToInt32(second);
            if (current is Int64 int64Value)
                return int64Value / Convert.ToInt64(second);
            if (current is UInt64 uint64Value)
                return uint64Value / Convert.ToUInt64(second);
            if (current is Double doubleValue)
                return doubleValue / Convert.ToDouble(second);
            if (current is Decimal decimalValue)
                return decimalValue / Convert.ToDecimal(second);
            if (current is Byte byteValue)
                return byteValue / Convert.ToByte(second);
            return ProcessCommonOperation(current, second, "op_Division");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Object ProcessMult(Object? current, Object? second)
        {
            if (current is Int32 int32Value)
                return int32Value * Convert.ToInt32(second);
            if (current is Int64 int64Value)
                return int64Value * Convert.ToInt64(second);
            if (current is UInt64 uint64Value)
                return uint64Value * Convert.ToUInt64(second);
            if (current is Double doubleValue)
                return doubleValue * Convert.ToDouble(second);
            if (current is Decimal decimalValue)
                return decimalValue * Convert.ToDecimal(second);
            if (current is Byte byteValue)
                return byteValue * Convert.ToByte(second);
            return ProcessCommonOperation(current, second, "op_Multiply");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Object ProcessMinus(Object? current, Object? second)
        {
            if (current is Int32 int32Value)
                return int32Value - Convert.ToInt32(second);
            if (current is Int64 int64Value)
                return int64Value - Convert.ToInt64(second);
            if (current is UInt64 uint64Value)
                return uint64Value - Convert.ToUInt64(second);
            if (current is Double doubleValue)
                return doubleValue - Convert.ToDouble(second);
            if (current is Decimal decimalValue)
                return decimalValue - Convert.ToDecimal(second);
            if (current is Byte byteValue)
                return byteValue - Convert.ToByte(second);
            return ProcessCommonOperation(current, second, "op_Subtraction");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Object ProcessPlus(Object? current, Object? second)
        {
            if (current is String strValue)
                return strValue + second;
            if (current is Int32 int32Value)
                return int32Value + Convert.ToInt32(second);
            if (current is Int64 int64Value)
                return int64Value + Convert.ToInt64(second);
            if (current is UInt64 uint64Value)
                return uint64Value + Convert.ToUInt64(second);
            if (current is Double doubleValue)
                return doubleValue + Convert.ToDouble(second);
            if (current is Decimal decimalValue)
                return decimalValue + Convert.ToDecimal(second);
            if (current is Byte byteValue)
                return byteValue + Convert.ToByte(second);
            return ProcessCommonOperation(current, second, "op_Addition");
        }

        private static Object ProcessCommonOperation(Object? current, Object? second, String op)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));

            var method = current.GetType().GetMethod(op);
            if (method == null)
                throw new InvalidOperationException($"Undefined operator {op} for type {current.GetType()}.");

            current = method.Invoke(null, new[] {current, second});
            return current;
        }
    }
}