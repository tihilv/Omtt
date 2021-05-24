using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Omtt.Generator;
using NUnit.Framework;

namespace Omtt.Tests
{
    public sealed class WorkloadTests
    {
        private const Int32 ElementCount = 1_000_000;
        [SetUp]
        public void Setup()
        {
        }

        private IEnumerable<TestItem> EnumerateBigItems()
        {
            var now = DateTime.Now;
            for (int i = 0; i < ElementCount; i++)
            {
                var iStr = i.ToString();
                yield return new TestItem()
                {
                    Amount = i,
                    Code = Guid.NewGuid().ToString(),
                    Npp = i,
                    CompanyCode = "code" + iStr,
                    ContractCode = "c_code" + iStr,
                    ContragentCode = "ctragcode" + iStr,
                    CurrencyCode = "cur"+iStr,
                    DocumentDate = now,
                    WorkplaceAccount = "wac"+iStr,
                    Payload = new Byte[80*1024]
                };
            }
        }
        
        [Test]
        public async Task EnumerableContentTest()
        {
            using (var inputStream = new MemoryStream())
            {
                using (var inputWriter = new StreamWriter(inputStream, leaveOpen:true))
                    inputWriter.WriteLine("<#<forEach source=\"this.Documents\">{{this.Npp}}#>");
                
                inputStream.Position = 0;

                TestData data = new TestData();
                data.Documents = EnumerateBigItems();
                
                TemplateTransformer generator = await TemplateTransformer.CreateAsync(inputStream);
                using (var outputStream = new MemoryStream())
                {
                    await generator.GenerateAsync(data, outputStream);
                    outputStream.Position = 0;

                    using (var streamReader = new StreamReader(outputStream))
                    {
                        var result = streamReader.ReadToEnd().Trim();
                        var ethalon = String.Join("", Enumerable.Range(0, ElementCount).Select(r => r.ToString()));
                        var mem3 = Process.GetCurrentProcess().PrivateMemorySize64;
                        Assert.AreEqual(ethalon, result);
                    }
                }
            }
        }
        
        class TestData
        {
            public IEnumerable<TestItem> Documents { get; set; }
        }
        
        class TestItem
        {
            public Int64 Npp { get; set; }

            public String Code { get; set; }

            public DateTime DocumentDate { get; set; }

            public String ContractCode { get; set; }

            public String ContragentCode { get; set; }

            public String CompanyCode { get; set; }

            public String CurrencyCode { get; set; }

            public Decimal Amount { get; set; }

            public String WorkplaceAccount { get; set; }
            
            public Byte[] Payload { get; set; }
        }

        
    }
}