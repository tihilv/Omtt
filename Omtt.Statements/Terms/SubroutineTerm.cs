using System;
using System.Collections.Generic;
using System.Linq;
using Omtt.Api.StatementModel;

namespace Omtt.Statements.Terms
{
    internal sealed class SubroutineTerm : ITerm
    {
        private readonly String _name;
        private readonly Expression[] _arguments;

        internal SubroutineTerm(String name, IEnumerable<Expression> arguments)
        {
            _name = name;
            _arguments = arguments.ToArray();
        }

        public Object? Calculate(IStatementContext context)
        {
            return context.ExecuteFunction(_name, context, _arguments.Select(a=>a.Calculate(context)).ToArray());
        }
    }
}