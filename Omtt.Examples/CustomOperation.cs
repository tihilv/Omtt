using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;
using Omtt.Generator;
using Omtt.Generator.Extensions;

namespace Omtt.Examples
{
    public class CustomOperation
    {
        [Test]
        public async Task Base64FailExample()
        {
            var generator = TemplateTransformer.Create("Hello, <#<base64>{{this.Name}}#>!");
            try
            {
                _ = await generator.GenerateTextAsync(new {Name = "World"});
                Assert.Fail();
            }
            catch (MissingMethodException)
            {
                // no operation found
            }
        }

        [Test]
        public async Task Base64Example()
        {
            var generator = TemplateTransformer.Create("Hello, <#<base64>{{this.Name}}#>!");
            generator.AddOperation(new Base64Operation());
            var result = await generator.GenerateTextAsync(new {Name = "World"});
            Assert.AreEqual($"Hello, {Convert.ToBase64String(Encoding.UTF8.GetBytes("World"))}!", result);
        }

        [Test]
        public async Task Base64TrimExample()
        {
            var generator = TemplateTransformer.Create("Hello, <#<base64 trim=\"this.Trim\">{{this.Name}}#>!");
            generator.AddOperation(new Base64Operation());
            var result = await generator.GenerateTextAsync(new {Name = "World", Trim = true});
            Assert.AreEqual($"Hello, {Convert.ToBase64String(Encoding.UTF8.GetBytes("World")).TrimEnd('=')}!", result);
        }

        [Test]
        public async Task Base64SourceScheme()
        {
            var generator = TemplateTransformer.Create("Hello, <#<base64 trim=\"this.SomeTrim\">{{this.Name}}#>!");
            generator.AddOperation(new Base64Operation());
            var result = await generator.GetSourceSchemeAsync();
            Assert.AreEqual(" { Name, SomeTrim }", result.ToString());
        }

        private sealed class Base64Operation : ITemplateOperation
        {
            private const String TrimEqualsName = "trim";

            // Name of the operation to call in the template.
            public String Name => "base64";

            // Main transformation function.
            public async Task PerformAsync(OperationTemplatePart part, IGeneratorContext ctx)
            {
                // Check if inner part exists
                if (part.InnerPart == null)
                    throw new ArgumentNullException("Operation content is null.");

                Boolean trim = false;
                // Get the format parameter taking into account that it might be not defined.
                if (part.Parameters.TryGetValue(TrimEqualsName, out var trimExpr) && trimExpr != null)
                    trim = (Boolean) ctx.EvaluateStatement(trimExpr)!;

                using var memoryStream = new MemoryStream();
                using (ctx.OverloadStream(memoryStream)) // Overload the output to the temporary stream...
                {
                    await ctx.ExecuteAsync(part.InnerPart!); // ... and process the inner part as a template
                } // At this point the original output stream is restored

                // Read the processed inner part from the temporary stream and convert it to Base64
                memoryStream.Position = 0;
                var result = Convert.ToBase64String(memoryStream.ToArray());

                if (trim) // Apply termination trimming if needed
                    result = result.TrimEnd('=');

                // Writes the result to the output stream
                await ctx.WriteAsync(result);
            }

            // Function to implement the proper source scheme generation
            public Task PerformAsync(OperationTemplatePart part, ISourceSchemeContext ctx)
            {
                // Process parameter
                if (part.Parameters.TryGetValue(TrimEqualsName, out var formatExpr) && formatExpr != null)
                    ctx.EvaluateStatement(formatExpr);

                // Process inner part
                return ctx.ExecuteAsync(part.InnerPart!);
            }
        }
    }
}