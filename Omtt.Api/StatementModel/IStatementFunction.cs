using System;

namespace Omtt.Api.StatementModel
{
    public interface IStatementFunction
    {
        String Name { get; }
        Byte ArgumentCount { get; }

        Object Execute(IStatementContext statementContext, Object?[] input);
    }
}