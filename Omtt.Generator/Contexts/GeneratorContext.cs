using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.StatementModel;
using Omtt.Api.TemplateModel;
using Omtt.Generator.Extensions;
using Omtt.Statements;

namespace Omtt.Generator.Contexts
{
    public delegate IStatementContext CreateStatementContextDelegate(Object? sourceData, IStatementContext? parentStatementContext);

    internal sealed class GeneratorContext : ProcessingContext<GeneratorContext>, IGeneratorContext
    {
        private StreamWriter _streamWriter;
        private readonly CreateStatementContextDelegate? _createStatementContextFunc;

        public static StreamWriter CreateStreamWriter(Stream outputStream)
        {
            return new (outputStream, null, -1, true);
        }

        private GeneratorContext(Dictionary<String, ITemplateOperation>? operations, StreamWriter streamWriter, Stack<GeneratorContext> contexts, Object? sourceData, String? fragmentType, CreateStatementContextDelegate? createStatementContextFunc):
            base (operations, contexts, fragmentType,
                createStatementContextFunc == null ? new StatementContext(sourceData, contexts.PeekOrDefault()?.CurrentStatementContext) : createStatementContextFunc(sourceData, contexts.PeekOrDefault()?.CurrentStatementContext))
        {
            _streamWriter = streamWriter;
            _createStatementContextFunc = createStatementContextFunc;
        }
        
        public GeneratorContext(StreamWriter streamWriter, Object? sourceData, CreateStatementContextDelegate? getStatementContextFunc): this(null, streamWriter, new Stack<GeneratorContext>(), sourceData, null, getStatementContextFunc)
        {
            Contexts.Push(this);
            ReplaceCurrentData(sourceData);
        }

        public override Task ProcessOperationAsync(OperationTemplatePart operationPart)
        {
            var operation = GetOperation(operationPart);
            return operation.PerformAsync(operationPart, this);
        }

        public override Task WriteAsync(String result)
        {
            return _streamWriter.WriteAsync(result);
        }

        protected override GeneratorContext CreateChildContext(Object? data)
        {
            return new GeneratorContext(Operations, _streamWriter, Contexts, data, FragmentType, _createStatementContextFunc);
        }

        public IDisposable OverloadStream(Stream stream)
        {
            return new StreamOverload(this, stream);
        }

        private class StreamOverload: IDisposable
        {
            private readonly StreamWriter _originalStreamWriter;
            private readonly GeneratorContext _context;

            public StreamOverload(GeneratorContext context, Stream newStream)
            {
                _context = context;
                _originalStreamWriter = _context._streamWriter;
                _context._streamWriter = CreateStreamWriter(newStream);
            }

            public void Dispose()
            {
                _context._streamWriter.Flush();
                _context._streamWriter.Dispose();
                _context._streamWriter = _originalStreamWriter;
            }
        }
    }
}