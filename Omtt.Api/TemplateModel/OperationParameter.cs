using System;
using Omtt.Api.StatementModel;

namespace Omtt.Api.TemplateModel
{
    public sealed class OperationParameter
    {
        private readonly String _name;
        private readonly IStatement _value;

        public String Name => _name;

        public IStatement Value => _value;

        public OperationParameter(String name, IStatement value)
        {
            _name = name;
            _value = value;
        }
    }
}