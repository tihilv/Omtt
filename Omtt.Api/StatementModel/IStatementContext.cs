using System;
using System.Collections.Generic;

namespace Omtt.Api.StatementModel
{
    /// <summary>
    /// Statement context for expression evaluations.
    /// </summary>
    public interface IStatementContext
    {
        /// <summary>
        /// Current data object.
        /// </summary>
        Object? CurrentData { get; }

        /// <summary>
        /// Sets new current data object. Old value becomes lost.
        /// </summary>
        /// <param name="currentData">New data object</param>
        void ReplaceCurrentData(Object? currentData);

        /// <summary>
        /// Tries to search a variable by the name starting from the current context to the root.
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="result">Value of the variable if found</param>
        /// <returns>Is variable found</returns>
        Boolean TryFindVariable(String name, out Object? result);
        
        /// <summary>
        /// Sets variable value.
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="value">Value of the variable</param>
        /// <param name="forceCurrent">If true, the variable is always written to the current context.
        /// Otherwise it's written to the closest context from the current where the variable is defined. If there is no such context, the variable is written to the current.</param>
        void SetVariable(String name, Object? value, Boolean forceCurrent);
        
        /// <summary>
        /// Returns the parent data source object in the given count of steps from the current one.
        /// A step is counted only when n'th and (n-1)'th objects are different.
        /// </summary>
        /// <param name="parentCount">Steps count to get the parent object.</param>
        /// <returns></returns>
        Object? GetParent(Int32 parentCount);

        /// <summary>
        /// Executes the function.
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <param name="arguments">Arguments of the function</param>
        /// <returns></returns>
        Object? ExecuteFunction(String name, Object?[] arguments);
        
        /// <summary>
        /// Registers a custom function. 
        /// </summary>
        /// <param name="functions">Custom statement function</param>
        void AddFunctions(IEnumerable<IStatementFunction> functions);
    }
}