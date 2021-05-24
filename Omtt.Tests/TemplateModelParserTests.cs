using System;
using Omtt.Parser;
using NUnit.Framework;
using Omtt.Api.TemplateModel;

namespace Omtt.Tests
{
    public sealed class TemplateModelParserTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SimpleContentTest()
        {
            String content = "   Simnple \ncontent   ";
            var parser = new TemplateModelParser();
            var result = parser.ParseTemplateModel(content);

            var textContent = result as TextTemplatePart;
            Assert.AreEqual(content, textContent.Text);
        }
        
        [Test]
        public void SimpleBlockTest()
        {
            String content = "<#<mbl>#>";
            var parser = new TemplateModelParser();
            var result = parser.ParseTemplateModel(content);

            var operationContent = result as OperationTemplatePart;
            Assert.AreEqual("mbl", operationContent.OperationName);
            Assert.AreEqual(0, operationContent.Parameters.Count);
            Assert.Null(operationContent.InnerPart);
        }

        [Test]
        public void SimpleBlockWithSpacesTest()
        {
            String content = "<#<mbl      >#>";
            var parser = new TemplateModelParser();
            var result = parser.ParseTemplateModel(content);

            var operationContent = result as OperationTemplatePart;
            Assert.AreEqual("mbl", operationContent.OperationName);
            Assert.AreEqual(0, operationContent.Parameters.Count);
            Assert.Null(operationContent.InnerPart);
        }

        
        [Test]
        public void ParameterBlockTest()
        {
            String content = "<#<mbl p1=\"this\">#>";
            var parser = new TemplateModelParser();
            var result = parser.ParseTemplateModel(content);

            var operationContent = result as OperationTemplatePart;
            Assert.AreEqual("mbl", operationContent.OperationName);
            Assert.AreEqual(1, operationContent.Parameters.Count);
            Assert.NotNull(operationContent.Parameters["p1"]);
            Assert.Null(operationContent.InnerPart);
        }

        [Test]
        public void ParametersBlockTest()
        {
            String content = "<#<mbl p1=\"this\" p2=\"env\">#>";
            var parser = new TemplateModelParser();
            var result = parser.ParseTemplateModel(content);

            var operationContent = result as OperationTemplatePart;
            Assert.AreEqual("mbl", operationContent.OperationName);
            Assert.AreEqual(2, operationContent.Parameters.Count);
            Assert.NotNull(operationContent.Parameters["p1"]);
            Assert.NotNull(operationContent.Parameters["p2"]);
            Assert.Null(operationContent.InnerPart);
        }
        
        [Test]
        public void ParametersBlockWithSpacesTest()
        {
            String content = "<#<mbl    p1   =  \"this\"  p2 =  \"env\"  >#>";
            var parser = new TemplateModelParser();
            var result = parser.ParseTemplateModel(content);

            var operationContent = result as OperationTemplatePart;
            Assert.AreEqual("mbl", operationContent.OperationName);
            Assert.AreEqual(2, operationContent.Parameters.Count);
            Assert.NotNull(operationContent.Parameters["p1"]);
            Assert.NotNull(operationContent.Parameters["p2"]);
            Assert.Null(operationContent.InnerPart);
        }
        
        [Test]
        public void CombinedBlockTest()
        {
            String content1 = " Some text 1 ";
            String content2 = "  Some  text  2  ";
            String content = $"{content1}<#<mbl>#>{content2}";
            
            var parser = new TemplateModelParser();
            var result = parser.ParseTemplateModel(content);

            var combinedTemplateContent = result as CombinedTemplatePart;
            Assert.AreEqual(3, combinedTemplateContent.Children.Length);
            
            Assert.AreEqual(content1, ((TextTemplatePart)combinedTemplateContent.Children[0]).Text);
            Assert.AreEqual(content2, ((TextTemplatePart)combinedTemplateContent.Children[2]).Text);
            
            Assert.AreEqual("mbl", ((OperationTemplatePart)combinedTemplateContent.Children[1]).OperationName);
        }

        [Test]
        public void ExpressionBlockTest()
        {
            String content1 = " Some text 1 ";
            String content2 = "  Some  text  2  ";
            String content = content1+"{{this}}"+content2;
            
            var parser = new TemplateModelParser();
            var result = parser.ParseTemplateModel(content);

            var combinedTemplateContent = result as CombinedTemplatePart;
            Assert.AreEqual(3, combinedTemplateContent.Children.Length);
            
            Assert.AreEqual(content1, ((TextTemplatePart)combinedTemplateContent.Children[0]).Text);
            Assert.AreEqual(content2, ((TextTemplatePart)combinedTemplateContent.Children[2]).Text);
            
            Assert.AreEqual("write", ((OperationTemplatePart)combinedTemplateContent.Children[1]).OperationName);
        }
        
        [Test]
        public void InnerBlockTest()
        {
            String innerContent = "  Some  text  2  ";
            String content = $"<#<mbl>{innerContent}#>";
            var parser = new TemplateModelParser();
            var result = parser.ParseTemplateModel(content);

            var operationContent = result as OperationTemplatePart;
            Assert.AreEqual("mbl", operationContent.OperationName);
            Assert.AreEqual(0, operationContent.Parameters.Count);
            Assert.AreEqual(innerContent, ((TextTemplatePart)operationContent.InnerPart).Text);
        }
        
        [Test]
        public void EndingTest()
        {
            String content = "A{{this}}B";
            var parser = new TemplateModelParser();
            var result = parser.ParseTemplateModel(content);

            var combined = result as CombinedTemplatePart;
            Assert.NotNull(combined);
            Assert.AreEqual(3, combined.Children.Length);
            Assert.AreEqual("A", (combined.Children[0] as TextTemplatePart)?.Text);
            Assert.AreEqual("B", (combined.Children[2] as TextTemplatePart)?.Text);
        }

    }
}