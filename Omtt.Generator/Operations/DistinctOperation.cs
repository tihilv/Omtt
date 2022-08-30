using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Operations
{
    internal sealed class DistinctOperation: ITemplateOperation
    {
        public String Name => "distinct";

        private const String LinkTracking = "1";
        private const String LinkedChanged = "2";
        
        private const String DistinctControlVariableName = "^distinct_manage";
    
        public async Task PerformAsync(OperationTemplatePart part, IGeneratorContext ctx)
        {
            if (part.InnerPart == null)
                throw new ArgumentNullException("Operation content is null.");
        
            if (ctx.StatementContext.Parent == null)
                throw new ArgumentNullException("Parent operation context doesn't exist.");

            var linkedElementsChanged = GetLinkedElementsChanged(ctx, part);

            part.Parameters.TryGetValue(DefaultTemplateParameterNames.Source, out var sourceExpr);
            var sourceValue = ctx.EvaluateStatement(sourceExpr)?.ToString();
            
            string content;
            using (var memoryStream = new MemoryStream())
            {
                using (ctx.OverloadStream(memoryStream))
                    await ctx.ExecuteAsync(part.InnerPart!);

                memoryStream.Position = 0;
                content = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            var varName = "^distinct_" + part.GetHashCode();
            ctx.StatementContext.TryFindVariable(varName, out var prevValue);

            var valueToCompare = sourceValue ?? content;
            bool valueChanged = !valueToCompare.Equals(prevValue);
            if (valueChanged)
                SetLinkedValueChanged(ctx, varName, valueToCompare, linkedElementsChanged);

            if (valueChanged || linkedElementsChanged == true)
                await ctx.WriteAsync(content);
        }

        // null - no linked elements, false - not changed, true - linked elements changed
        private static Boolean? GetLinkedElementsChanged(IGeneratorContext ctx, OperationTemplatePart part)
        {
            part.Parameters.TryGetValue(DefaultTemplateParameterNames.Link, out var linkedExpr);
            var linked = (linkedExpr != null) ? ctx.EvaluateStatement(linkedExpr)?.ToString() : null;

            bool? linkedElementsChanged = null; 
            if (linked == "1")
            {
                ctx.StatementContext.Parent!.SetVariable(DistinctControlVariableName, LinkTracking, true);
                linkedElementsChanged = false;
            }
            else
            {
                ctx.StatementContext.TryFindVariable(DistinctControlVariableName, out var inUniqueStr);
                if (LinkTracking.Equals(inUniqueStr))
                    linkedElementsChanged = false;
                else if (LinkedChanged.Equals(inUniqueStr))
                    linkedElementsChanged = true;
            }
            if (linked == "0")
                ctx.StatementContext.Parent!.SetVariable(DistinctControlVariableName, null, true);

            return linkedElementsChanged;
        }

        private static void SetLinkedValueChanged(IGeneratorContext ctx, String varName, String content, Boolean? linkedElementsChanged)
        {
            ctx.StatementContext.Parent!.SetVariable(varName, content, true);
            if (linkedElementsChanged == false)
                ctx.StatementContext.Parent.SetVariable(DistinctControlVariableName, LinkedChanged, true);
        }

        public Task PerformAsync(OperationTemplatePart part, ISourceSchemeContext ctx)
        {
            return ctx.ExecuteAsync(part.InnerPart!);
        }
    }
}