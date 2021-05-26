using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Omtt.Api.StatementModel;
using Omtt.Generator;
using Omtt.Generator.Extensions;

namespace Omtt.Examples
{
    public class CustomFunction
    {
        [Test]
        public async Task FibFailExample()
        {
            var generator = TemplateTransformer.Create("{{this.N}}'th Fibonacci number is {{fib(this.N)}}");
            try
            {
                _ = await generator.GenerateTextAsync(new {N = 6});
                Assert.Fail();
            }
            catch (MissingMethodException)
            {
                // no operation found
            }
        }
        
        [Test]
        public async Task FibExample()
        {
            var generator = TemplateTransformer.Create("{{this.N}}'th Fibonacci number is {{fib(this.N)}}");
            generator.AddFunction(new FibonacciFunction());
            var result = await generator.GenerateTextAsync(new {N = 6});
            Assert.AreEqual("6'th Fibonacci number is 8", result);
        }

        [Test]
        public async Task FibCalculationExample()
        {
            var generator = TemplateTransformer.Create("{{this.N+1}}'th Fibonacci number is {{fib(this.N-1)+fib(this.N)}}");
            generator.AddFunction(new FibonacciFunction());
            var result = await generator.GenerateTextAsync(new {N = 6});
            Assert.AreEqual("7'th Fibonacci number is 13", result);
        }

        private class FibonacciFunction: IStatementFunction
        {
            // Name of the operation
            public String Name => "fib";

            // One argument: i'th number
            public Byte ArgumentCount => 1;

            public Object Execute(Object?[] input, IStatementContext statementContext)
            {
                // Convert the single parameter to the desired type
                var n = Convert.ToUInt32(input[0]);
                
                // Calculate n'th Fibonacci number and return
                return GetFibonacci(n);
            }

            private static UInt64 GetFibonacci(UInt32 n)
            {
                if (n < 2)
                    return n;
                UInt64 a = 0;
                UInt64 b = 1;

                for (var i = 0; i < n; i++)
                {
                    var c = a + b;
                    a = b;
                    b = c;
                }
                return a;  
            }
        }
    }       
}