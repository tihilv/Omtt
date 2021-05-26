using System;
using System.Globalization;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Operations
{
    /// <summary>
    /// Default implementation of `write` markup operation.
    /// </summary>
    public class WriteOperation: ITemplateOperation
    {
        public String Name => CommonOperations.WriteOperation;

        public Task PerformAsync(OperationTemplatePart part, IGeneratorContext ctx)
        {
            var expr = part.Parameters[DefaultTemplateParameterNames.Source];
            var value = ctx.EvaluateStatement(expr);

            String? format = null;
            String? cultureName = null;
            Object? align = null;
            if (part.Parameters.TryGetValue(DefaultTemplateParameterNames.Format, out var formatExpr) && formatExpr != null)
                format = ctx.EvaluateStatement(formatExpr)?.ToString();

            if (part.Parameters.TryGetValue(DefaultTemplateParameterNames.Culture, out var cultureExpr) && cultureExpr != null)
                cultureName = ctx.EvaluateStatement(cultureExpr)?.ToString();

            if (part.Parameters.TryGetValue(DefaultTemplateParameterNames.Align, out var alignExpr))
                 align = ctx.EvaluateStatement(alignExpr);
            
            var culture = (String.IsNullOrEmpty(cultureName)) ? CultureInfo.CurrentCulture : CultureInfo.CreateSpecificCulture(cultureName!);
            
            String? valueStr = FormatResult(value, format, culture, ctx);

            if (!String.IsNullOrEmpty(valueStr))
            {
                if (align != null)
                    valueStr = String.Format("{0," + align + "}", valueStr);
                
                var fragmentType = ctx.FragmentType;
                if (fragmentType != null)
                    valueStr = FormatFragment(valueStr!, fragmentType);

                return ctx.WriteAsync(valueStr!);
            }

            return Task.CompletedTask;
        }

        public Task PerformAsync(OperationTemplatePart part, ISourceSchemeContext ctx)
        {
            var expr = part.Parameters[DefaultTemplateParameterNames.Source];
            ctx.EvaluateStatement(expr);

            if (part.Parameters.TryGetValue(DefaultTemplateParameterNames.Format, out var formatExpr) && formatExpr != null)
                ctx.EvaluateStatement(formatExpr);

            if (part.Parameters.TryGetValue(DefaultTemplateParameterNames.Culture, out var cultureExpr))
                ctx.EvaluateStatement(cultureExpr);

            if (part.Parameters.TryGetValue(DefaultTemplateParameterNames.Align, out var alignExpr))
                ctx.EvaluateStatement(alignExpr);
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Virtual method for formatting a value to a string according to format string and culture.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="format">Format strng</param>
        /// <param name="culture">Culture</param>
        /// <param name="generatorContext">Current generator context</param>
        /// <returns>Formatted string</returns>
        protected virtual String? FormatResult(Object? value, String? format, CultureInfo culture, IGeneratorContext generatorContext)
        {
            if (!String.IsNullOrEmpty(format))
            {
                if (value is DateTime dt)
                    return dt.ToString(format, culture);

                if (value is Byte bt)
                    return bt.ToString(format, culture);

                if (value is Int32 it)
                    return it.ToString(format, culture);

                if (value is Decimal dct)
                    return dct.ToString(format, culture);

                if (value is Double dbt)
                    return dbt.ToString(format, culture);

                if (value is String str)
                {
                    if (format!.Equals("U", StringComparison.InvariantCultureIgnoreCase))
                        return str.ToUpper();

                    if (format.Equals("L", StringComparison.InvariantCultureIgnoreCase))
                        return str.ToLower();
                }
            }
            else
            {
                if (value is DateTime dt)
                    return dt.ToString(culture);

                if (value is Byte bt)
                    return bt.ToString(culture);

                if (value is Int32 it)
                    return it.ToString(culture);

                if (value is Decimal dct)
                    return dct.ToString(culture);

                if (value is Double dbt)
                    return dbt.ToString(culture);
            }

            return value?.ToString();
        }

        /// <summary>
        /// Virtual method for escaping a string according to fragment type.
        /// </summary>
        /// <param name="valueStr">String to escape</param>
        /// <param name="fragmentType">Fragment type</param>
        /// <returns>Escaped string</returns>
        /// <exception cref="ArgumentException">Thrown when no known fragment type is provided</exception>
        protected virtual String FormatFragment(String valueStr, String fragmentType)
        {
            if (fragmentType.Equals(CommonFragmentTypes.Xml, StringComparison.InvariantCultureIgnoreCase))
                return SecurityElement.Escape(valueStr)!;
            
            if (fragmentType.Equals(CommonFragmentTypes.Html, StringComparison.InvariantCultureIgnoreCase))
                return HttpUtility.HtmlEncode(valueStr);
            
            throw new ArgumentException($"Unable to process the given fragment type: '{fragmentType}'");
        }
    }
}