﻿using OmniXaml;

namespace XamlLoadTest
{
    public class Reference : IMarkupExtension
    {
        public object GetValue(ExtensionValueContext context)
        {
            return Target?.Value;
        }

        public ReferenceTarget Target { get; set; }
    }
}