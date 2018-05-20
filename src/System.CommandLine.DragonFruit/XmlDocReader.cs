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

        public bool TryGetMethodDescription(MethodInfo info, out MethodDescription methodDescription)
        {
            methodDescription = null;

            var sb = new StringBuilder();
            sb.Append("M:")
                .Append(info.DeclaringType.FullName)
                .Append(".")
                .Append(info.Name)
                .Append("(");

            var first = true;
            foreach (var param in info.GetParameters())
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

            var name = sb.ToString();

            var member = _members.Elements("member")
                .FirstOrDefault(m => string.Equals(m.Attribute("name")?.Value, name));

            if (member == null)
            {
                return false;
            }

            methodDescription = new MethodDescription();

            foreach (var element in member.Elements())
            {
                switch (element.Name.ToString())
                {
                    case "summary":
                        methodDescription.Description = element.Value?.Trim();
                        break;
                    case "param":
                        methodDescription.AddParameter(element.Attribute("name")?.Value, element.Value?.Trim());
                        break;
                }
            }

            return true;
        }
    }

    internal class MethodDescription
    {
        private readonly Dictionary<string, string> _parameters = new Dictionary<string, string>();

        public string Description { get; set; }

        public void AddParameter(string parameterName, string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                description = null;
            }

            _parameters.Add(parameterName, description);
        }

        public bool TryGetParameterDescription(string parameterName, out string description)
            => _parameters.TryGetValue(parameterName, out description);
    }
}
