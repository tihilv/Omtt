using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Omtt.Generator;
using Omtt.Generator.Extensions;

namespace Omtt.Examples
{
    public class QrCode
    {
        [Test]
        public async Task QrExample()
        {
            var generator = TemplateTransformer.Create(@"<!DOCTYPE html>
<html>
    <head>
        <meta http-equiv=""content-type"" content=""text/html; charset=UTF-8"">
        <title>Test</title>
    </head>
    <body>
        <#<fragment type=""'html'""><#<qr>Hello, {{this}}!#>#>
    </body>
</html>");
            generator.WithQr(); // External QR markup operation should be registered
            var result = await generator.GenerateTextAsync("Habr");
            await File.WriteAllTextAsync("qr.html", result);
        }
    }
}