using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.StatementModel;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Contexts
{
    public abstract class ProcessingContext<T>: IProcessingContext where T: class, IProcessingContext
    {
        protected Dictionary<String, ITemplateOperation>? Operations;
        protected readonly Stack<T> Contexts;
        protected readonly IStatementContext CurrentStatementContext;

        public String? FragmentType { get; set; }
        
        public IStatementContext StatementContext => CurrentStatementContext;

        protected ProcessingContext(Dictionary<String, ITemplateOperation>? operations, Stack<T> contexts, String? fragmentType, IStatementContext currentStatementContext)
        {
            Operations = operations;
            Contexts = contexts;
            FragmentType = fragmentType;
            CurrentStatementContext = currentStatementContext;
        }

        public void SetOperations(Dictionary<String, ITemplateOperation> operations)
        {
            Operations = operations;
        }

        protected ITemplateOperation GetOperation(OperationTemplatePart operationTemplatePart)
        {
            if (operationTemplatePart.CachedTemplateOperation != null)
                return operationTemplatePart.CachedTemplateOperation;

            if (Operations != null && Operations.TryGetValue(operationTemplatePart.OperationName, out var operation))
            {
                operationTemplatePart.CachedTemplateOperation = operation;
                return operation;
            }

            throw new MissingMethodException($"Operation '{operationTemplatePart.OperationName}' not found.");
        }

        public async Task ExecuteAsync(ITemplatePart templatePart)
        {
            var transformer = ContentTransformers.Get(templatePart);
            await transformer.TransformAsync(templatePart, this);
        }

        public TT WithContext<TT>(Object? data, Func<IProcessingContext, TT> func)
        {
            var childCtx = CreateChildContext(data);
            try
            {
                Contexts.Push(childCtx);
                return func(childCtx);
            }
            finally
            {
                Contexts.Pop();
            }
        }

        public void ReplaceCurrentData(Object? sourceData)
        {
            CurrentStatementContext.ReplaceCurrentData(sourceData);
        }

        public abstract Task ProcessOperationAsync(OperationTemplatePart operationPart);
        public abstract Task WriteAsync(String result);
        protected abstract T CreateChildContext(Object? data);
    }
}