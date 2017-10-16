using System;
using System.Collections.Generic;

namespace Tavis.UriTemplates
{
    public class UriTemplateTable
    {
        private readonly Dictionary<string, UriTemplate> _templates;

        public UriTemplateTable()
        {
            _templates = new Dictionary<string, UriTemplate>();
        }

        public UriTemplate this[string key]
        {
            get
            {
                UriTemplate value;
                if (_templates.TryGetValue(key, out value))
                {
                    return value;
                }

                return null;
            }
        }

        public void Add(string key, UriTemplate template)
        {
            _templates.Add(key, template);
        }

        public TemplateMatch Match(Uri url)
        {
            foreach (var template in _templates)
            {
                var parameters = template.Value.GetParameters(url);
                if (parameters != null)
                {
                    return new TemplateMatch {Key = template.Key, Parameters = parameters, Template = template.Value};
                }
            }
            return null;
        }
    }

    public class TemplateMatch
    {
        public string Key { get; set; }
        public UriTemplate Template { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
    }
}