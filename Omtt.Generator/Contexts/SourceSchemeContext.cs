using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Omtt.Api.DataModel;
using Omtt.Api.Generation;
using Omtt.Api.StatementModel;
using Omtt.Api.TemplateModel;
using Omtt.Generator.Extensions;
using Omtt.Statements;

namespace Omtt.Generator.Contexts
{
    public class SourceSchemeContext : ProcessingContext<SourceSchemeContext>, ISourceSchemeContext
    {
        protected SourceSchemeContext(Dictionary<String, ITemplateOperation>? operations, Stack<SourceSchemeContext> contexts, Object? sourceData, String? fragmentType) : 
            base(operations, contexts, fragmentType,  
                new StatementContextForSourceScheme(sourceData, contexts.PeekOrDefault()?.CurrentStatementContext))
        {
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
            var result = statement.Execute(CurrentStatementContext);
            
            if (arrayExpected && result is SourceScheme sourceScheme)
                sourceScheme.SetIsArray();

            return result!;
        }

        /// <summary>
        /// Creates a child context.
        /// </summary>
        /// <param name="data">Current data object</param>
        /// <returns>Child source scheme context</returns>
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