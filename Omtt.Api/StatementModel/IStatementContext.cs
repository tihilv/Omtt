using System;
using System.Collections.Generic;

namespace Omtt.Api.StatementModel
{
    public interface IStatementContext
    {
        Boolean TryFindVariable(String name, out Object? result);
        void SetVariable(String name, Object? value, Boolean forceCurrent);
        Object? GetParent(Int32 parentCount);
        Object? ExecuteFunction(String name, IStatementContext statementContext, Object?[] arguments);
        void AddFunctions(IEnumerable<IStatementFunction> functions);
    }
}