using System;
using Omtt.Api.StatementModel;

namespace Omtt.Api.Generation
{
    public static class GeneratorContextExtensions
    {
        public static Object? EvaluateStatement(this IProcessingContext generatorContext, IStatement? statement)
        {
            if (statement == null)
                return null;
            
            return statement.Execute(generatorContext.StatementContext);
        }
    }
}