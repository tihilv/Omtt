using System;
using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Operations
{
    internal sealed class FragmentOperation: ITemplateOperation
    {
        public String Name => "fragment";
        
        public async Task PerformAsync(OperationTemplatePart part, IGeneratorContext ctx)
        {
            if (part.InnerPart == null)
                throw new ArgumentNullException("Operation content is null.");
            
            var expr = part.Parameters[DefaultTemplateParameterNames.Type];

            var type = ctx.EvaluateStatement(expr)?.ToString();
            var oldFragmentType = ctx.FragmentType;
            ctx.FragmentType = type;
            await ctx.ExecuteAsync(part.InnerPart!, ctx.SourceData);
            ctx.FragmentType = oldFragmentType;
        }

        public Task PerformAsync(OperationTemplatePart part, ISourceSchemeContext ctx)
        {
            var expr = part.Parameters[DefaultTemplateParameterNames.Type];
            ctx.EvaluateStatement(expr);
            return ctx.ExecuteAsync(part.InnerPart!, ctx.SourceData);
        }
    }
}