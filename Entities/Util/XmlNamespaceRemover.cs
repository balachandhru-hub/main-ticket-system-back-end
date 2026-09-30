using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Entities.Util
{
    public static class XmlNamespaceRemover
    {
        public static XElement RemoveAllNamespaces(XElement xml)
        {
            return new XElement(xml.Name.LocalName,
                xml.HasElements
                    ? xml.Elements().Select(RemoveAllNamespaces)
                    : (object)xml.Value
            );
        }
    }
}
