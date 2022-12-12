using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Omtt.Generator;
using NUnit.Framework;
using Omtt.Api.DataModel;
using Omtt.Generator.Extensions;

namespace Omtt.Tests
{
    public sealed class FinalGeneratorTests
    {
        [SetUp]
        public void Setup()
        {
        }

        private static MemoryStream GetInputStream(String content)
        {
            var inputStream = new MemoryStream();

            using (var inputWriter = new StreamWriter(inputStream, leaveOpen:true))
                inputWriter.WriteLine(content);
            inputStream.Position = 0;
            
            return inputStream;
        }

        [Test]
        public async Task SimpleContentTest()
        {
            using (var inputStream = GetInputStream("Hello <#<write source=\"1+3\">#> = 4"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                using (var outputStream = new MemoryStream())
                {
                    await generator.GenerateAsync(null, outputStream);
                    outputStream.Position = 0;

                    using (var streamReader = new StreamReader(outputStream))
                    {
                        var result = streamReader.ReadToEnd().Trim();
                        Assert.AreEqual("Hello 4 = 4", result);
                    }
                }
            }
        }
        
        [Test]
        public async Task SimpleExpressionContentTest()
        {
            using (var inputStream = GetInputStream("Hello {{2*2}} = 4"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(null);

                Assert.AreEqual("Hello 4 = 4", result);
            }
        }
        
        [Test]
        public async Task SimpleErrorExpressionContentTest()
        {
            using (var inputStream = GetInputStream("Hello {{this.SomethingStrange}} = 4"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                using (var outputStream = new MemoryStream())
                {
                    try
                    {
                        await generator.GenerateAsync(new Object(), outputStream);
                        Assert.Fail();
                    }
                    catch (ArgumentException)
                    {
                        // ok
                    }
                }
            }
        }

        [Test]
        public async Task FormatterEscapingTest1()
        {
            using (var inputStream = GetInputStream("<test>{{this.Str}}</test>"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(new TestClassA() {Str = "<SI>"});
                Assert.AreEqual($"<test><SI></test>", result);
            }
        }

        [Test]
        public async Task FormatterEscapingTest2()
        {
            using (var inputStream = GetInputStream("<#<fragment type=\"'xml'\"><test>{{this.Str}}</test>#>"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(new TestClassA() {Str = "<SI>"});
                Assert.AreEqual($"<test>&lt;SI&gt;</test>", result);
            }
        }

        [Test]
        public async Task FormatterEscapingTest3()
        {
            using (var inputStream = GetInputStream("<#<fragment type=\"'html'\"><test>{{this.Str}}</test>#>"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(new TestClassA() {Str = "<SI>"});
                Assert.AreEqual($"<test>&lt;SI&gt;</test>", result);
            }
        }

        [Test]
        public async Task LowerUpperCaseTest()
        {
            using (var inputStream = GetInputStream("{{'AbCd'|l}} {{'AbCd'|u}}"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(null);
                Assert.AreEqual($"abcd ABCD", result);
            }
        }

        private static TestClassA PrepareData()
        {
            return new TestClassA()
            {
                Str = "ASDF",
                ClassesB = { new TestClassB()
                    {
                        MyInt = 4,
                        Decimals = new [] {1.3m ,5.5m}
                    },
                    new TestClassB()
                    {
                        MyInt = 8,
                        Decimals = new [] {2.2m ,11.9m}
                    }  
                },
            };
        }

        private static TestClassA PrepareDataForGroups()
        {
            return new TestClassA()
            {
                Str = "ASDF",
                ClassesB =
                {
                    new TestClassB()
                    {
                        MyInt = 4,
                        Decimals = new[] {1m}
                    },
                    new TestClassB()
                    {
                        MyInt = 5,
                        Decimals = new[] {2m}
                    },
                    new TestClassB()
                    {
                        MyInt = 4,
                        Decimals = new[] {3m}
                    }
                },
            };
        }


        [Test]
        public async Task ReadDataTest()
        {
            var ths = PrepareData();

            using (var inputStream = GetInputStream("{{this.Str}} {{this.ClassesB[1].MyInt|D3}} {{this.ClassesB[0].Decimals[1]}}"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(ths);
                Assert.AreEqual($"ASDF 008 {5.5m}", result);
            }
        }

        [Test]
        public async Task IntFormat1Test()
        {
            var ths = PrepareData();

            using (var inputStream = GetInputStream("{{this.ClassesB[0].Decimals[1]||en}}"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(ths);
                Assert.AreEqual($"5.5", result);
            }
        }

        [Test]
        public async Task IntFormat2Test()
        {
            var ths = PrepareData();

            using (var inputStream = GetInputStream("{{this.ClassesB[0].Decimals[1]||ru}}"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(ths);
                Assert.AreEqual($"5,5", result);
            }
        }

        [Test]
        public async Task IntAlign1Test()
        {
            var ths = PrepareData();

            using (var inputStream = GetInputStream("|{{this.ClassesB[0].Decimals[1]||ru|5}}|"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(ths);
                Assert.AreEqual($"|  5,5|", result);
            }
        }

        [Test]
        public async Task IntAlign2Test()
        {
            var ths = PrepareData();

            using (var inputStream = GetInputStream("|{{this.ClassesB[0].Decimals[1]||ru|-5}}|"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(ths);
                Assert.AreEqual($"|5,5  |", result);
            }
        }
        
        [Test]
        public async Task ReadTemplateDataTest()
        {
            var templateData = PrepareData();

            using (var inputStream = GetInputStream("{{this.Str}} {{this.ClassesB[1].MyInt}} {{this.ClassesB[0].Decimals[1]}}"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(templateData);
                Assert.AreEqual($"ASDF 8 {5.5m}", result);
            }
        }

        [Test]
        public async Task IfTemplateTest()
        {
            var ths = new TestClassB() {MyInt = 3};

            using (var inputStream = GetInputStream("<#<if clause=\"this.MyInt > 3\">abc#>"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(ths);
                Assert.AreEqual($"", result);

                ths.MyInt = 4;
                result = await generator.GenerateTextAsync(ths);
                Assert.AreEqual($"abc", result);
            }
        }

        [Test]
        public async Task ForEachTemplateTest()
        {
            var templateData = PrepareData();

            using (var inputStream = GetInputStream("{{this.Str}}<#<forEach source=\"this.ClassesB\"> {{parent.Str}} {{this.MyInt}}" +
                                                    "<#<forEach source=\"this.Decimals\"> {{parent.parent.Str}} {{this}}#>" +
                                                    "#>"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(templateData);
                Assert.AreEqual($"ASDF ASDF 4 ASDF {1.3m} ASDF {5.5m} ASDF 8 ASDF {2.2m} ASDF {11.9m}", result);
            }
        }

        [Test]
        public async Task ForEachVariablesTest()
        {
            using (var inputStream = GetInputStream("<#<forEach source=\"this\"><#<if clause=\"$first\">!#>{{this}}<#<if clause=\"!$last\">, #>#>"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(Enumerable.Range(0, 10));
                Assert.AreEqual($"!0, 1, 2, 3, 4, 5, 6, 7, 8, 9", result);
            }
        }
        
        [Test]
        public async Task DistinctTest()
        {
            using (var inputStream = GetInputStream("<#<forEach source=\"this\"><#<distinct>{{this.A}}#><#<distinct>{{this.B}}#>#>"))
            {
                var list = new List<TestClassC>() {
                    new() {A = 1, B = "q"},
                    new() {A = 2, B = "q"},
                    new() {A = 2, B = "q"},
                    new() {A = 2, B = "w"},
                };
                
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(list);
                Assert.AreEqual($"1q2w", result);
            }
        }
        
        [Test]
        public async Task LinkedDistinctTest()
        {
            using (var inputStream = GetInputStream("<#<forEach source=\"this\"><#<distinct link=\"1\">{{this.A}}#><#<distinct>{{this.B}}#><#<distinct link=\"0\">{{this.C}}#><#<distinct>{{this.D}}#>#>"))
            {
                var list = new List<TestClassC>() {
                    new() {A = 1, B = "q", C = 1, D = 1},
                    new() {A = 2, B = "q", C = 1, D = 1},
                    new() {A = 2, B = "q", C = 1, D = 1},
                    new() {A = 2, B = "w", C = 1, D = 1},
                };
                
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(list);
                Assert.AreEqual($"1q112q1w1", result);
            }
        }
        
        [Test]
        public async Task DistinctBySourceTest()
        {
            using (var inputStream = GetInputStream("<#<forEach source=\"this\"><#<distinct>{{this.A}}#><#<distinct source=\"this.A\">{{this.B}}#>#>"))
            {
                var list = new List<TestClassC>() {
                    new() {A = 1, B = "q"},
                    new() {A = 2, B = "q"},
                    new() {A = 2, B = "q"},
                    new() {A = 2, B = "w"},
                };
                
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(list);
                Assert.AreEqual($"1q2q", result);
            }
        }
                
        [Test]
        public async Task SortTest()
        {
            using (var inputStream = GetInputStream("<#<sort source=\"this\" clause1=\"this.A\" clause2=\"this.B\" clause3-=\"this.C\"><#<forEach source=\"this\">{{this.D}}#>#>"))
            {
                var list = new List<TestClassC>() {
                    new() {A = 1, B = "6", C = 1, D = 1},
                    new() {A = 2, B = "7", C = 2, D = 2},
                    new() {A = 2, B = null, C = 3, D = 3},
                    new() {A = 2, B = "7", C = 4, D = 4},
                };
                
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(list);
                Assert.AreEqual($"1342", result);
            }
        }
        
        [Test]
        public async Task GroupTemplateTest()
        {
            var templateData = PrepareDataForGroups();

            using (var inputStream = GetInputStream("<#<group source=\"this.ClassesB\" key=\"this.MyInt\"> {{this.Key}}" +
                                                    "<#<forEach source=\"this.Values\"> {{this.Decimals[0]}}#>" +
                                                    "#>"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(templateData);
                Assert.AreEqual($" 4 1 3 5 2", result);
            }
        }
        
        [Test]
        public async Task CodeInCycleTest()
        {
            var templateData = new List<TestClassA>();
            templateData.Add(new TestClassA(){Str = "a"});
            templateData.Add(new TestClassA(){Str = "b"});
            templateData.Add(new TestClassA(){Str = "c"});

            using (var inputStream = GetInputStream("<#<code source=\"let tmp = 2;\">" +
                                                    "<#<forEach source=\"this\"><#<code source=\"let tmp = tmp+1;\">#>{{tmp}}{{this.Str}}" +
                                                    "#>#>"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(templateData);
                Assert.AreEqual($"3a4b5c", result);
            }
        }
        
        [Test]
        public async Task WriteDataTest()
        {
            var ths = PrepareData();
            
            using (var inputStream = GetInputStream("{{let this.ClassesB[0].Decimals[1] = 11; let this.ClassesB[1].MyInt=444;}}"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                using (var outputStream = new MemoryStream())
                {
                    await generator.GenerateAsync(ths, outputStream);
                    outputStream.Position = 0;

                    using (var streamReader = new StreamReader(outputStream))
                    {
                        var result = streamReader.ReadToEnd().Trim();
                        Assert.AreEqual(11, ths.ClassesB[0].Decimals[1]);
                        Assert.AreEqual(444, ths.ClassesB[1].MyInt);
                    }
                }
            }
        }
        
        [Test]
        public async Task PropertySetObjectTest()
        {
            using (var inputStream = GetInputStream("{{this.Str1}} {{this.SubObj.Str1}} {{this.SubObj.Str2}}"))
            {
                var testObject = new PropertySetObject {["Str1"] = "A", ["SubObj"] = new PropertySetObject {["Str1"] = "B", ["Str2"] = "C"}};

                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(testObject);
                Assert.AreEqual("A B C", result);
            }
        }

        [Test]
        public async Task PropertySetObjectCloneTest()
        {
            var templateData = new PropertySetObject(PrepareData());

            using (var inputStream = GetInputStream("{{this.Str}} {{this.ClassesB[1].MyInt}} {{this.ClassesB[0].Decimals[1]}}"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(templateData);
                Assert.AreEqual($"ASDF 8 {5.5m}", result);
            }
        }
        
        [Test]
        public async Task DictionaryStringObjectTest()
        {
            var model = new { Dict = new Dictionary<string, string>() { ["A"] = "first", ["B"] = "second" } };
            using (var inputStream = GetInputStream("{{this.Dict['B']}} {{this.Dict['A']}}"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(model);
                Assert.AreEqual("second first", result);
            }
        }

        [Test]
        public async Task DictionaryIntegerObjectTest()
        {
            var model = new { Dict = new Dictionary<int, string>() { [5] = "first", [9] = "second" } };
            using (var inputStream = GetInputStream("{{this.Dict[9]}} {{this.Dict[5]}}"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(model);
                Assert.AreEqual("second first", result);
            }
        }
        
        [Test]
        public async Task TimeZoneTemplateTest()
        {
            var timeZone = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(z => z.BaseUtcOffset == TimeSpan.FromHours(8));
            var ths = new TestClassD() { MyDateTime = new DateTime(2000, 1, 1, 10, 15, 25, DateTimeKind.Utc), TimeZoneInfo = timeZone};

            using (var inputStream = GetInputStream("<#<timeZone key=\"this.TimeZoneInfo\">{{this.MyDateTime|HH:mm:ss|de}}#>"))
            {
                var generator = await TemplateTransformer.CreateAsync(inputStream);
                var result = await generator.GenerateTextAsync(ths);
                Assert.AreEqual($"18:15:25", result);

                ths.MyDateTime = new DateTime(2000, 1, 1, 10, 15, 25, DateTimeKind.Local);
                result = await generator.GenerateTextAsync(ths);
                Assert.AreEqual($"10:15:25", result);
            }
        }
        
        class TestClassA
        {
            public String Str { get; set; }
            public List<TestClassB> ClassesB { get; } = new List<TestClassB>();
        }
        
        class TestClassB
        {
            public Int32 MyInt { get; set; }
            public Decimal[] Decimals { get; set; }
        }

        class TestClassC
        {
            public Int32 A { get; set; }
            public String B { get; set; }
            public Byte C { get; set; }
            public Int32 D { get; set; }
        }
        
        class TestClassD
        {
            public DateTime MyDateTime { get; set; }
            public TimeZoneInfo TimeZoneInfo { get; set; }
        }
    }
}