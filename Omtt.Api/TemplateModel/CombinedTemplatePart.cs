using System.Collections.Generic;
using System.Linq;

namespace Omtt.Api.TemplateModel
{
    public sealed class CombinedTemplatePart : ITemplatePart
    {
        private readonly ITemplatePart[] _children;

        public ITemplatePart[] Children => _children;

        public CombinedTemplatePart(IEnumerable<ITemplatePart> children)
        {
            _children = children.ToArray();
        }
    }
}