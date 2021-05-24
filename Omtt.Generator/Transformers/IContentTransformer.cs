using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;

namespace Omtt.Generator.Transformers
{
    public interface IContentTransformer
    {
        Task TransformAsync(ITemplatePart part, IProcessingContext context);
    }
}