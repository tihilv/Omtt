using System;

namespace Omtt.Api.TemplateModel
{
    public sealed class TextTemplatePart : ITemplatePart
    {
        private readonly String _text;

        public String Text => _text;

        public TextTemplatePart(String text)
        {
            _text = text;
        }
    }
}