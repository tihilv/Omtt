using System;

namespace Omtt.Api.StatementModel
{
    /// <summary>
    /// Interface for custom expression function.
    /// </summary>
    public interface IStatementFunction
    {
        /// <summary>
        /// Name of custom expression function. Case-sensitive.
        /// </summary>
        String Name { get; }
        
        /// <summary>
        /// Number of arguments of the function.
        /// Allows to distinguish functions with the same name but different parameter count.
        /// </summary>
        Byte ArgumentCount { get; }

        /// <summary>
        /// Calculates the custom expression function.
        /// </summary>
        /// <param name="input">Calculated parameters of the function</param>
        /// <param name="statementContext">Current statement context</param>
        /// <returns></returns>
        Object Execute(Object?[] input, IStatementContext statementContext);
    }
}