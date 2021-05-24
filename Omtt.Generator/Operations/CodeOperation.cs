using System;
using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Operations
{
    internal sealed class CodeOperation: ITemplateOperation
    {
        public String Name => "code";
        
        public async Task PerformAsync(OperationTemplatePart part, IGeneratorContext ctx)
        {
            var expr = part.Parameters[DefaultTemplateParameterNames.Source];
            ctx.EvaluateStatement(expr);
            if (part.InnerPart != null)
                await ctx.ExecuteAsync(part.InnerPart!, ctx.SourceData);
        }

        public Task PerformAsync(OperationTemplatePart part, ISourceSchemeContext ctx)
        {
            var expr = part.Parameters[DefaultTemplateParameterNames.Source];
            ctx.EvaluateStatement(expr);
            if (part.InnerPart != null)
                return ctx.ExecuteAsync(part.InnerPart!, ctx.SourceData);
            
            return Task.CompletedTask;
        }
    }
}