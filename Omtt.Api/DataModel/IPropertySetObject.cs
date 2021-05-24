using System;

namespace Omtt.Api.DataModel
{
    public interface IPropertySetObject
    {
        Object? this[String key] { get; set; }
    }
}