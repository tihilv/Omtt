using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Omtt.Api.StatementModel;
using Omtt.Api.TemplateModel;

namespace Omtt.Api.Generation
{
    public interface IProcessingContext
    {
        void SetOperations(Dictionary<String, ITemplateOperation> operations);
        String? FragmentType { get; set; }
        
        /// <summary>
        /// Current statement context.
        /// </summary>
        IStatementContext StatementContext { get; }
        
        Task ProcessOperationAsync(OperationTemplatePart operationPart);

        /// <summary>
        /// Processes template transformation.
        /// </summary>
        /// <param name="templatePart">Template to transform</param>
        Task ExecuteAsync(ITemplatePart templatePart);
        
        /// <summary>
        /// Performs the function creating a child context for the given data object. 
        /// </summary>
        /// <param name="data">New current data object</param>
        /// <param name="func">Delegate to execute</param>
        /// <typeparam name="TT">Return type</typeparam>
        /// <returns>Result of the function</returns>
        TT WithContext<TT>(Object? data, Func<IProcessingContext, TT> func);
        
        /// <summary>
        /// Writes the string to the current output stream.
        /// </summary>
        /// <param name="result">Stream to write</param>
        Task WriteAsync(String result);

        /// <summary>
        /// Sets new current data object. Old value becomes lost.
        /// </summary>
        /// <param name="sourceData">New data object</param>
        void ReplaceCurrentData(Object? sourceData);
    }
    
    /// <summary>
    /// Omtt context
    /// </summary>
    public interface IGeneratorContext: IProcessingContext
    {
        /// <summary>
        /// Temporary reroutes output to the given stream. 
        /// </summary>
        /// <param name="stream">Stream to reroute the output</param>
        /// <returns>A handle to cancel the rerouting</returns>
        IDisposable OverloadStream(Stream stream);
    }

    /// <summary>
    /// Omtt source scheme generation context.
    /// </summary>
    public interface ISourceSchemeContext: IProcessingContext
    {
        /// <summary>
        /// Processes the statement to generate source scheme.
        /// </summary>
        /// <param name="statement">Statement to process</param>
        /// <param name="arrayExpected">Is array object expected</param>
        /// <returns>Intermediate object</returns>
        Object EvaluateStatement(IStatement statement, Boolean arrayExpected = false);
    }
}