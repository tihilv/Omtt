using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.StatementModel;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Operations
{
    internal sealed class SortOperation: ITemplateOperation
    {
        public String Name => "sort";
        
        public async Task PerformAsync(OperationTemplatePart part, IGeneratorContext ctx)
        {
            if (part.InnerPart == null)
                throw new ArgumentNullException("Operation content is null.");
            
            var sourceExpr = part.Parameters[DefaultTemplateParameterNames.Source];
            var clauseExprs = part.Parameters
                .Where(p => p.Key.StartsWith(DefaultTemplateParameterNames.Clause))
                .Select(k=>new SortTuple(k.Key, k.Value))
                .OrderBy(k => k.Index).ToArray();

            var dataToSort = new List<(Object, Object?[])>();
            if (ctx.EvaluateStatement(sourceExpr) is IEnumerable valueCollection)
            {
                foreach (var item in valueCollection)
                {
                    await ctx.WithContext(null, childCtx =>
                    {
                        var childPart = item;
                        childCtx.ReplaceCurrentData(childPart);
                        Object?[] sortFields = new Object[clauseExprs.Length];
                        for (var index = 0; index < clauseExprs.Length; index++)
                            sortFields[index] = childCtx.EvaluateStatement(clauseExprs[index].Statement);

                        dataToSort.Add((childPart, sortFields));
                        return Task.CompletedTask;
                    });
                }
            }
            
            dataToSort.Sort(new TableComparer(clauseExprs));

            await ctx.WithContext(null, async childCtx =>
            {
                childCtx.ReplaceCurrentData(dataToSort.Select(d => d.Item1));
                await childCtx.ExecuteAsync(part.InnerPart!);
            });
        }

        public Task PerformAsync(OperationTemplatePart part, ISourceSchemeContext ctx)
        {
            foreach (var param in part.Parameters)
                ctx.EvaluateStatement(param.Value);
            
            return ctx.ExecuteAsync(part.InnerPart!);
        }

        internal struct SortTuple
        {
            public readonly Int32 Index;
            public readonly Boolean Reverse;
            public readonly IStatement Statement;

            public SortTuple(String key, IStatement statement)
            {
                Index = Int32.Parse(key.Substring(DefaultTemplateParameterNames.Clause.Length).TrimEnd('-'));
                Reverse = key[key.Length - 1] == '-';
                Statement = statement;
            }
        }
        
        internal class TableComparer : IComparer<(object, object?[])>
        {
            private readonly SortTuple[] _clauseExprs;

            internal TableComparer(SortTuple[] clauseExprs)
            {
                _clauseExprs = clauseExprs;
            }

            public Int32 Compare((Object, Object?[]) x, (Object, Object?[]) y)
            {
                for (int i = 0; i < x.Item2.Length; i++)
                {
                    var result = DoCompare(x.Item2[i], y.Item2[i]);
                    if (result != 0)
                    {
                        if (_clauseExprs[i].Reverse)
                            result = -result;
                        return result;
                    }
                }

                return 0;
            }

            private Int32 DoCompare(Object? x, Object? y)
            {
                if (x == null && y == null)
                    return 0;

                if (x == null)
                    return -1;

                if (y == null)
                    return 1;

                if (x is IComparable comparable)
                    return comparable.CompareTo(y);

                throw new ArgumentException("Unable to compare items.");
            }
        }
    }

    
}