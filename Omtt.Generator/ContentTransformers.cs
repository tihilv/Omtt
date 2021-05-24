using System;
using System.Collections.Generic;
using Omtt.Api.TemplateModel;
using Omtt.Generator.Transformers;

namespace Omtt.Generator
{
    internal static class ContentTransformers
    {
        private static readonly Dictionary<Type, IContentTransformer> Transformers;

        static ContentTransformers()
        {
            Transformers = new Dictionary<Type, IContentTransformer>();
            Transformers.Add(typeof(TextTemplatePart), new TextContentTransformer());
            Transformers.Add(typeof(CombinedTemplatePart), new CombinedContentTransformer());
            Transformers.Add(typeof(OperationTemplatePart), new OperationContentTransformer());
        }

        internal static IContentTransformer Get(ITemplatePart part)
        {
            return Transformers[part.GetType()];
        }
    }
}