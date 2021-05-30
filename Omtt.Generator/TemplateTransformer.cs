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

        /// <summary>
        /// Adds a new markup operation.
        /// </summary>
        /// <param name="operation">New operation</param>
        public void AddOperation(ITemplateOperation operation)
        {
            _operations[operation.Name] = operation;
        }

        /// <summary>
        /// Adds a new custom function for expressions processing.
        /// </summary>
        /// <param name="function"></param>
        public void AddFunction(IStatementFunction function)
        {
            _functions.Add(function);
        }
        
        /// <summary>
        /// Creates an instance of TemplateTransformer initialized with given template.
        /// </summary>
        /// <param name="textContent">A stream of Omtt template in string format</param>
        /// <returns>A task for receiving new Omtt instance</returns>
        public static async Task<TemplateTransformer> CreateAsync(Stream textContent)
        {
            var parser = new TemplateModelParser();
            var templateModel = await parser.ParseTemplateModelAsync(textContent);
            
            return new TemplateTransformer(templateModel);
        }

        /// <summary>
        /// Creates an instance of TemplateTransformer initialized with given template.
        /// </summary>
        /// <param name="content">Omtt template in string format</param>
        /// <returns>New Omtt instance</returns>
        public static TemplateTransformer Create(String content)
        {
            var parser = new TemplateModelParser();
            var templateModel = parser.ParseTemplateModel(content);
            
            return new TemplateTransformer(templateModel);
        }
        
        /// <summary>
        /// Transforms template with the given data and writes it to the output stream.  
        /// </summary>
        /// <param name="data">Source data object for template transformation</param>
        /// <param name="outputStream">Output stream for the result</param>
        /// <param name="createStatementContextDelegate">An optional delegate for creation a custom statement context</param>
        /// <returns></returns>
        public Task GenerateAsync(Object? data, Stream outputStream, CreateStatementContextDelegate? createStatementContextDelegate = null)
        {
            using (var streamWriter = GeneratorContext.CreateStreamWriter(outputStream))
            {
                var ctx = new GeneratorContext(streamWriter, data, createStatementContextDelegate);
                ctx.SetOperations(_operations);
                ctx.StatementContext.AddFunctions(_functions);
                return ctx.ExecuteAsync(_templateModel);
            }
        }

        /// <summary>
        /// Returns source data object scheme information that corresponds to the template.
        /// </summary>
        /// <param name="rootCtx">An optional custom source scheme context.</param>
        /// <returns></returns>
        public async Task<SourceScheme> GetSourceSchemeAsync(ISourceSchemeContext? rootCtx = null)
        {
            var ctx = rootCtx??new SourceSchemeContext();
            ctx.SetOperations(_operations);
            var result = (ctx.StatementContext.CurrentData as SourceScheme)??new SourceScheme(String.Empty, false);
            ctx.ReplaceCurrentData(result);
            await ctx.ExecuteAsync(_templateModel);
            return result;
        }
    }
}