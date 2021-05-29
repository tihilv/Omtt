# Omtt - One More Template Transformer

![Build](https://github.com/tihilv/Omtt/actions/workflows/dotnet.yml/badge.svg) 
![Nuget](https://img.shields.io/nuget/v/Omtt.Generator)

Omtt is a .NET-running text template engine. It transforms structured templates to an output stream using input data given. Omtt runs its own template markup syntax and supports simplified expression computations.

## Feature List
- Structured extendable markup.
- Conditions, loops, and grouping operations in templates.
- Hierarchical source data support.
- Built-in simple expression evaluation.
- Custom functions support.
- Output text formatting including culture info.
- Source data scheme generation from templates (to use with SQL or GraphQL queries).

## Reference Guide
### Hello World

By default, any text is a valid Omtt template and it's simply copied from input to the output without any change:

```c#
var generator = TemplateTransformer.Create("Hello, World!");
var result = await generator.GenerateTextAsync(null);
Assert.AreEqual("Hello, World!", result);
```

Such example is not very useful as it's not parametrized, so the easiest practical example of Omtt usage might be the following:
```c#
var generator = TemplateTransformer.Create("Hello, {{this}}!");
var result = await generator.GenerateTextAsync("World");
Assert.AreEqual("Hello, World!", result);
```

The first line creates an instance of Omtt which is configured to transform the template `"Hello, {{this}}!"`. The keyword `this` represents the current data object that is passed to Omtt in the second line (in this case it's a `"World"` string).

Data objects might be single literals, classes, or collections. In the following example a class with two properties is used:
```c#
var generator = TemplateTransformer.Create("Hello, {{this.A}} and {{this.B}}!");
var result = await generator.GenerateTextAsync(new {A = "Alice", B = "Bob"});
Assert.AreEqual("Hello, Alice and Bob!", result);
```

The Omtt can process not only plain data objects but nested objects as well:
```c#
var generator = TemplateTransformer.Create("Hello, {{this.A.Name}} and {{this.B.Name}}!");
var result = await generator.GenerateTextAsync(new {A = new {Id = 1, Name = "Alice"}, B = new {Id = 2, Name = "Bob"}});
Assert.AreEqual("Hello, Alice and Bob!", result);
```

### Formatting
Various formatting might be applied to the output:
```c#
var generator = TemplateTransformer.Create("{{this.Id|D3}}, {{this.FamilyName|u}}, {{this.Name}}");
var result = await generator.GenerateTextAsync(new {Id = 7, Name = "James", FamilyName = "Bond"});
Assert.AreEqual("007, BOND, James", result);
```

Culture formatting also can be applied (pay attention to the decimal separator):
```c#
var generator = TemplateTransformer.Create("{{this|F2|en}} {{this|F2|ru}}");
var result = await generator.GenerateTextAsync(Math.PI);
Assert.AreEqual("3.14 3,14", result);
```

### Collections
Collection processing might be done in different ways. One option is to access elements via indices directly:
```c#
var generator = TemplateTransformer.Create("Hello, {{this[0].Name}} and {{this[1].Name}}!");
var result = await generator.GenerateTextAsync(new[] {new {Name = "Alice"}, new {Name = "Bob"}});
Assert.AreEqual("Hello, Alice and Bob!", result);
```

But the better option is to use `forEach` markup:
```c#
var generator = TemplateTransformer.Create("Hello, <#<forEach source=\"this\">{{this.Name}} and #>!");
var result = await generator.GenerateTextAsync(new[] {new {Name = "Alice"}, new {Name = "Bob"}});
Assert.AreEqual("Hello, Alice and Bob and !", result);
```
Here the full syntax of Omtt markup is presented. The operation starts with `<#` tag followed by operation tag `<forEach source="this">` with `source` parameter set to `this`. The rest of the text before closing tag `#>` represents the template for each element in the enumeration: `{{this.Name}} and `.   
Note: `this` inside the loop body is moved to the currently processing element. 

### Conditions
As we see, the resulting string is not formatted very nicely as `and` string is added on each iteration, including the last one. This can be fixed using `if` markup:
```c#
var generator = TemplateTransformer.Create(
    "Hello, <#<forEach source=\"this\">{{this.Name}}<#<if clause=\"!$last\"> and #>#>!"
);
var result = await generator.GenerateTextAsync(new[] {new {Name = "Alice"}, new {Name = "Bob"}});
Assert.AreEqual("Hello, Alice and Bob!", result);
```

In this example, the `and` string is put inside a conditional markup `if` that outputs the inner template content if the clause is true. The `clause="!$last"` is true when special loop's variable `$last` is false. So `and` substring is added for each loop iteration except the latest one.

### Computations

Omtt supports simple arithmetical operations that might be useful for reducing the amount of data for source objects. The following example demonstrates the creation of a multiplication table that includes computations, inner loops, and text alignment formatting:
```c#
var generator = TemplateTransformer.Create(
    "<#<forEach source=\"this\"><#<forEach source=\"parent\">{{this*parent|||3}}#>\r\n#>"
);
var result = await generator.GenerateTextAsync(Enumerable.Range(1, 5));
Assert.AreEqual(
"  1  2  3  4  5\r\n" +
"  2  4  6  8 10\r\n" +
"  3  6  9 12 15\r\n" +
"  4  8 12 16 20\r\n" +
"  5 10 15 20 25", result);
```
The first forEach `<#<forEach source=\"this\">` enumerates throw the source data representing the range of ints from 1 to 5. Data scope stack can be represented at this point in the following way:

| Variable        | Value   |
| --------------- |:-------:|
| `this`          | [1, 5]  |

In order to perform the inner loop `<#<forEach source=\"parent\">` on the same data, `parent` keyword is used. Data scope stack in this case on the first iteration:

| Variable        | Value   |
| --------------- |:-------:|
| `this`          | 1       |
| `parent`        | [1, 5]  |

The content of the inner loop should access the current number via `this` keyword, and the value of the parent's cycle via `parent` keyword. Data scope stack in the case for the first iteration of the inner loop:

| Variable        | Value   |
| --------------- |:-------:|
| `this`          | 1       |
| `parent`        | 1       |
| `parent.parent` | [1, 5]  |

`|||3` suffix means that no particular formatting or culture is applied to the result, but the text should be aligned in 3 symbols to the right. 

### Grouping

Collections might be processed using grouping by a given key.
```c#
var generator = TemplateTransformer.Create(
"<#<group source=\"this\" key=\"this.Name\">{{this.Key}}: <#<forEach source=\"this.Values\">{{this.FamilyName}}<#<if clause=\"!$last\">, #>#>\r\n#>"
);
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
```

## Markup
### Definition

Omtt uses its own markup syntax based on Omtt open (`<#`) and close (`#>`) tags and operation definitions that might be parametrized. The generalized syntax of the markup tag inside the template is the following:

`...<#<operation attr1="expr1" attr2="expr2"... attrN="exprN">...#>...`

Default operations and their parameters will be discussed later in this chapter.  
Expressions will be described in the next chapter.  

`...` represents independent template parts. This allows to express templates using nested operations of any arbitrary depth. 
An operation executes within its own current data object. The root of the data object is passed in the method
```c#
var result = await generator.GenerateTextAsync(rootDataObject);
```
When transforming the inner template, the operation can preserve the existing current object (like `write`, `fragment`, `code` operations) or change it. In this case, current objects form a stack, each previous element of it can be accessed via `parent` keywords if needed.  

Operation names are case-sensitive.

### write
The operation formats the expression to put it into the output stream. The default syntax is the following:

`<#<write source="expression" format="format" culture="culture" align="align">#>`

`expression` represents the expression to transform to a string.

`format` in most cases matches the format parameter of `ToString` method for standard CLR types (see [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/standard/base-types/formatting-types)). 
There are two additions for strings formatting: 
- `"u"` format specifier converts a string to the upper case,   
- `"l"` format specifier converts a string to the lower case.

`culture` provides the culture name for values representation. The name of the culture should correspond to the names of `CultureInfo` available in .NET.

`align` allows to allocate an area of the given size to the text and to align it there to the right (positive value) or to the left (negative value).

As write operation is the most used operation in templates, a short form is introduced:

`{{expression|format|culture|align}}`

Each field except `expression` is optional and might be omitted. For instance, this syntax says that `SomeProperty` property of the current object should be aligned in 6 symbols to the right:

`{{this.SomeProperty|||6}}`

Note: The operation ignores the inner template part.

### if

The operation processes the inner part only if the expression in the `clause` is true:

`<#<if clause="expression">conditional inner part #>`

The operation preserves the current object.

### forEach

The operation treats `source` expression as `IEnumerable` and generates an inner template for each element setting it as a current data object.

`<#<forEach source="expression">per-element inner part #>`

Also, during the execution operation provides the following service variables:
- `$first` - true when the first object is processed.
- `$last` - true when the last object is processed.

These variables might be used to provide some conditional formatting, for instance, separators between items. 

Note: as data access is organized using an enumerator, and streaming is used for output generation, it should be memory-safe to use `forEach` operation when transforming a big amount of data.  

### group

The operation, treating `source` expression as `IEnumerable`, groups its content by the `key` expression and then repeats the inner template for every group.

`<#<group source="expression" key="key expression">inner part using this.Key and this.Values>#>`

The operation replaces the current data object for the inner part as
```c#
internal sealed class KeyValueList
{
    internal Object? Key { get; }
    internal List<Object?> Values { get; }
}
```

Note: grouping suppose to have all the data in memory before further processing, so avoid grouping of large data sets.

### fragment

A service operation that might be used together with `write` operation. It specifies the format of the output allowing the writing operation to provide proper symbol escaping of an inner part.

`<#<fragment type="expression">inner part#>`

For the current moment, `xml` and `html` fragment types are supported. Further fragment types can be added on-demand or developed manually at the client-side.

The operation preserves the current object.

### code

A service operation that executes the expression in its `source` attribute. Might be used for simple data aggregation.

The operation preserves the current object.

### qr

An operation that transforms the content of the inner template to QR code. As it uses 3rd-party control, it's shipped independently. 

To attach the operation to the Omtt, the following code should be called before template transformation:

```c#
generator.WithQr();
```

If `html` fragment is used, the operation wraps the resulting image to an embedded `img`. Otherwise, it provides a base64 representation of the image.

## Expressions
Every attribute of Omtt operation is an expression. An expression might be:
- a literal: `123`, `'abc'`, `true`...
- a property of a current object or parent: `this.SomeProperty`, `parent.parent.SomeArray[3].SomeProperty`.
- an arithmetical expression involving literals or properties: `3*this.SomeProperty`.
- a custom function call having N parameters: `SomeFunction(5, this.SomeProperty)`.
- logical expression: `if (3>5) { true; } else { false; }`
- assignment expression: `let c = 4`.
- earlier declared variables: `c`.

Every expression might end with a semicolon. Multiple operations inside a `{...}` block must end with a semicolon.

Every expression returns a value. For blocks, the last expression treated as the result of the block.

### Data Types

Omtt has dynamic typing. In most cases, the left operand in binary operations defines the resulting type.

When dealing with data objects, operator overloading (`+, -, *, /`) and `IComparable` are supported.
Using literals only a subset of CLR types can be expressed:

|Type| CLR Type | Example | Unary Operations | Binary Operations |
| :----------- | :----------- | :----------- |  :----------- | :----------- |
| Integer | Int64 | 11 | - (minus) | +, -, /, *, <, =, >
| Real | Decimal | 11.234 | - (minus) | +, -, /, *, <, =, >
| String | String | 'str' | | +, <, =, >
| Boolean | Boolean | true, false | ! (negation) | &, l, =
| DateTime | DateTime | %15.09.2020% | |
| Undefined | Object | null | |

Binary operations are processed from left to right, priority can be changed with braces. 

### Variables Scope

A variable exists only within the operation scope containing the corresponding `let` expression. It is also available for and all inner operation scopes.

Assignment of a variable is processed according to the following algorithm: 
1. A variable is being searched by name starting from the current operation scope to the root.
2. If the variable is found, the assignment is performed for the corresponding scope. 
3. If the variable is not found, it is created for the current operation scope. 

This allows synchronizing  the variable value within inner scopes.

Variables are case-sensitive. 

## Scheme Generation

It might be useful to receive the appropriate data structure that is suitable for a given template. For instance, that might help to reduce the number of columns in an SQL query or to build a GraphQL query including the minimum necessary amount of data.

Omtt is able to reconstruct the data scheme by template:
```c#
var generator = TemplateTransformer.Create(
    "<#<forEach source=\"this.ClassesB\"> {{parent.Str}} {{this.MyInt1 + this.MyInt2}}" +
    "<#<forEach source=\"this.Decimals\"> {{parent.parent.Str}} {{this}}#>#>");
var dataStructure = await generator.GetSourceSchemeAsync();
Assert.AreEqual(
    " { ClassesB[] { Decimals[], MyInt1, MyInt2 }, Str }", dataStructure.ToString());
```

Source scheme represents the hierarchical structure of classes and their properties that participate in template transformation. As templates have no information about data types, the source data scheme contains no type details either. The only assumption that can be made is `IsArray` attribute for every property. It sets to `true` if the property participates in `forEach` or `group` operation. 

## Extensions
Omtt is designed with extendability in mind. Template transformation might be performed using:
- custom markup operations, 
- custom expression functions, 
- overloaded write operation, 
- user-defined operation context.

### Custom Markup Operations

Operations are the main building blocks of Omtt. They define the workflow of template processing and perform writing to the output stream. Omtt has a limited number of default operations but the list might be easily extended using custom operations.

Assume the task is to implement an Omtt feature that converts the inner template part to Base64 format. To make the task slightly more complicated assume that sometimes the conversion should be performed using the terminating `=` symbol, and sometimes it should be omitted.

The task might be solved using a custom markup operation that implements `ITemplateOperation` interface.

The interface has three main parts: 
- operation name (operations are case-sensitive),
- main conversion method, 
- method for extracting the data scheme of the operation.

A sample implementation:
```c#
private sealed class Base64Operation : ITemplateOperation
{
    private const String TrimEqualsName = "trim";
           
    // Name of the operation to call in the template.
    public String Name => "base64";
            
    // Main transformation function.
    public async Task PerformAsync(OperationTemplatePart part, IGeneratorContext ctx)
    {
        // Check if inner part exists
        if (part.InnerPart == null)
            throw new ArgumentNullException("Operation content is null.");

        Boolean trim = false;
        // Get the format parameter taking into account that it might be not defined.
        if (part.Parameters.TryGetValue(TrimEqualsName, out var trimExpr) && trimExpr != null)
            trim = (Boolean)ctx.EvaluateStatement(trimExpr)!;
                
        using var memoryStream = new MemoryStream();
        using (ctx.OverloadStream(memoryStream)) // Overload the output to the temporary stream...
        {
            await ctx.ExecuteAsync(part.InnerPart!, ctx.SourceData); // ... and process the inner part as a template
        } // At this point the original output stream is restored

        // Read the processed inner part from the temporary stream and convert it to Base64
        memoryStream.Position = 0;
        var result = Convert.ToBase64String(memoryStream.ToArray());

        if (trim) // Apply termination trimming if needed
            result = result.TrimEnd('=');
                    
        // Writes the result to the output stream
        await ctx.WriteAsync(result);
    }

    // Function to implement the proper source scheme generation
    public Task PerformAsync(OperationTemplatePart part, ISourceSchemeContext ctx)
    {
        // Process parameter
        if (part.Parameters.TryGetValue(TrimEqualsName, out var formatExpr) && formatExpr != null)
            ctx.EvaluateStatement(formatExpr);

        // Process inner part
        return ctx.ExecuteAsync(part.InnerPart!, ctx.SourceData);
    }
}
```

To use new operation it should be registered:
```c#
var generator = TemplateTransformer.Create("Hello, <#<base64>{{this.Name}}#>!");
generator.AddOperation(new Base64Operation());
var result = await generator.GenerateTextAsync(new {Name = "World"});
Assert.AreEqual($"Hello, {Convert.ToBase64String(Encoding.UTF8.GetBytes("World"))}!", result);
```

The trim parameter is passed as any other expression:
```c#
var generator = TemplateTransformer.Create("Hello, <#<base64 trim=\"this.Trim\">{{this.Name}}#>!");
generator.AddOperation(new Base64Operation());
var result = await generator.GenerateTextAsync(new {Name = "World", Trim = true});
Assert.AreEqual($"Hello, {Convert.ToBase64String(Encoding.UTF8.GetBytes("World")).TrimEnd('=')}!", result);
```

Source scheme generation also uses source data object properties:
```c#
var generator = TemplateTransformer.Create("Hello, <#<base64 trim=\"this.SomeTrim\">{{this.Name}}#>!");
generator.AddOperation(new Base64Operation());
var result = await generator.GetSourceSchemeAsync();
Assert.AreEqual(" { Name, SomeTrim }", result.ToString());
```

### Custom Expression Functions

Custom functions extend the functionality of Omtt on the expression level.

Assume the task is to implement a function that calculates i'th Fibonacci number. 

The task might be solved using a custom function that implements `IStatementFunction` interface.

The interface has three main parts:
- function name (functions are case-sensitive),
- number of arguments (functions are distinguished not only by names but also by argument count - a kind of overloading feature),  
- main calculation method.

A sample implementation:
```c#
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
```

To use the new function it should be registered:
```c#
var generator = TemplateTransformer.Create("{{this.N}}'th Fibonacci number is {{fib(this.N)}}");
generator.AddFunction(new FibonacciFunction());
var result = await generator.GenerateTextAsync(new {N = 6});
Assert.AreEqual("6'th Fibonacci number is 8", result);
```

As it's a function, it can be used for expression calculations:
```c#
var generator = TemplateTransformer.Create("{{this.N+1}}'th Fibonacci number is {{fib(this.N-1)+fib(this.N)}}");
generator.AddFunction(new FibonacciFunction());
var result = await generator.GenerateTextAsync(new {N = 6});
Assert.AreEqual("7'th Fibonacci number is 13", result);
```

### Custom Write

The `write` is the most important markup operation of Omtt. This means, in a different situation, it might be useful to adjust the behavior of that operation fully controlling the output. 

For instance, these features might be requested:
- string representation of custom data types,
- data filtration, 
- symbol escaping for new fragment types, 
- new formatting options...

These tasks can be solved using a descendant of `WriteOperation` class.

There are two methods to override:
```c#
protected virtual String? FormatResult(Object? value, String? format, CultureInfo culture, IGeneratorContext generatorContext);

protected virtual String FormatFragment(String valueStr, String fragmentType);
```
To register the new implementation of `write` markup operation `AddOperation` method can be used.

### Custom Context

For some rare cases, it might be necessary to share some additional data between markup operations and expression functions. It might be implemented by using custom contexts:

- `StatementContext`  
A factory method for statement context creation is passed as a parameter of `GenerateAsync` method. As input, current data object and parent statement context (if any) are passed.
  

- `SourceSchemeContext`  
Root context should be passed as a parameter of `GetSourceSchemeAsync` method.  
Children contexts should be created using overridden function:  
`protected abstract T CreateChildContext(Object? data);`
