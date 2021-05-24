using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Operations
{
    internal sealed class IfOperation: ITemplateOperation
    {
        public String Name => "if";
        
        public async Task PerformAsync(OperationTemplatePart part, IGeneratorContext ctx)
        {
            if (part.InnerPart == null)
                throw new ArgumentNullException("Operation content is null.");
            
            var expr = part.Parameters[DefaultTemplateParameterNames.Clause];

            if (ctx.EvaluateStatement(expr) is Boolean val && val)
                await ctx.ExecuteAsync(part.InnerPart!, ctx.SourceData);
        }

        public Task PerformAsync(OperationTemplatePart part, ISourceSchemeContext ctx)
        {
            var expr = part.Parameters[DefaultTemplateParameterNames.Clause];
            ctx.EvaluateStatement(expr);
            return ctx.ExecuteAsync(part.InnerPart!, ctx.SourceData);
        }
    }
}