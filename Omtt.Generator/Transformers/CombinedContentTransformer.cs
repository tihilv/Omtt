using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Transformers
{
    internal sealed class CombinedContentTransformer : IContentTransformer
    {
        public async Task TransformAsync(ITemplatePart part, IProcessingContext context)
        {
            var combinedPart = (CombinedTemplatePart) part;

            foreach (var child in combinedPart.Children)
                await context.ExecuteAsync(child);
        }
    }
}