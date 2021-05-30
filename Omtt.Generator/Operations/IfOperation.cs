using System;
using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Operations
{
    internal sealed class IfOperation: ITemplateOperation
    {
        public String Name => "if";
        
        public Task PerformAsync(OperationTemplatePart part, IGeneratorContext ctx)
        {
            if (part.InnerPart == null)
                throw new ArgumentNullException("Operation content is null.");
            
            var expr = part.Parameters[DefaultTemplateParameterNames.Clause];

            if (ctx.EvaluateStatement(expr) is Boolean val && val)
                return ctx.ExecuteAsync(part.InnerPart!);
            
            return Task.CompletedTask;
        }

        public Task PerformAsync(OperationTemplatePart part, ISourceSchemeContext ctx)
        {
            var expr = part.Parameters[DefaultTemplateParameterNames.Clause];
            ctx.EvaluateStatement(expr);
            return ctx.ExecuteAsync(part.InnerPart!);
        }
    }
}