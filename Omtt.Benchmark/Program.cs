using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Omtt.Generator;
using Scriban;
using Scriban.Runtime;

namespace Omtt.Benchmark
{
    [MemoryDiagnoser]
    public class Program
    {
        private static readonly TemplateTransformer OmttExpressionTemplate;
        private static readonly Template ScribanExpressionTemplate;
        private static readonly ExpressionData ExpressionData;

        private static readonly String OmttListTemplateText;
        private static readonly String ScribanListTemplateText;
        private static readonly TemplateTransformer OmttListTemplate;
        private static readonly Template ScribanListTemplate;
        private static readonly ProductsData ListData;
        private static readonly List<ScriptObject> ScribanListData;
        private static readonly TemplateContext ScribanListTemplateContext;

        static Program()
        {
            OmttExpressionTemplate = TemplateTransformer.Create("{{this.IntA + this.IntB+this.IntA + this.IntB}} {{this.DoubleA+this.DoubleB+this.DoubleA+this.DoubleB}} {{this.DecimalA+this.DecimalB+this.DecimalA+this.DecimalB}}");
            ScribanExpressionTemplate = Template.Parse("{{int_a + int_b+int_a + int_b}} {{double_a+double_b+double_a+double_b}} {{decimal_a+decimal_b+decimal_a+decimal_b}}");
            ExpressionData = new ExpressionData(12, 19, 11.3, 19.4, 1111m, 2222m, 34, 45);

            OmttListTemplateText = @"
<ul id='products'>
  <#<forEach source=""this.Products"">
    <li>
      <h2>{{ this.Name }}</h2>
           Price: {{ this.Amount }}
    </li>
  #>
</ul>
";

            ScribanListTemplateText = @"
<ul id='products'>
  {{ for product in products }}
    <li>
      <h2>{{ product.name }}</h2>
           Price: {{ product.amount }}
    </li>
  {{ end }}
</ul>
";
            
            OmttListTemplate = TemplateTransformer.Create(OmttListTemplateText);

            ScribanListTemplate = Template.Parse(ScribanListTemplateText);
            ListData = new ProductsData(new ProductData[1000]);
            ScribanListData = new List<ScriptObject>(ListData.Products.Length);
            for (int i = 0; i < ListData.Products.Length; i++)
            {
                ListData.Products[i] = new ProductData("Product " + i, i * 101.5m);

                var obj = new ScriptObject {["name"] = ListData.Products[i].name, ["amount"] = ListData.Products[i].amount};
                ScribanListData.Add(obj);
            }

            ScribanListTemplateContext = new TemplateContext();
        }

        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
        }

        [Benchmark]
        public async ValueTask OmttExpressionTest()
        {
            using (var stream = new MemoryStream())
                await OmttExpressionTemplate.GenerateAsync(ExpressionData, stream);
        }

        [Benchmark]
        public async ValueTask ScribanExpressionTest() => await ScribanExpressionTemplate.RenderAsync(ExpressionData);

        [Benchmark]
        public async ValueTask OmttListTest()
        {
            using (var stream = new MemoryStream())
                await OmttListTemplate.GenerateAsync(ListData, stream);
        }

        [Benchmark]
        public async ValueTask ScribanListTest()
        {
            ScribanListTemplateContext.BuiltinObject.SetValue("products", ScribanListData, false);
            ScribanListTemplateContext.PushOutput(StringBuilderOutput.GetThreadInstance());
            _ = await ScribanListTemplate.RenderAsync(ScribanListTemplateContext);
            ScribanListTemplateContext.PopOutput();
        }

        [Benchmark]
        public async ValueTask ScribanListWithPreparationTest()
        {
            var scribanListData = new List<ScriptObject>(ListData.Products.Length);
            for (int i = 0; i < ListData.Products.Length; i++)
            {
                var obj = new ScriptObject {["name"] = ListData.Products[i].name, ["amount"] = ListData.Products[i].amount};
                scribanListData.Add(obj);
            }

            var scribanListTemplateContext = new TemplateContext();
            scribanListTemplateContext.BuiltinObject.SetValue("products", scribanListData, false);
            scribanListTemplateContext.PushOutput(StringBuilderOutput.GetThreadInstance());
            _ = await ScribanListTemplate.RenderAsync(scribanListTemplateContext);
            scribanListTemplateContext.PopOutput();
        }

        [Benchmark]
        public void OmttParsingTest() => TemplateTransformer.Create(OmttListTemplateText);

        [Benchmark]
        public void ScribanParsingTest() => Template.Parse(ScribanListTemplateText);

    }

    record ExpressionData(Int32 IntA, Int32 IntB, Double DoubleA, Double DoubleB, Decimal DecimalA, Decimal DecimalB, Byte ByteA, Byte ByteB);
    record ProductData(String name, Decimal amount);

    record ProductsData(ProductData[] Products);
}