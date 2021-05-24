using System;
using System.IO;
using System.Threading.Tasks;

namespace Omtt.Generator.Extensions
{
    public static class TemplateTransformerExtensions
    {
        public static async Task<String> GenerateTextAsync(this TemplateTransformer generator, Object? data)
        {
            using (var outputStream = new MemoryStream())
            {
                await generator.GenerateAsync(data, outputStream);
                outputStream.Position = 0;

                using (var streamReader = new StreamReader(outputStream))
                {
                    return streamReader.ReadToEnd().TrimEnd('\r', '\n');
                }
            }
        }
    }
}