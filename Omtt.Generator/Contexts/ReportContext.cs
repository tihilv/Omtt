using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.StatementModel;
using Omtt.Api.TemplateModel;
using Omtt.Statements;

namespace Omtt.Generator.Contexts
{
    public delegate IStatementContext CreateStatementContextDelegate(Object? sourceData, IStatementContext? parentStatementContext);

    internal sealed class GeneratorContext : ProcessingContext<GeneratorContext>, IGeneratorContext
    {
        private Stream _outputStream;
        private readonly CreateStatementContextDelegate? _createStatementContextFunc;
        private readonly IStatementContext _currentStatementContext;

        public IStatementContext StatementContext => _currentStatementContext;
        
        private GeneratorContext(Dictionary<String, ITemplateOperation>? operations, Stream outputStream, Stack<GeneratorContext> contexts, Object? sourceData, String? fragmentType, CreateStatementContextDelegate? createStatementContextFunc):
            base (operations, contexts, fragmentType, sourceData)
        {
            _outputStream = outputStream;
            _createStatementContextFunc = createStatementContextFunc;

            var parentContext = (contexts.Count > 0)?contexts.Peek():null;
            
            if (_createStatementContextFunc == null)
                _currentStatementContext = new StatementContext(sourceData, parentContext?._currentStatementContext);
            else
                _currentStatementContext = _createStatementContextFunc(sourceData, parentContext?._currentStatementContext);
        }
        
        public GeneratorContext(Stream outputStream, CreateStatementContextDelegate? getStatementContextFunc): this(null, outputStream, new Stack<GeneratorContext>(), null, null, getStatementContextFunc)
        {
            Contexts.Push(this);
        }

        public override Task ProcessOperationAsync(OperationTemplatePart operationPart)
        {
            var operation = GetOperation(operationPart);
            return operation.PerformAsync(operationPart, this);
        }

        public override Task WriteAsync(String result)
        {
            var buffer = Encoding.UTF8.GetBytes(result);
            return _outputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        protected override GeneratorContext CreateChildContext(Object? data)
        {
            return new GeneratorContext(Operations, _outputStream, Contexts, data, FragmentType, _createStatementContextFunc);
        }

        public IDisposable OverloadStream(Stream stream)
        {
            return new StreamOverload(this, stream);
        }

        private class StreamOverload: IDisposable
        {
            private readonly Stream _originalStream;
            private readonly GeneratorContext _context;

            public StreamOverload(GeneratorContext context, Stream newStream)
            {
                _context = context;
                _originalStream = _context._outputStream;
                _context._outputStream = newStream;
            }

            public void Dispose()
            {
                _context._outputStream = _originalStream;
            }
        }
    }
}