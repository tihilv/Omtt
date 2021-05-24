using System;
using System.Collections.Generic;
using System.Linq;
using Omtt.Api.Generation;
using Omtt.Api.StatementModel;

namespace Omtt.Api.TemplateModel
{
    public sealed class OperationTemplatePart : ITemplatePart
    {
        private readonly String _operationName;
        private readonly ITemplatePart? _innerPart;
        private readonly Dictionary<String, IStatement> _parameters;

        public String OperationName => _operationName;
        public ITemplateOperation? CachedTemplateOperation;

        public ITemplatePart? InnerPart => _innerPart;

        public Dictionary<String, IStatement> Parameters => _parameters;

        public OperationTemplatePart(String operationName, ITemplatePart? innerPart, IEnumerable<OperationParameter> parameters)
        {
            _operationName = operationName;
            _innerPart = innerPart;
            _parameters = parameters.ToDictionary(p => p.Name, p => p.Value);
        }
    }
}