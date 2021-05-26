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
        public Object? SourceData { get; }
        Task ProcessOperationAsync(OperationTemplatePart operationPart);

        /// <summary>
        /// Processes template transformation.
        /// </summary>
        /// <param name="templatePart">Template to transform</param>
        /// <param name="data">Current data object</param>
        Task ExecuteAsync(ITemplatePart templatePart, Object? data);
        
        TT WithContext<TT>(Object? data, Func<IProcessingContext, TT> func);
        
        /// <summary>
        /// Writes the string to the current output stream.
        /// </summary>
        /// <param name="result">Stream to write</param>
        Task WriteAsync(String result);
    }
    
    /// <summary>
    /// Omtt context
    /// </summary>
    public interface IGeneratorContext: IProcessingContext
    {
        /// <summary>
        /// Current statement context.
        /// </summary>
        IStatementContext StatementContext { get; }
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