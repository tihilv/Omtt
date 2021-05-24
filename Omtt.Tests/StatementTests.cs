using System;
using System.Collections.Generic;
using Omtt.Statements;
using NUnit.Framework;

namespace Omtt.Tests
{
    public sealed class StatementTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SimpleStatementTest()
        {
            var statement = StatementParser.Parse("1+3+0");
            Assert.True(statement is SingleStatement);
            
            var context = new StatementContext(null, null);

            var result = statement.Execute(context);
            Assert.AreEqual(4, result);
        }

        [Test]
        public void FloatStatementTest()
        {
            var statement = StatementParser.Parse("1.3+3.7+0");
            Assert.True(statement is SingleStatement);
            
            var context = new StatementContext(null, null);

            var result = statement.Execute(context);
            Assert.AreEqual(5, result);
        }

        
        [Test]
        public void SimpleBracketsTest()
        {
            var statement = StatementParser.Parse("1+(3*4)");
            Assert.True(statement is SingleStatement);
            
            var context = new StatementContext(null, null);

            var result = statement.Execute(context);
            Assert.AreEqual(13, result);
        }
        
        [Test]
        public void SpacesInExpressionTest()
        {
            var statement = StatementParser.Parse(" 1  +   (  3   *     4  )   ");
            Assert.True(statement is SingleStatement);
            
            var context = new StatementContext(null, null);

            var result = statement.Execute(context);
            Assert.AreEqual(13, result);
        }
        
        [Test]
        public void Condition1Test()
        {
            var statement = StatementParser.Parse("if (3>5) { true; } else { false; }");
            Assert.True(statement is IfStatement);
            
            var context = new StatementContext(null, null);

            var result = statement.Execute(context);
            Assert.AreEqual(false, result);
        }
        
        [Test]
        public void Condition2Test()
        {
            var statement = StatementParser.Parse("if (3<5) { true; } else { false; }");
            Assert.True(statement is IfStatement);
            
            var context = new StatementContext(null, null);

            var result = statement.Execute(context);
            Assert.AreEqual(true, result);
        }
        
