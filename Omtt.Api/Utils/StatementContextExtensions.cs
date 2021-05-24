using System;
using Omtt.Api.Exceptions;

namespace Omtt.Api.StatementModel
{
    public static class StatementContextExtensions
    {
        public static Object? GetVariable(this IStatementContext context, String name)
        {
            if (context.TryFindVariable(name, out var result))
                return result;
            
            throw new MissingMemberException($"Variable '{name}' is not found.");
        }
    }
}