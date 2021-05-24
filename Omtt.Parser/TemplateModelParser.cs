using System;
using System.IO;
using System.Threading.Tasks;
using Omtt.Api.TemplateModel;
using Omtt.Parser.Builder;
using Omtt.Parser.Lexical;

namespace Omtt.Parser
{
    public sealed class TemplateModelParser
    {
        private readonly TemplateModelLexicalParser _lexicalParser;
        private readonly TemplateModelSyntaxBuilder _syntaxBuilder;

        public TemplateModelParser()
        {
            _lexicalParser = new TemplateModelLexicalParser();
            _syntaxBuilder = new TemplateModelSyntaxBuilder();
        }

        public ITemplatePart? ParseTemplateModel(String content)
        {
            var lexems = _lexicalParser.Parse(content);
            return _syntaxBuilder.Build(lexems);
        }
        
        public async Task<ITemplatePart?> ParseTemplateModelAsync(Stream content)
        {
            var lexems = await _lexicalParser.ParseAsync(content);
            return _syntaxBuilder.Build(lexems);
        }

    }
}