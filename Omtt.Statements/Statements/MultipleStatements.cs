using System;
using System.Collections.Generic;
using System.Linq;
using Omtt.Api.StatementModel;

namespace Omtt.Statements
{
    internal sealed class MultipleStatements : IStatement
    {
        private readonly IStatement[] _statements;

        internal MultipleStatements(IEnumerable<IStatement> statements)
        {
            _statements = statements.ToArray();
        }

        public Object? Execute(IStatementContext context)
        {
            Object? result = null;
            
            foreach (var statement in _statements)
                result = statement.Execute(context);

            return result;
        }
    }
}