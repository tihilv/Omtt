using System;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Operations
{
    internal sealed class ForEachOperation: ITemplateOperation
    {
        internal const String IsFirstVariable = "$first";
        internal const String IsLastVariable = "$last";
        internal const String IndexVariable = "$index";
        
        public String Name => "forEach";
        
        public async Task PerformAsync(OperationTemplatePart part, IGeneratorContext ctx)
        {
            if (part.InnerPart == null)
                throw new ArgumentNullException("Operation content is null.");
            
            var expr = part.Parameters[DefaultTemplateParameterNames.Source];

            if (ctx.EvaluateStatement(expr) is IEnumerable valueCollection)
            {
                var enumerator = valueCollection.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    Boolean isFirst = true;
                    Boolean hasNext;
                    ctx.StatementContext.SetVariable(IsLastVariable, false, true);
                    do
                    {
                        var childPart = enumerator.Current;
                        hasNext = enumerator.MoveNext();
                    
                        if (!hasNext)
                            ctx.StatementContext.SetVariable(IsLastVariable, true, true);
                        
                        if (isFirst)
                            ctx.StatementContext.SetVariable(IsFirstVariable, true, true);
                        
                        await ctx.ExecuteAsync(part.InnerPart!, childPart);
                        if (isFirst)
                        {
                            isFirst = false;
                            ctx.StatementContext.SetVariable(IsFirstVariable, false, true);
                        }
                    } while (hasNext);
                }
            }
        }

        public Task PerformAsync(OperationTemplatePart part, ISourceSchemeContext ctx)
        {
            var expr = part.Parameters[DefaultTemplateParameterNames.Source];
            var arrayElement = ctx.EvaluateStatement(expr, true);
            return ctx.ExecuteAsync(part.InnerPart!, arrayElement);
        }
    }
}