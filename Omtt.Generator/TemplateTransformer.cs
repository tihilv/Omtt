using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Omtt.Api.DataModel;
using Omtt.Api.Generation;
using Omtt.Api.StatementModel;
using Omtt.Api.TemplateModel;
using Omtt.Generator.Contexts;
using Omtt.Generator.Operations;
using Omtt.Parser;

namespace Omtt.Generator
{
    public sealed class TemplateTransformer
    {
        private readonly ITemplatePart _templateModel;
        private readonly Dictionary<String, ITemplateOperation> _operations;
        private readonly List<IStatementFunction> _functions;

        private TemplateTransformer(ITemplatePart? templateModel)
        {
            _templateModel = templateModel ?? throw new ArgumentNullException(nameof(templateModel));
            
            _operations = new Dictionary<String, ITemplateOperation>();
            _functions = new List<IStatementFunction>();

            GetDefaultOperations();
        }

        private void GetDefaultOperations()
        {
            AddOperation(new WriteOperation());
            AddOperation(new IfOperation());
            AddOperation(new ForEachOperation());
            AddOperation(new GroupOperation());
            AddOperation(new FragmentOperation());
            AddOperation(new CodeOperation());
        }

        public void AddOperation(ITemplateOperation operation)
        {
            _operations[operation.Name] = operation;
        }

        public void AddFunction(IStatementFunction function)
        {
            _functions.Add(function);
        }
        
        public static async Task<TemplateTransformer> CreateAsync(Stream textContent)
        {
            var parser = new TemplateModelParser();
            var templateModel = await parser.ParseTemplateModelAsync(textContent);
            
            return new TemplateTransformer(templateModel);
        }

        public static TemplateTransformer Create(String content)
        {
            var parser = new TemplateModelParser();
            var templateModel = parser.ParseTemplateModel(content);
            
            return new TemplateTransformer(templateModel);
        }
        
        public Task GenerateAsync(Object? data, Stream outputStream, CreateStatementContextDelegate? createStatementContextDelegate = null)
        {
            var ctx = new GeneratorContext(outputStream, createStatementContextDelegate);
            ctx.SetOperations(_operations);
            ctx.StatementContext.AddFunctions(_functions);
            return ctx.ExecuteAsync(_templateModel, data);
        }

        public async Task<SourceScheme> GetSourceSchemeAsync(ISourceSchemeContext? rootCtx = null)
        {
            var ctx = rootCtx??new SourceSchemeContext();
            ctx.SetOperations(_operations);
            var result = (ctx.SourceData as SourceScheme)??new SourceScheme(String.Empty, false);
            await ctx.ExecuteAsync(_templateModel, result);
            return result;
        }
    }
}