using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Omtt.Generator;
using Omtt.Generator.Extensions;

namespace Omtt.Benchmark
{
    [MemoryDiagnoser]
    public class Program
    {
        private static readonly TemplateTransformer _templateTransformer;
        private static readonly TestData _testData;

        static Program()
        {
            _templateTransformer = TemplateTransformer.Create("{{this.IntA + this.IntB+this.IntA + this.IntB}} {{this.DoubleA+this.DoubleB+this.DoubleA+this.DoubleB}} {{this.DecimalA+this.DecimalB+this.DecimalA+this.DecimalB}} {{this.ByteA+this.ByteB+this.ByteA+this.ByteB}}");
            _testData = new TestData(12, 19, 11.3, 19.4, 1111m, 2222m, 34, 45);
        }

        static void Main(string[] args)
        {
            //var result = _templateTransformer.GenerateTextAsync(_testData).Result;
            
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
        }

        [Benchmark]
        public Task TestMath() => _templateTransformer.GenerateTextAsync(_testData);
    }

    record TestData(Int32 intA, Int32 intB, Double DoubleA, Double DoubleB, Decimal DecimalA, Decimal DecimalB, Byte ByteA, Byte ByteB);
}