using System.Xml.Linq;

namespace Umbraco.Db.Extensions
{
    public static partial class XmlExtension
    {
        public static bool AttributeEquals(this XElement element, string name, string value)
        {
            var a = element.Attribute(name);
            return a != null && value.Equals(a.Value);
        }

        public static bool HasAttribute(this XElement element, string name)
        {
            return element.Attribute(name) != null;
        }
    }
}
