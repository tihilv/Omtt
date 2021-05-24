# Omtt - One More Template Transformer
Omtt is a .NET-running text template engine. It transforms structured templates to an output stream using input data given. Omtt runs its own template markup syntax and supports simplified expression computations.

## Feature List:
- Structured extendable markup.
- Conditions, loops and grouping operations in templates.
- Hierarchical source data support.
- Built-in simple expression evaluation.
- Custom functions support.
- Output text formatting including culture info.
- Source data scheme generation from templates (to use with SQL or GraphQL queries).

## Reference Guide
### Hello World

By default, any text is valid Omtt template and it's simply copied from input to the output without any change:

```c#
var generator = TemplateTransformer.Create("Hello, World!");
var result = await generator.GenerateTextAsync(null);
Assert.AreEqual("Hello, World!", result);
```

Such example is not very useful as it's not parametrized, sp the easiest practical example of Omtt usage might be the following:
```c#
var generator = TemplateTransformer.Create("Hello, {{this}}!");
var result = await generator.GenerateTextAsync("World");
Assert.AreEqual("Hello, World!", result);
```

The first line creates an instance of Omtt which is configured to transform the template `"Hello, {{this}}!"`. The keyword `this` represents the current data object that is passed to Omtt in the second line (in this case it's a `"World"` string).

Data objects might be single literals, classes or collections. In the following example a class with two properties is used:
```c#
var generator = TemplateTransformer.Create("Hello, {{this.A}} and {{this.B}}!");
var result = await generator.GenerateTextAsync(new {A = "Alice", B = "Bob"});
Assert.AreEqual("Hello, Alice and Bob!", result);
```

The Omtt can process not only plain data objects, but nested objects as well:
```c#
var generator = TemplateTransformer.Create("Hello, {{this.A.Name}} and {{this.B.Name}}!");
var result = await generator.GenerateTextAsync(new {A = new {Id = 1, Name = "Alice"}, B = new {Id = 2, Name = "Bob"}});
Assert.AreEqual("Hello, Alice and Bob!", result);
```

### Formatting
A various formatting might be applied to the output:
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
As we see, the resulting string is not formatted very nice as `and` string is added on each iteration, including the last one. This can be fixed using `if` markup:
```c#
var generator = TemplateTransformer.Create(
    "Hello, <#<forEach source=\"this\">{{this.Name}}<#<if clause=\"!$last\"> and #>#>!"
);
var result = await generator.GenerateTextAsync(new[] {new {Name = "Alice"}, new {Name = "Bob"}});
Assert.AreEqual("Hello, Alice and Bob!", result);
```

In this example the `and` string is put inside a conditional markup `if` that outputs the inner template content if the clause is true. The `clause="!$last"` is true when special loop's variable `$last` is false. So `and` substring is added for each loop iteration except the latest one.

### Computations

Omtt supports simple arithmetical operations that might be useful for reducing the amount of data for source objects. The following example demonstrate the creation of multiplication table that includes computations, inner loops and text alignment formatting:
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

In order to perform the inner loop `<#<forEach source=\"parent\">` on the same data, `parent` keyword is used. Data scope stack in this case on first iteration:

| Variable        | Value   |
| --------------- |:------ :|
| `this`          | 1       |
| `parent`        | [1, 5]  |

The content of the inner loop should access to the current number via `this` keyword, and the value of the parent's cycle via `parent` keyword. Data scope stack in the case for the first iteration of the inner loop:

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
When transforming inner template, the operation can preserve the existing current object (like `write`, `fragment`, `code` operations) or to change it. In this case current objects form a stack, each previous element of it can be accessed via `parent` keywords if needed.  

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

Each field except `expression` are optional and might be omitted. For instance, this syntax says that `SomeProperty` property of the current object should be aligned in 6 symbols to the right:

`{{this.SomeProperty|||6}}`

Note: The operation ignores inner template part.

### if

The operation processes inner part only if the expression in the `clause` is true:

`<#<if clause="expression">conditional inner part #>`

The operation preserves the current object.

### forEach

The operation treats `source` expression as `IEnumerable` and generates inner template for each element setting it as current data object.

`<#<forEach source="expression">per-element inner part #>`

Also, during the execution operation provides the following service variables:
- `$first` - true when the first object is processed.
- `$last` - true when the last object is processed.

These variables might be used to provide some conditional formatting, for instance, separators between items. 

Note: as data access is organised using an enumerator, and streaming is used for output generation, it should be memory-safe to use forEach operation when transforming a big amount of data.  

### group

The operation, treating `source` expression as `IEnumerable`, groups its content by the `key` expression and then repeats inner template for every group.

`<#<group source="expression" key="key expression">inner part using this.Key and this.Values>#>`

The operation replaces the current data object for the inner part as
```c#
internal sealed class KeyValueList
{
    internal Object? Key { get; }
    internal List<Object?> Values { get; }
}
```

Note: grouping suppose to have all the data in-memory before further processing, so avoid grouping of a large data sets.

### fragment

A service operation that might be used together with `write` operation. It specifies the format of the output allowing the writing operation to provide proper symbol escaping of an inner part.

`<#<fragment type="expression">inner part#>`

For the current moment `xml` and `html` fragment types are supported. Further fragment types can be added on demand or developed manually at the client side.

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

If `html` fragment is used, the operation wraps the resulting image to an embedded `img`. Otherwise it provides a base64 representation of the image.

## Expressions
### Data Types
### Common Operations
#### Unary
#### Binary

## Scheme Generation

## Extensions
### Custom Markup
### Custom Functions
### Custom Write
### Custom Context