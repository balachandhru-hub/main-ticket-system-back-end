using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Dto
{
    [XmlRoot(ElementName = "Envelope")]
    public class AribaSoapEnvelope
    {
        [XmlElement(ElementName = "Body")]
        public AribaSoapBody Body { get; set; }
    }

    public class AribaSoapBody
    {
        [XmlElement(ElementName = "ExternalReceiptImportReply")]
        public ExternalReceiptImportReply Reply { get; set; }
    }

    public class ExternalReceiptImportReply
    {
        [XmlArray("ExternalReceiptOutputBean_Item")]
        [XmlArrayItem("item")]
        public List<AribaReceiptItem> Items { get; set; }
    }

    public class AribaReceiptItem
    {
        [XmlElement(ElementName = "ERPReceiptNumber")]
        public string ERPReceiptNumber { get; set; }

        [XmlElement(ElementName = "UniqueName")]
        public string UniqueName { get; set; }

        [XmlElement(ElementName = "StatusString")]
        public string StatusString { get; set; }
    }

    [XmlRoot("Fault", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class SoapFault
    {
        [XmlElement("faultcode")]
        public string FaultCode { get; set; }

        [XmlElement("faultstring")]
        public string FaultString { get; set; }
    }
}
