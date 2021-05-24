using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Operations
{
    internal sealed class GroupOperation: ITemplateOperation
    {
        public String Name => "group";
        
        public async Task PerformAsync(OperationTemplatePart part, IGeneratorContext ctx)
        {
            if (part.InnerPart == null)
                throw new ArgumentNullException("Operation content is null.");
            
            var sourceExpr = part.Parameters[DefaultTemplateParameterNames.Source];
            var keyExpr = part.Parameters[DefaultTemplateParameterNames.Key];
            var valueCollection = ctx.EvaluateStatement(sourceExpr) as IEnumerable;

            if (valueCollection != null)
            {
                var dictionary = new Dictionary<Object?, KeyValueList>();
                
                foreach (var childPart in valueCollection)
                {
                    var key = ctx.WithContext(childPart, childContext => ((IGeneratorContext)childContext).EvaluateStatement(keyExpr));
                    if (!dictionary.TryGetValue(key, out var list))
                    {
                        list = new KeyValueList(key);
                        dictionary.Add(key, list);
                    }
                    
                    list.Values.Add(childPart);
                }

                foreach (var group in dictionary.Values.OrderBy(v=>v.Key))
                    await ctx.ExecuteAsync(part.InnerPart!, group);
            }
        }

        public Task PerformAsync(OperationTemplatePart part, ISourceSchemeContext ctx)
        {
            var sourceExpr = part.Parameters[DefaultTemplateParameterNames.Source];
            var keyExpr = part.Parameters[DefaultTemplateParameterNames.Key];
            var valueCollection = ctx.EvaluateStatement(sourceExpr, true);

            var keyValue = ctx.WithContext(valueCollection, childContext => ((ISourceSchemeContext)childContext).EvaluateStatement(keyExpr));

            return ctx.ExecuteAsync(part.InnerPart!, new KeyValueSchemeObject(keyValue, valueCollection));
        }
    }

    internal sealed class KeyValueList
    {
        internal Object? Key { get; }
        internal List<Object?> Values { get; }

        internal KeyValueList(Object? key)
        {
            Key = key;
            Values = new List<Object?>();
        }
    }
    
    internal sealed class KeyValueSchemeObject
    {
        internal Object Key { get; }
        internal Object Values { get; }

        internal KeyValueSchemeObject(Object key, Object values)
        {
            Key = key;
            Values = values;
        }
    }
}