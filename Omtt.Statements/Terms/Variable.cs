using System;
using System.Collections.Generic;
using System.Linq;
using Omtt.Api.StatementModel;
using Omtt.Statements.Utils;

namespace Omtt.Statements.Terms
{
    internal sealed class Variable: ITerm
    {
        private const String ParentText = "parent";
        
        private readonly VariablePathPair[] _path;

        internal Variable(IEnumerable<VariablePathPair> path)
        {
            _path = path.ToArray();
        }

        public Object? Calculate(IStatementContext context)
        {
            return GetObject(context, _path.Length);
        }

        internal void Set(IStatementContext context, Object? value)
        {
            if (_path.Length == 1)
                context.SetVariable(_path[0].Name, value, false);
            else
            {
                var obj = GetObject(context, _path.Length - 1);
                var pair = _path.Last();

                if (pair.ArrayExpression != null)
                {
                    var indexObj = pair.ArrayExpression.Calculate(context);
                    var finalArrayObj = GetPropertyValue(obj, pair.Name);
                    if (finalArrayObj == null)
                        throw new ArrayTypeMismatchException($"List '{pair.Name}' is not defined.");
                    
                    ValuesConverter.SetCollectionElement(finalArrayObj, indexObj, value);
                }
                else
                {
                    if (obj == null)
                        throw new MissingMemberException($"Given object '{_path[_path.Length - 2].Name}' is not defined.");

                    ValuesConverter.SetPropertyValue(obj, pair.Name, value);
                }
            }
        }
        
        private Object? GetObject(IStatementContext context, Int32 level)
        {
            var (root, propertyStartIndex) = GetRoot(context);

            var obj = GetArrayElement(root, _path[propertyStartIndex - 1], context);
            for (Int32 i = propertyStartIndex; i < level; i++)
            {
                var pair = _path[i];

                obj = GetPropertyValue(obj, pair.Name);
                obj = GetArrayElement(obj, pair, context);
            }

            return obj;
        }

        private Object? GetPropertyValue(Object? obj, String propertyName)
        {
            return ValuesConverter.GetPropertyValue(obj, propertyName);
        }

        private Object? GetArrayElement(Object? obj, VariablePathPair pair, IStatementContext context)
        {
            if (obj != null && pair.ArrayExpression != null)
            {
                var indexObject = pair.ArrayExpression.Calculate(context);
                return ValuesConverter.GetCollectionElement(obj, indexObject);
            }

            return obj;
        }

        private (Object? root, Int32 propertyStartIndex) GetRoot(IStatementContext context)
        {
            Object? root;
            Int32 propertyStartIndex = 0;
            if (_path[0].Name == ParentText)
            {
                while (propertyStartIndex < _path.Length && _path[propertyStartIndex].Name == ParentText)
                    propertyStartIndex++;

                root = context.GetParent(propertyStartIndex);
            }
            else
            {
                root = context.GetVariable(_path[0].Name);
                propertyStartIndex = 1;
            }

            return (root, propertyStartIndex);
        }
    }

    internal readonly struct VariablePathPair
    {
        internal readonly String Name;
        internal readonly Expression? ArrayExpression;

        internal VariablePathPair(String name, Expression? arrayExpression)
        {
            Name = name;
            ArrayExpression = arrayExpression;
        }
    }
}