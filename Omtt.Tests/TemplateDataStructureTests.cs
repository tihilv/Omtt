using System.Threading.Tasks;
using Omtt.Generator;
using NUnit.Framework;

namespace Omtt.Tests
{
    public sealed class TemplateDataStructureTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task GetSimpleDataStructureTest()
        {
            var generator = TemplateTransformer.Create("Hello {{this.SomeString}} = 4. {{this.SomeDate|s}}");
            var dataStructure = await generator.GetSourceSchemeAsync();

            Assert.AreEqual(" { SomeDate, SomeString }", dataStructure.ToString());
        }

        [Test]
        public async Task GetInnerDataStructureTest()
        {
            var generator = TemplateTransformer.Create("{{this.Str}} {{this.ClassesB[1].MyInt|D3}} {{this.ClassesB[0].Decimals[1]}}");
            var dataStructure = await generator.GetSourceSchemeAsync();

            Assert.AreEqual(" { ClassesB[] { Decimals[], MyInt }, Str }", dataStructure.ToString());
        }

        [Test]
        public async Task GetForEachDataStructureTest()
        {
            var generator = TemplateTransformer.Create("{{this.Str}}<#<forEach source=\"this.ClassesB\"> {{parent.Str}} {{this.MyInt}}" +
                                                               "<#<forEach source=\"this.Decimals\"> {{parent.parent.Str}} {{this}}#>" +
                                                               "#>");
            var dataStructure = await generator.GetSourceSchemeAsync();

            Assert.AreEqual(" { ClassesB[] { Decimals[], MyInt }, Str }", dataStructure.ToString());
        }

        [Test]
        public async Task GetIfDataStructureTest()
        {
            var generator = TemplateTransformer.Create("<#<if clause=\"this.MyInt > 3\">{{this.MyString}}>");
            var dataStructure = await generator.GetSourceSchemeAsync();

            Assert.AreEqual(" { MyInt, MyString }", dataStructure.ToString());
        }

        [Test]
        public async Task GetGroupsDataStructureTest()
        {
            var generator = TemplateTransformer.Create("<#<group source=\"this.ClassesB\" key=\"this.MyInt\"> {{this.Key}}" +
                                                   "<#<forEach source=\"this.Values\"> {{this.Decimals[0]}}{{parent.parent.Str}}#>" +
                                                   "#>");
            var dataStructure = await generator.GetSourceSchemeAsync();

            Assert.AreEqual(" { ClassesB[] { Decimals[], MyInt }, Str }", dataStructure.ToString());
        }

        [Test]
        public async Task GetIndexArraysAsParamDataStructureTest()
        {
            var generator = TemplateTransformer.Create("Hello {{this.SomeStrings[this.SomeInt]}} = 4.");
            var dataStructure = await generator.GetSourceSchemeAsync();

            Assert.AreEqual(" { SomeInt, SomeStrings[] }", dataStructure.ToString());
        }

        [Test]
        public async Task GetLetOperationDataStructureTest()
        {
            var generator = TemplateTransformer.Create("{{let this.ClassesB[0].Decimals[1] = 11; let this.ClassesB[1].MyInt=444;}}");
            var dataStructure = await generator.GetSourceSchemeAsync();

            Assert.AreEqual(" { ClassesB[] { Decimals[], MyInt } }", dataStructure.ToString());
        }

        [Test]
        public async Task SetLetOperationDataStructureTest()
        {
            var generator = TemplateTransformer.Create("<#<if clause=\"let test = this.ClassesB[0]; this.ClassesB[1].MyInt=444\">{{test.Decimals[1]}}#>");
            var dataStructure = await generator.GetSourceSchemeAsync();

            Assert.AreEqual(" { ClassesB[] { Decimals[], MyInt } }", dataStructure.ToString());
        }

        [Test]
        public async Task GetFunctionDataStructureTest()
        {
            var generator = TemplateTransformer.Create("Hello {{Func1(this.SomeStrings[Func2(this.SomeInt)])}}");
            var dataStructure = await generator.GetSourceSchemeAsync();

            Assert.AreEqual(" { SomeInt, SomeStrings[] }", dataStructure.ToString());
        }
        
        [Test]
        public async Task GetCodeDataStructureTest()
        {
            var generator = TemplateTransformer.Create("<#<code source=\"let tmp = this.SomeInt;\">{{this.SomeString}}#>");
            var dataStructure = await generator.GetSourceSchemeAsync();

            Assert.AreEqual(" { SomeInt, SomeString }", dataStructure.ToString());
        }
        
        [Test]
        public async Task FragmentDataStructureTest()
        {
            var generator = TemplateTransformer.Create("<#<fragment type=\"this.SomeType;\">{{this.SomeString}}#>");
            var dataStructure = await generator.GetSourceSchemeAsync();

            Assert.AreEqual(" { SomeString, SomeType }", dataStructure.ToString());
        }

    }
}