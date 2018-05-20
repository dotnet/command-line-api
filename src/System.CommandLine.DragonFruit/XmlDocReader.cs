// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace System.CommandLine.DragonFruit
{
    internal class XmlDocReader
    {
        private readonly IEnumerable<XElement> _members;

        private XmlDocReader(XDocument document)
        {
            _members = document.Descendants("members");
        }

        public static bool TryLoad(string filePath, out XmlDocReader xmlDocReader)
        {
            try
            {
                return TryLoad(File.OpenText(filePath), out xmlDocReader);
            }
            catch
            {
                xmlDocReader = null;
                return false;
            }
        }

        public static bool TryLoad(TextReader reader, out XmlDocReader xmlDocReader)
        {
            try
            {
                xmlDocReader = new XmlDocReader(XDocument.Load(reader));
                return true;
            }
            catch
            {
                xmlDocReader = null;
                return false;
            }
        }

        public bool TryGetMethodDescription(MethodInfo info, out CommandHelpMetadata commandHelpMetadata)
        {
            commandHelpMetadata = null;

            var sb = new StringBuilder();
            sb.Append("M:");
            AppendTypeName(sb, info.DeclaringType);
            sb.Append(".")
                .Append(info.Name)
                .Append("(");

            bool first = true;
            foreach (ParameterInfo param in info.GetParameters())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(",");
                }

                AppendTypeName(sb, param.ParameterType);
            }

            sb.Append(")");

            string name = sb.ToString();

            XElement member = _members.Elements("member")
                .FirstOrDefault(m => string.Equals(m.Attribute("name")?.Value, name));

            if (member == null)
            {
                return false;
            }

            commandHelpMetadata = new CommandHelpMetadata();

            foreach (XElement element in member.Elements())
            {
                switch (element.Name.ToString())
                {
                    case "summary":
                        commandHelpMetadata.Description = element.Value?.Trim();
                        break;
                    case "param":
                        commandHelpMetadata.AddParameter(element.Attribute("name")?.Value, element.Value?.Trim());
                        break;
                }
            }

            return true;
        }

        private static void AppendTypeName(StringBuilder sb, Type type)
        {
            if (type.IsNested)
            {
                AppendTypeName(sb, type.DeclaringType);
                sb.Append(".").Append(type.Name);
                return;
            }

            sb.Append(type.FullName);
        }
    }
}
