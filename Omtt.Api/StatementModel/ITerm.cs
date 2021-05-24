using System;

namespace Omtt.Api.StatementModel
{
    public interface ITerm
    {
        Object? Calculate(IStatementContext context);
    }
}