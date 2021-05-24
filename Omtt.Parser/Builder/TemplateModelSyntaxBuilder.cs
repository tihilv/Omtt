using System.Collections.Generic;
using Omtt.Api.Exceptions;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;
using Omtt.Parser.Lexical;
using Omtt.Statements;

namespace Omtt.Parser.Builder
{
    internal sealed class TemplateModelSyntaxBuilder
    {
        internal ITemplatePart? Build(List<Lexem> lexems)
        {
            var source = new BuildingSource(lexems);
            return FindContent(source);
        }

        private ITemplatePart? FindContent(BuildingSource source)
        {
            var contents = new List<ITemplatePart>();

            ITemplatePart? current;
            do
            {
                current = GetNextContent(source);

                if (current != null)
                    contents.Add(current);

            } while (current != null && source.Any());

            if (contents.Count == 0)
                return null;

            if (contents.Count == 1)
                return contents[0];
            
            return new CombinedTemplatePart(contents);
        }
        
        private ITemplatePart? GetNextContent(BuildingSource source)
        {
            var lexem = source.GetNext();

            if (lexem.Type == LexemType.Text)
                return new TextTemplatePart(lexem.Content);
            
            if (lexem.Type == LexemType.Symbol && lexem.Content == LexicalLiterals.OpenSymbol)
                return GetOperationContent(source);
            
            if (lexem.Type == LexemType.Symbol && lexem.Content == LexicalLiterals.OpenExpression)
                return GetExpressionContent(source);
            
            if (lexem.Type == LexemType.Symbol && lexem.Content == LexicalLiterals.CloseSymbol)
                return null;

            throw new LexicalException($"Unknown lexem {lexem}");
        }

        private ITemplatePart GetExpressionContent(BuildingSource source)
        {
            var content = source.GetNext();

            if (content.Type != LexemType.Text)
                throw new LexicalException($"Wrong lexem for expression content: {content}");

            var nextLexem = source.GetNext();

            if (nextLexem.Type != LexemType.Symbol || nextLexem.Content != LexicalLiterals.CloseExpression)
                throw new LexicalException($"Wrong lexem for expression close: {content}");

            var parsed = content.Content.Split(LexicalLiterals.PipeSeparator);

            OperationParameter[] parameters;

            if (parsed.Length == 1)
                parameters = new[]
                {
                    new OperationParameter(DefaultTemplateParameterNames.Source, StatementParser.Parse(parsed[0])!)
                };
            else if (parsed.Length == 2)
                parameters = new[]
                {
                    new OperationParameter(DefaultTemplateParameterNames.Source, StatementParser.Parse(parsed[0])!),
                    new OperationParameter(DefaultTemplateParameterNames.Format, StatementParser.Parse($"'{parsed[1]}'")!)
                };
            else if (parsed.Length == 3)
                parameters = new[]
                {
                    new OperationParameter(DefaultTemplateParameterNames.Source, StatementParser.Parse(parsed[0])!),
                    new OperationParameter(DefaultTemplateParameterNames.Format, StatementParser.Parse($"'{parsed[1]}'")!),
                    new OperationParameter(DefaultTemplateParameterNames.Culture, StatementParser.Parse($"'{parsed[2]}'")!)
                };
            else
                parameters = new[]
                {
                    new OperationParameter(DefaultTemplateParameterNames.Source, StatementParser.Parse(parsed[0])!),
                    new OperationParameter(DefaultTemplateParameterNames.Format, StatementParser.Parse($"'{parsed[1]}'")!),
                    new OperationParameter(DefaultTemplateParameterNames.Culture, StatementParser.Parse($"'{parsed[2]}'")!),
                    new OperationParameter(DefaultTemplateParameterNames.Align, StatementParser.Parse($"'{parsed[3]}'")!)
                };

            return new OperationTemplatePart(CommonOperations.WriteOperation, null, parameters);
        }

        private ITemplatePart GetOperationContent(BuildingSource source)
        {
            var nameLexem = source.GetNext();
            
            if (nameLexem.Type != LexemType.Text)
                throw new LexicalException($"Wrong lexem for operation name content: {nameLexem}");

            var operationName = nameLexem.Content;

            List<OperationParameter> parameters = new List<OperationParameter>();
            
            while (true)
            {
                var nextLexem = source.GetNext();

                if (nextLexem.Type == LexemType.Symbol && nextLexem.Content == LexicalLiterals.CloseTagSymbol)
                    break;
                
                if (nextLexem.Type != LexemType.Text)
                    throw new LexicalException($"Wrong lexem for parameter name: {nextLexem}");

                var parameterName = nextLexem.Content;
                
                nextLexem = source.GetNext();
                if (nextLexem.Type != LexemType.Symbol && nextLexem.Content != LexicalLiterals.AssignSymbol)
                    throw new LexicalException($"Wrong lexem for equals symbol: {nextLexem}");

                nextLexem = source.GetNext();
                if (nextLexem.Type != LexemType.Symbol && nextLexem.Content != LexicalLiterals.QuoteSymbol)
                    throw new LexicalException($"Wrong lexem for quote symbol: {nextLexem}");

                nextLexem = source.GetNext();
                if (nextLexem.Type != LexemType.Text)
                    throw new LexicalException($"Wrong lexem for parameter value symbol: {nextLexem}");

                var parameterValue = nextLexem.Content;
                
                nextLexem = source.GetNext();
                if (nextLexem.Type != LexemType.Symbol && nextLexem.Content != LexicalLiterals.QuoteSymbol)
                    throw new LexicalException($"Wrong lexem for quote symbol: {nextLexem}");
                
                parameters.Add(new OperationParameter(parameterName, StatementParser.Parse(parameterValue)!));
            }

            var innerContent = FindContent(source);
            
            return new OperationTemplatePart(operationName, innerContent, parameters);
        }
    }
}