using System.Collections.Generic;
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
                xmlDocReader = new XmlDocReader(XDocument.Load(filePath));
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
            sb.Append("M:")
                .Append(info.DeclaringType.FullName)
                .Append(".")
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

                sb.Append(param.ParameterType.FullName);
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
    }
}
