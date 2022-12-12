using System;
using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Operations
{
    internal sealed class TimeZoneOperation: ITemplateOperation
    {
        internal const string TimeZoneVariableName = "^time_zone";
        public String Name => "timeZone";
        
        public async Task PerformAsync(OperationTemplatePart part, IGeneratorContext ctx)
        {
            if (part.InnerPart == null)
                throw new ArgumentNullException("Operation content is null.");
            
            var expr = part.Parameters[DefaultTemplateParameterNames.Key];

            var timeZoneValue = ctx.EvaluateStatement(expr);
            TimeZoneInfo timeZoneInfo;
            if (timeZoneValue is TimeZoneInfo tzi)
                timeZoneInfo = tzi;
            else if (timeZoneValue != null)
                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneValue.ToString());
            else
                throw new ArgumentNullException("TimeZone is not specified.");
            
            ctx.StatementContext.SetVariable(TimeZoneVariableName, timeZoneInfo, true);
            await ctx.ExecuteAsync(part.InnerPart!);
        }

        public Task PerformAsync(OperationTemplatePart part, ISourceSchemeContext ctx)
        {
            var expr = part.Parameters[DefaultTemplateParameterNames.Key];
            ctx.EvaluateStatement(expr);
            return ctx.ExecuteAsync(part.InnerPart!);
        }
    }
}