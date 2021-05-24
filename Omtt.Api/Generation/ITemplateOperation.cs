using System;
using System.Threading.Tasks;
using Omtt.Api.TemplateModel;

namespace Omtt.Api.Generation
{
    public interface ITemplateOperation
    {
        String Name { get; }
        Task PerformAsync(OperationTemplatePart part, IGeneratorContext ctx);
        Task PerformAsync(OperationTemplatePart part, ISourceSchemeContext ctx);
    }
}