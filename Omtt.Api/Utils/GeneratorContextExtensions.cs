using System;
using Omtt.Api.StatementModel;

namespace Omtt.Api.Generation
{
    public static class GeneratorContextExtensions
    {
        public static Object? EvaluateStatement(this IGeneratorContext generatorContext, IStatement statement)
        {
            return statement.Execute(generatorContext.StatementContext);
        }
    }
}