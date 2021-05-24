using System;

namespace Omtt.Api.StatementModel
{
    public interface IStatement
    {
        Object? Execute(IStatementContext context);
    }
}