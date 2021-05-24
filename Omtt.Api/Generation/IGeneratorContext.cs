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
        Task ExecuteAsync(ITemplatePart templatePart, Object? data);
        TT WithContext<TT>(Object? data, Func<IProcessingContext, TT> func);
        Task WriteAsync(String result);
    }
    
    public interface IGeneratorContext: IProcessingContext
    {
        IStatementContext StatementContext { get; }
        Object? EvaluateStatement(IStatement statement);
        IDisposable OverloadStream(Stream stream);
    }

    public interface ISourceSchemeContext: IProcessingContext
    {
        Object EvaluateStatement(IStatement statement, Boolean arrayExpected = false);
    }
}