        [Test]
        public void CombinedCondition1Test()
        {
            var statement = StatementParser.Parse("if ((3>5) | (true)) { true; } else { false; }");
            Assert.True(statement is IfStatement);
            
            var context = new StatementContext(null, null);

            var result = statement.Execute(context);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void CombinedCondition2Test()
        {
            var statement = StatementParser.Parse("if ((3>5) & (true)) { true; } else { false; }");
            Assert.True(statement is IfStatement);
            
            var context = new StatementContext(null, null);

            var result = statement.Execute(context);
            Assert.AreEqual(false, result);
        }

        [Test]
        public void StringTest()
        {
            var statement = StatementParser.Parse("if ((3>5) & (true)) { 'asdf'; } else { 'qwer'+'ty'; }");
            Assert.True(statement is IfStatement);
            
            var context = new StatementContext(null, null);

            var result = statement.Execute(context);
            Assert.AreEqual("qwerty", result);
        }

        [Test]
        public void DateTest()
        {
            var statement = StatementParser.Parse("%2020.11.28%");
            Assert.True(statement is SingleStatement);
            
            var context = new StatementContext(null, null);

            var result = statement.Execute(context);
            Assert.AreEqual(new DateTime(2020, 11, 28), result);
        }

        [Test]
        public void UnaryTest1()
        {
            var statement = StatementParser.Parse("-11");
            Assert.True(statement is SingleStatement);
            
            var context = new StatementContext(null, null);

            var result = statement.Execute(context);
            Assert.AreEqual(-11, result);
        }

        [Test]
        public void UnaryTest2()
        {
            var statement = StatementParser.Parse("-11.44");
            Assert.True(statement is SingleStatement);
            
            var context = new StatementContext(null, null);

            var result = statement.Execute(context);
            Assert.AreEqual(-11.44, result);
        }
        
        [Test]
        public void UnaryTest3()
        {
            var statement = StatementParser.Parse("!false");
            Assert.True(statement is SingleStatement);
            
            var context = new StatementContext(null, null);

            var result = statement.Execute(context);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void NullTest()
        {
            var statement = StatementParser.Parse("null");
            Assert.True(statement is SingleStatement);
            
            var context = new StatementContext(null, null);

            var result = statement.Execute(context);
            Assert.AreEqual(null, result);
        }

        
        [Test]
        public void LetTest()
        {
            var statement = StatementParser.Parse("{ let a = 'A'; let b = 'B'; a+b;}");
            Assert.True(statement is MultipleStatements);
            
            var context = new StatementContext(null, null);

            var result = statement.Execute(context);
            Assert.AreEqual("AB", result);
        }

        [Test]
        public void PropertyParseTest()
        {
            var statement = StatementParser.Parse("a.b[3].c[4].d");
            Assert.True(statement is SingleStatement);
        }

        [Test]
        public void CustomValueTest1()
        {
            var statement = StatementParser.Parse("this.A+this.B");
            Assert.True(statement is SingleStatement);
            
            var context = new StatementContext(new Compound(1, 5), null);

            var result = statement.Execute(context);
            Assert.AreEqual(6, ((CustomValue)result).Inner);
        }
       
        [Test]
        public void CustomValueTest2()
        {
            var statement = StatementParser.Parse("this.A-this.B");
            Assert.True(statement is SingleStatement);
            
            var context = new StatementContext(new Compound(6, 2), null);

            var result = statement.Execute(context);
            Assert.AreEqual(4, ((CustomValue)result).Inner);
        }
        
        [Test]
        public void CustomValueTest3()
        {
            var statement = StatementParser.Parse("this.A*this.B");
            Assert.True(statement is SingleStatement);
            
            var context = new StatementContext(new Compound(6, 2), null);

            var result = statement.Execute(context);
            Assert.AreEqual(12, ((CustomValue)result).Inner);
        }
        
        [Test]
        public void CustomValueTest4()
        {
            var statement = StatementParser.Parse("this.A/this.B");
            Assert.True(statement is SingleStatement);
            
            var context = new StatementContext(new Compound(6, 2), null);

            var result = statement.Execute(context);
            Assert.AreEqual(3, ((CustomValue)result).Inner);
        }
        
        [Test]
        public void EqualityTest1()
        {
            var statement = StatementParser.Parse("this.A = this.B");
            Assert.True(statement is SingleStatement);
            
            var context = new StatementContext(new Compound(6, 2), null);

            var result = statement.Execute(context);
            Assert.AreEqual(false, result);
        }
        
        [Test]
        public void EqualityTest2()
        {
            var statement = StatementParser.Parse("this.A = this.B");
            Assert.True(statement is SingleStatement);
            
            var context = new StatementContext(new Compound(6, 6), null);

            var result = statement.Execute(context);
            Assert.AreEqual(true, result);
        }
    }

    class Compound
    {
        public readonly CustomValue A;
        public readonly CustomValue B;

        public Compound(Int32 a, Int32 b)
        {
            A = new CustomValue(a);
            B = new CustomValue(b);
        }
    }
    
    class CustomValue : IComparable<CustomValue>, IComparable
    {
        private readonly Int32 _inner;

        public Int32 Inner => _inner;

        public CustomValue(Int32 inner)
        {
            _inner = inner;
        }
        
        public static CustomValue operator +(CustomValue a, CustomValue b) => new CustomValue(a._inner + b._inner);
        public static CustomValue operator -(CustomValue a, CustomValue b) => new CustomValue(a._inner - b._inner);
        public static CustomValue operator *(CustomValue a, CustomValue b) => new CustomValue(a._inner * b._inner);
        public static CustomValue operator /(CustomValue a, CustomValue b) => new CustomValue(a._inner / b._inner);

        public int CompareTo(CustomValue other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return _inner.CompareTo(other._inner);
        }

        public int CompareTo(Object obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            return obj is CustomValue other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(CustomValue)}");
        }

        public static bool operator <(CustomValue left, CustomValue right)
        {
            return Comparer<CustomValue>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(CustomValue left, CustomValue right)
        {
            return Comparer<CustomValue>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(CustomValue left, CustomValue right)
        {
            return Comparer<CustomValue>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(CustomValue left, CustomValue right)
        {
            return Comparer<CustomValue>.Default.Compare(left, right) >= 0;
        }
    }
}