using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Omtt.Api.Exceptions;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Contexts
{
    public abstract class ProcessingContext<T>: IProcessingContext where T: class, IProcessingContext
    {
        private readonly Object? _sourceData;
        protected Dictionary<String, ITemplateOperation>? Operations;
        protected readonly Stack<T> Contexts;

        public Object? SourceData => _sourceData;
        public String? FragmentType { get; set; }
        
        public void SetOperations(Dictionary<String, ITemplateOperation> operations)
        {
            Operations = operations;
        }

        protected ProcessingContext(Dictionary<String, ITemplateOperation>? operations, Stack<T> contexts, String? fragmentType, Object? sourceData)
        {
            Operations = operations;
            Contexts = contexts;
            FragmentType = fragmentType;
            _sourceData = sourceData;
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

        public async Task ExecuteAsync(ITemplatePart templatePart, Object? data)
        {
            var childCtx = CreateChildContext(data);
            try
            {
                Contexts.Push(childCtx);
                var transformer = ContentTransformers.Get(templatePart);
                await transformer.TransformAsync(templatePart, childCtx);
            }
            finally
            {
                Contexts.Pop();
            }
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
        
        public abstract Task ProcessOperationAsync(OperationTemplatePart operationPart);
        public abstract Task WriteAsync(String result);
        protected abstract T CreateChildContext(Object? data);
    }
}