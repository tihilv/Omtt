using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Transformers
{
    internal sealed class TextContentTransformer : IContentTransformer
    {
        public Task TransformAsync(ITemplatePart part, IProcessingContext context)
        {
            var textPart = (TextTemplatePart) part;
            
            return context.WriteAsync(textPart.Text);
        }
    }
}