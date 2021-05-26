using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Omtt.Api.DataModel;
using Omtt.Api.Generation;
using Omtt.Api.StatementModel;
using Omtt.Api.TemplateModel;
using Omtt.Statements;

namespace Omtt.Generator.Contexts
{
    public class SourceSchemeContext : ProcessingContext<SourceSchemeContext>, ISourceSchemeContext
    {
        private readonly IStatementContext _currentStatementContext;

        protected SourceSchemeContext(Dictionary<String, ITemplateOperation>? operations, Stack<SourceSchemeContext> contexts, Object? sourceData, String? fragmentType) : base(operations, contexts, fragmentType, sourceData)
        {
            var parentContext = (contexts.Any())?contexts.Peek():null;
            _currentStatementContext = new StatementContextForSourceScheme(sourceData, parentContext?._currentStatementContext);
        }

        public SourceSchemeContext(): this(null, new Stack<SourceSchemeContext>(), null, null)
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
            return Task.CompletedTask; // the actual write is not necessary
        }

        public Object EvaluateStatement(IStatement statement, Boolean arrayExpected = false)
        {
            var result = statement.Execute(_currentStatementContext);
            
            if (arrayExpected && result is SourceScheme sourceScheme)
                sourceScheme.SetIsArray();

            return result!;
        }

        protected override SourceSchemeContext CreateChildContext(Object? data)
        {
            return new SourceSchemeContext(Operations, Contexts, data, FragmentType);
        }

        private class StatementContextForSourceScheme : StatementContext
        {
            public StatementContextForSourceScheme(Object? currentData, IStatementContext? parent) : base(currentData, parent)
            {
            }

            public override Object? ExecuteFunction(String name, Object?[] arguments)
            {
                if (arguments.Any())
                    return arguments[0];

                return null;
            }
        }
    }
}