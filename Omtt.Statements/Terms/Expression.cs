using System;
using System.Collections.Generic;
using System.Linq;
using Omtt.Api.DataModel;
using Omtt.Api.StatementModel;

namespace Omtt.Statements.Terms
{
    internal sealed class Expression: ITerm
    {
        private readonly ITerm[] _terms;
        private readonly String[] _operators;

        internal Expression(IEnumerable<ITerm> terms, IEnumerable<String> operators)
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
                
                if (op == "+")
                {
                    current = ProcessPlus(current, second);
                }

                else if (op == "-")
                {
                    current = ProcessMinus(current, second);
                }

                else if (op == "*")
                {
                    current = ProcessMult(current, second);
                }

                else if (op == "/")
                {
                    current = ProcessDiv(current, second);
                }
                else if (op == "&")
                {
                    current = ProcessAnd(current, second, op);
                }
                else if (op == "|")
                {
                    current = ProcessOr(current, second, op);
                }
                else if (op == "=")
                {
                    current = ProcessEq(current, second);
                }
                else if (op == ">")
                {
                    current = ProcessGt(current, second);
                }
                else if (op == "<")
                {
                    current = ProcessLt(current, second);
                }
                else
                    throw new InvalidOperationException($"Undefined operator {op}.");
            }

            return current;
        }

        private static Boolean ProcessGt(Object? current, Object? second)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));

            var comparable = (IComparable)current;
            var secondConverted = Convert.ChangeType(second, current.GetType());
            return comparable.CompareTo(secondConverted) > 0;
        }

        private static Boolean ProcessLt(Object? current, Object? second)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));
            
            var comparable = (IComparable)current;
            var secondConverted = Convert.ChangeType(second, current.GetType());
            return comparable.CompareTo(secondConverted) < 0;
        }

        private static Object ProcessEq(Object? current, Object? second)
        {
            if (current == null && second == null)
                return true;

            if (current is IComparable comparable)
                return comparable.CompareTo(second) == 0;
            
            return current?.ToString() == second?.ToString();
        }

        private static Object ProcessOr(Object? current, Object? second, String op)
        {
            if (current is Boolean a && second is Boolean b)
                current = a | b;
            else
                throw new InvalidOperationException($"Undefined operator {op} for type {current?.GetType()}");
            return current;
        }


        private static Object ProcessAnd(Object? current, Object? second, String op)
        {
            if (current is Boolean a && second is Boolean b)
                current = a & b;
            else
                throw new InvalidOperationException($"Undefined operator {op} for type {current?.GetType()}");
            return current;
        }

        private static Object ProcessDiv(Object? current, Object? second)
        {
            if (current is Int32 int32Value)
                current = int32Value / Convert.ToInt32(second);
            else if (current is Int64 int64Value)
                current = int64Value / Convert.ToInt64(second);
            else if (current is UInt64 uint64Value)
                current = uint64Value / Convert.ToUInt64(second);
            else if (current is Double doubleValue)
                current = doubleValue / Convert.ToDouble(second);
            else if (current is Decimal decimalValue)
                current = decimalValue / Convert.ToDecimal(second);
            else if (current is Byte byteValue)
                current = byteValue / Convert.ToByte(second);
            else
                current = ProcessCommonOperation(current, second, "op_Division");

            return current;
        }

        private static Object ProcessMult(Object? current, Object? second)
        {
            if (current is Int32 int32Value)
                current = int32Value * Convert.ToInt32(second);
            else if (current is Int64 int64Value)
                current = int64Value * Convert.ToInt64(second);
            else if (current is UInt64 uint64Value)
                current = uint64Value * Convert.ToUInt64(second);
            else if (current is Double doubleValue)
                current = doubleValue * Convert.ToDouble(second);
            else if (current is Decimal decimalValue)
                current = decimalValue * Convert.ToDecimal(second);
            else if (current is Byte byteValue)
                current = byteValue * Convert.ToByte(second);
            else
                current = ProcessCommonOperation(current, second, "op_Multiply");

            return current;
        }

        private static Object ProcessMinus(Object? current, Object? second)
        {
            if (current is Int32 int32Value)
                current = int32Value - Convert.ToInt32(second);
            else if (current is Int64 int64Value)
                current = int64Value - Convert.ToInt64(second);
            else if (current is UInt64 uint64Value)
                current = uint64Value - Convert.ToUInt64(second);
            else if (current is Double doubleValue)
                current = doubleValue - Convert.ToDouble(second);
            else if (current is Decimal decimalValue)
                current = decimalValue - Convert.ToDecimal(second);
            else if (current is Byte byteValue)
                current = byteValue - Convert.ToByte(second);
            else
                current = ProcessCommonOperation(current, second, "op_Subtraction");
                
            return current;
        }

        private static Object ProcessPlus(Object? current, Object? second)
        {
            if (current is String strValue)
                current = strValue + second;
            else if (current is Int32 int32Value)
                current = int32Value + Convert.ToInt32(second);
            else if (current is Int64 int64Value)
                current = int64Value + Convert.ToInt64(second);
            else if (current is UInt64 uint64Value)
                current = uint64Value + Convert.ToUInt64(second);
            else if (current is Double doubleValue)
                current = doubleValue + Convert.ToDouble(second);
            else if (current is Decimal decimalValue)
                current = decimalValue + Convert.ToDecimal(second);
            else if (current is Byte byteValue)
                current = byteValue + Convert.ToByte(second);
            else
                current = ProcessCommonOperation(current, second, "op_Addition");

            return current;
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