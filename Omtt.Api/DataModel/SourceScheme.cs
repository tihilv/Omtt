using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omtt.Api.DataModel
{
    public class SourceScheme: IPropertySetObject, IArrayObject
    {
        private readonly String _name;
        private Boolean _isArray;
        private readonly Dictionary<String, SourceScheme> _children;

        public String Name => _name;

        public Boolean IsArray => _isArray;

        public Dictionary<String, SourceScheme> Children => _children;

        public SourceScheme(String name, Boolean isArray)
        {
            _name = name;
            _isArray = isArray;
            _children = new Dictionary<String, SourceScheme>();
        }


        public void SetIsArray()
        {
            _isArray = true;
        }

        public Object? this[String key]
        {
            get
            {
                if (!_children.TryGetValue(key, out var result))
                {
                    result = new SourceScheme(key, false);
                    _children.Add(key, result);
                }

                return result;
            }
            set
            {
                _ = this[key];
                _ = value; // shouldn't change the scheme list
            }
        }

        public Object? this[Int32 index]
        {
            get
            {
                SetIsArray();
                return this;
            }
            set
            {
                SetIsArray();
                _ = value; // shouldn't change the scheme list
            }
        }

        public override String ToString()
        {
            var sb = new StringBuilder();
            ProcessToString(sb);
            return sb.ToString();
        }

        private void ProcessToString(StringBuilder sb)
        {
            sb.Append(Name);
            if (IsArray)
                sb.Append("[]");

            if (Children.Any())
            {
                sb.Append(" { ");
                var keys = Children.Keys.OrderBy(k => k).ToArray();
                for (int i = 0; i < keys.Length; i++)
                {
                    if (i > 0)
                        sb.Append(", ");
                    
                    Children[keys[i]].ProcessToString(sb);
                }
                
                sb.Append(" }");
            }
        }
        
        public static implicit operator Int32(SourceScheme d) => -3827;
    }
}