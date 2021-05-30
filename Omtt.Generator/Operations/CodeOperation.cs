using System;
using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Operations
{
    internal sealed class CodeOperation: ITemplateOperation
    {
        public String Name => "code";
        
        public Task PerformAsync(OperationTemplatePart part, IGeneratorContext ctx)
        {
            var expr = part.Parameters[DefaultTemplateParameterNames.Source];
            ctx.EvaluateStatement(expr);
            if (part.InnerPart != null)
                return ctx.WithContext(ctx.StatementContext.CurrentData, childContext => childContext.ExecuteAsync(part.InnerPart!));
                
            return Task.CompletedTask;
        }

        public Task PerformAsync(OperationTemplatePart part, ISourceSchemeContext ctx)
        {
            var expr = part.Parameters[DefaultTemplateParameterNames.Source];
            ctx.EvaluateStatement(expr);
            if (part.InnerPart != null)
                return ctx.WithContext(ctx.StatementContext.CurrentData, childContext => childContext.ExecuteAsync(part.InnerPart!));
            
            return Task.CompletedTask;
        }
    }
}