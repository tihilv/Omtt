using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Transformers
{
    internal sealed class OperationContentTransformer : IContentTransformer
    {
        public Task TransformAsync(ITemplatePart part, IProcessingContext context)
        {
            var operationPart = (OperationTemplatePart) part;

            return context.ProcessOperationAsync(operationPart);
        }
    }
}