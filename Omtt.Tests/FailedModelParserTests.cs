using System;
using NUnit.Framework;
using Omtt.Api.Exceptions;
using Omtt.Parser;

namespace Omtt.Tests
{
    public class FailedModelParserTests
    {
        [Test]
        public void NoCloseTagTest()
        {
            Fail<LexicalException>("<#<tag param=\"val\"qwe#>");
        }

        [Test]
        public void NoParameterValueTagTest()
        {
            Fail<LexicalException>("<#<tag param>qwe#>");
        }

        [Test]
        public void WrongParameterEqualityTest()
        {
            Fail<LexicalException>("<#<tag param=>qwe#>");
        }

        [Test]
        public void WrongParameterValueTagTest()
        {
            Fail<LexicalException>("<#<tag param='abc'>qwe#>");
        }
        
        [Test]
        public void WrongParameterContinuationTest()
        {
            Fail<LexicalException>("<#<tag param=\"abc\"=>qwe#>");
        }
        
        [Test]
        public void WrongExpressionBracesTest()
        {
            Fail<LexicalException>("<#<tag param=\"func(abc\">qwe#>");
            Fail<LexicalException>("<#<tag param=\"func(abc,def\">qwe#>");
        }
        
        [Test]
        public void WrongOperatorsTest()
        {
            Fail<LexicalException>("<#<tag param=\"abc++def\">qwe#>");
            Fail<LexicalException>("<#<tag param=\"abc+*def\">qwe#>");
            Fail<LexicalException>("<#<tag param=\"abc + * def\">qwe#>");
        }
        
        private static void Fail<T>(String content) where T:Exception
        {
            var parser = new TemplateModelParser();
            try
            {
                var t = parser.ParseTemplateModel(content);
                Assert.Fail();
            }
            catch(T)
            {
                // ok
            }
        }
    }
}