using System;
using System.Collections.Generic;
using Omtt.Api.StatementModel;

namespace Omtt.Statements
{
    public class StatementContext : IStatementContext
    {
        private readonly Object? _currentData;
        private readonly StatementContext? _parent;
        private readonly Dictionary<(String, Byte), IStatementFunction> _functions;

        private Dictionary<String, Object?>? _localVariables;

        public StatementContext(Object? currentData, IStatementContext? parent)
        {
            _currentData = currentData;
            _parent = parent as StatementContext;

            _functions = _parent?._functions ?? new Dictionary<(String, Byte), IStatementFunction>();
        }

        public Boolean TryFindVariable(String name, out Object? result)
        {
            switch (name)
            {
                case "this":
                {
                    result = _currentData;
                    return true;
                }
                case "env":
                {
                    result = this;
                    return true;
                }
                default:
                    return TryFindLocalVariable(name, out result);
            }
        }

        public void SetVariable(String name, Object? value, Boolean forceCurrent)
        {
            if (forceCurrent || !TrySetVariable(name, value))
            {
                _localVariables ??= new Dictionary<String, Object?>();
                _localVariables[name] = value;
            }
        }

        private Boolean TrySetVariable(String name, Object? value)
        {
            if (_localVariables != null && _localVariables.ContainsKey(name))
            {
                _localVariables[name] = value;
                return true;
            }

            if (_parent != null)
                return _parent.TrySetVariable(name, value);

            return false;
        }

        public Object? GetParent(Int32 generation)
        {
            var result = _currentData;
            StatementContext? context = this;

            int steps = 0;
            do
            {
                context = context?._parent;
                if (result != context?._currentData)
                {
                    result = context?._currentData;
                    steps++;
                }
                
            } while (steps < generation);

            return result;
        }

        public virtual Object? ExecuteFunction(String name, IStatementContext statementContext, Object?[] arguments)
        {
            var key = (name, (Byte) arguments.Length);
            if (_functions.TryGetValue(key, out var function))
                return function.Execute(statementContext, arguments);

            throw new MissingMethodException($"Unknown function '{name}'\'{arguments.Length}.");
        }

        public void AddFunctions(IEnumerable<IStatementFunction> functions)
        {
            foreach (var function in functions)
                _functions[(function.Name, function.ArgumentCount)] = function;
        }

        private Boolean TryFindLocalVariable(String name, out Object? result)
        {
            result = null;
            
            if (_localVariables != null && _localVariables.TryGetValue(name, out result))
                return true;

            if (_parent != null)
                return _parent.TryFindLocalVariable(name, out result);

            return false;
        }
    }
}