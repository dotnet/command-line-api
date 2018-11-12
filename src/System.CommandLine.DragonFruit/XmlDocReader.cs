// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.DragonFruit
{
    public class XmlDocReader
    {
        private IEnumerable<XElement> _members { get; }

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
                        commandHelpMetadata.ParameterDescriptions.Add(element.Attribute("name")?.Value, element.Value?.Trim());
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
            }
            else if (type.IsGenericType)
            {
                var typeDefName = type.GetGenericTypeDefinition().FullName;

                sb.Append(typeDefName.Substring(0, typeDefName.IndexOf("`")));

                sb.Append("{");

                foreach (var genericArgument in type.GetGenericArguments())
                {
                    AppendTypeName(sb, genericArgument);
                }

                sb.Append("}");
            }
            else
            {
                sb.Append(type.FullName);
            }
        }
    }
}
