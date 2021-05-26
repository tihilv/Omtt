using System;

namespace Omtt.Api.StatementModel
{
    /// <summary>
    /// Expression statement.
    /// </summary>
    public interface IStatement
    {
        /// <summary>
        /// Executes the statement for the given context.
        /// </summary>
        /// <param name="context">Current statement context</param>
        /// <returns>Result of statement calculation</returns>
        Object? Execute(IStatementContext context);
    }
}