using System;
using System.Collections.Generic;
using System.Reflection;
using Omtt.Api.Exceptions;

namespace Omtt.Api.DataModel
{
    public class PropertySetObject: IPropertySetObject
    {
        private readonly Dictionary<String, Object?> _properties;

        public PropertySetObject(Object? fromObject = null)
        {
            _properties = new Dictionary<String, Object?>();

            if (fromObject != null)
            {
                var properties = fromObject!.GetType().GetProperties(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public);
                foreach (var property in properties)
                    if (property.CanRead)
                        this[property.Name] = property.GetValue(fromObject);
            }
        }

        public Object? this[String key]
        {
            get
            {
                if (_properties.TryGetValue(key, out var result))
                    return result;
                
                throw new MissingMemberException($"Property '{key}' is not found.");

            }
            set { _properties[key] = value; }
        }
    }
}