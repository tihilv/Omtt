using System;
using System.Threading.Tasks;
using Omtt.Api.TemplateModel;

namespace Omtt.Api.Generation
{
    /// <summary>
    /// Interface for custom markup operation.
    /// </summary>
    public interface ITemplateOperation
    {
        /// <summary>
        /// Name of the operation. Case-sensitive.
        /// </summary>
        String Name { get; }
        
        /// <summary>
        /// Processes the requested markup operation.
        /// </summary>
        /// <param name="part">Markup operation input data</param>
        /// <param name="ctx">Current generator context</param>
        Task PerformAsync(OperationTemplatePart part, IGeneratorContext ctx);
        
        /// <summary>
        /// Provides the data structure info that is used by the operation to generate source scheme. 
        /// </summary>
        /// <param name="part">Markup operation input data</param>
        /// <param name="ctx">Current source scheme context</param>
        /// <returns></returns>
        Task PerformAsync(OperationTemplatePart part, ISourceSchemeContext ctx);
    }
}