using Omtt.Generator.Operations;

namespace Omtt.Generator
{
    public static class GeneratorExtensions
    {
        public static void WithQr(this TemplateTransformer generator)
        {
            generator.AddOperation(new QrOperation());
        }
    }
}