using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Omtt.Generator;
using Omtt.Generator.Extensions;
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

            OmttListTemplate = TemplateTransformer.Create(@"
<ul id='products'>
  <#<forEach source=""this.Products"">
    <li>
      <h2>{{ this.Name }}</h2>
           Price: {{ this.Amount }}
    </li>
  #>
</ul>
");

            ScribanListTemplate = Template.Parse(@"
<ul id='products'>
  {{ for product in products }}
    <li>
      <h2>{{ product.name }}</h2>
           Price: {{ product.amount }}
    </li>
  {{ end }}
</ul>
");
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
        public async ValueTask OmttExpressionTest() => await OmttExpressionTemplate.GenerateTextAsync(ExpressionData);

        [Benchmark]
        public async ValueTask ScribanExpressionTest() => await ScribanExpressionTemplate.RenderAsync(ExpressionData);

        [Benchmark]
        public async ValueTask OmttListTest() => await OmttListTemplate.GenerateTextAsync(ListData);

        [Benchmark]
        public async ValueTask ScribanListTest()
        {
            ScribanListTemplateContext.BuiltinObject.SetValue("products", ScribanListData, false);
            ScribanListTemplateContext.PushOutput(StringBuilderOutput.GetThreadInstance());
            var result = await ScribanListTemplate.RenderAsync(ScribanListTemplateContext);
            ScribanListTemplateContext.PopOutput();
        }
    }

    record ExpressionData(Int32 intA, Int32 intB, Double DoubleA, Double DoubleB, Decimal DecimalA, Decimal DecimalB, Byte ByteA, Byte ByteB);
    record ProductData(String name, Decimal amount);

    record ProductsData(ProductData[] Products);
}