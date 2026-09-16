using System;

namespace Omtt.Api.DataModel
{
    public interface IOptionalPropertySetObject: IPropertySetObject
    {
        bool TryGetValue(String key, out Object? value);
    }
}