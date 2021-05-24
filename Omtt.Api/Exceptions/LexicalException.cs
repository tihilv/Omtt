using System;

namespace Omtt.Api.Exceptions
{
    public sealed class LexicalException: Exception
    {
        public LexicalException(String message): base(message)
        {
        }
    }
}