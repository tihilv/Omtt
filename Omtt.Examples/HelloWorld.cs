using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Omtt.Generator;
using Omtt.Generator.Extensions;

namespace Omtt.Examples
{
    public class HelloWorld
    {
        [Test]
        public async Task NoParamHelloWorld()
        {
            var generator = TemplateTransformer.Create("Hello, World!");
            var result = await generator.GenerateTextAsync(null);
            Assert.AreEqual("Hello, World!", result);
        }
        
        [Test]
        public async Task SingleText()
        {
            var generator = TemplateTransformer.Create("Hello, {{this}}!");
            var result = await generator.GenerateTextAsync("World");
            Assert.AreEqual("Hello, World!", result);
        }
        
        [Test]
        public async Task SimpleClass()
        {
            var generator = TemplateTransformer.Create("Hello, {{this.A}} and {{this.B}}!");
            var result = await generator.GenerateTextAsync(new {A = "Alice", B = "Bob"});
            Assert.AreEqual("Hello, Alice and Bob!", result);
        }

        [Test]
        public async Task NestedObjects()
        {
            var generator = TemplateTransformer.Create("Hello, {{this.A.Name}} and {{this.B.Name}}!");
            var result = await generator.GenerateTextAsync(new {A = new {Id = 1, Name = "Alice"}, B = new {Id = 2, Name = "Bob"}});
            Assert.AreEqual("Hello, Alice and Bob!", result);
        }

        [Test]
        public async Task Formatting()
        {
            var generator = TemplateTransformer.Create("{{this.Id|D3}}, {{this.FamilyName|u}}, {{this.Name}}");
            var result = await generator.GenerateTextAsync(new {Id = 7, Name = "James", FamilyName = "Bond"});
            Assert.AreEqual("007, BOND, James", result);
        }

        [Test]
        public async Task FormattingCulture()
        {
            var generator = TemplateTransformer.Create("{{this|F2|en}} {{this|F2|ru}}");
            var result = await generator.GenerateTextAsync(Math.PI);
            Assert.AreEqual("3.14 3,14", result);
        }
        
        [Test]
        public async Task AccessCollectionViaIndices()
        {
            var generator = TemplateTransformer.Create("Hello, {{this[0].Name}} and {{this[1].Name}}!");
            var result = await generator.GenerateTextAsync(new[] {new {Name = "Alice"}, new {Name = "Bob"}});
            Assert.AreEqual("Hello, Alice and Bob!", result);
        }

        [Test]
        public async Task ForEachCollection()
        {
            var generator = TemplateTransformer.Create("Hello, <#<forEach source=\"this\">{{this.Name}} and #>!");
            var result = await generator.GenerateTextAsync(new[] {new {Name = "Alice"}, new {Name = "Bob"}});
            Assert.AreEqual("Hello, Alice and Bob and !", result);
        }

        [Test]
        public async Task ForEachCollectionWithIf()
        {
            var generator = TemplateTransformer.Create("Hello, <#<forEach source=\"this\">{{this.Name}}<#<if clause=\"!$last\"> and #>#>!");
            var result = await generator.GenerateTextAsync(new[] {new {Name = "Alice"}, new {Name = "Bob"}});
            Assert.AreEqual("Hello, Alice and Bob!", result);
        }

        [Test]
        public async Task Grouping()
        {
            var generator = TemplateTransformer.Create("<#<group source=\"this\" key=\"this.Name\">{{this.Key}}: <#<forEach source=\"this.Values\">{{this.FamilyName}}<#<if clause=\"!$last\">, #>#>\r\n#>");
            var result = await generator.GenerateTextAsync(new[]
            {
                new {Name = "Alice", FamilyName = "Rivaz"}, 
                new {Name = "Bob", FamilyName = "Martin"},
                new {Name = "Alice", FamilyName = "Thompson"},
                new {Name = "Bob", FamilyName = "Dylan"}, 
            });
            Assert.AreEqual(
                "Alice: Rivaz, Thompson\r\n" +
                "Bob: Martin, Dylan", result);
        }
        
        [Test]
        public async Task InnerCycles()
        {
            var generator = TemplateTransformer.Create("<#<forEach source=\"this\"><#<forEach source=\"parent\">{{this*parent|||3}}#>\r\n#>");
            var result = await generator.GenerateTextAsync(Enumerable.Range(1, 5));
            Assert.AreEqual(
                "  1  2  3  4  5\r\n" +
                "  2  4  6  8 10\r\n" +
                "  3  6  9 12 15\r\n" +
                "  4  8 12 16 20\r\n" +
                "  5 10 15 20 25", result);
        }
        
        [Test]
        public async Task SchemeGeneration()
        {
            var generator = TemplateTransformer.Create("<#<forEach source=\"this.ClassesB\"> {{parent.Str}} {{this.MyInt1 + this.MyInt2}}" +
                                                       "<#<forEach source=\"this.Decimals\"> {{parent.parent.Str}} {{this}}#>" +
                                                       "#>");
            var dataStructure = await generator.GetSourceSchemeAsync();
            Assert.AreEqual(" { ClassesB[] { Decimals[], MyInt1, MyInt2 }, Str }", dataStructure.ToString());
        }

    }
}