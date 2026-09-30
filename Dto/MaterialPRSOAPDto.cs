using System;
using System.Xml.Serialization;

namespace Dto
{
    [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class MaterialPRSoapEnvelope
    {

        [XmlElement("Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public MaterialPRSoapBody Body { get; set; }
        [XmlElement("Header", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public MaterialPRSoapHeader Header { get; set; }
        [XmlIgnore]
        public string PRNumber => Body?.RequisitionImportPullReply?.RequisitionItemWrapper?.Item?.UniqueName;
        [XmlIgnore]
        public string Status => Body?.RequisitionImportPullReply?.RequisitionItemWrapper?.Item?.StatusString;
    }

    public class MaterialPRSoapBody
    {

        [XmlElement("RequisitionImportPullReply", Namespace = "urn:Ariba:Buyer:vsap")]
        public RequisitionImportPullReply RequisitionImportPullReply { get; set; }


    }
    public class MaterialPRSoapHeader
    {
        [XmlAnyElement]
        public List<System.Xml.XmlElement> Any { get; set; }
    }

    public class RequisitionImportPullReply
    {

        [XmlElement("Requisition_RequisitionIdExport_Item", Namespace = "urn:Ariba:Buyer:vsap")]
        public RequisitionItemWrapper RequisitionItemWrapper { get; set; }
    }

    public class RequisitionItemWrapper
    {

        [XmlElement("item", Namespace = "urn:Ariba:Buyer:vsap")]
        public RequisitionItem Item { get; set; }
    }

    public class RequisitionItem
    {
        [XmlElement("UniqueName", Namespace = "urn:Ariba:Buyer:vsap")]
        public string UniqueName { get; set; }

        [XmlElement("OriginatingSystemReferenceID", Namespace = "urn:Ariba:Buyer:vsap")]
        public string OriginatingSystemReferenceID { get; set; }

        [XmlElement("StatusString", Namespace = "urn:Ariba:Buyer:vsap")]
        public string StatusString { get; set; }

        [XmlElement("OriginatingSystem", Namespace = "urn:Ariba:Buyer:vsap")]
        public string OriginatingSystem { get; set; }

        [XmlElement("OriginatingSystemAmendmentID", Namespace = "urn:Ariba:Buyer:vsap")]
        public string OriginatingSystemAmendmentID { get; set; }
    }

    // Optional: For parsing SOAP Faults
    [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class SoapFaultEnvelope
    {
        [XmlElement("Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public SoapFaultBody Body { get; set; }

        [XmlIgnore]
        public string FaultString => Body?.Fault?.Faultstring;
    }

    public class SoapFaultBody
    {
        [XmlElement("Fault")]
        public AribaSoapFault Fault { get; set; }
    }

    public class AribaSoapFault
    {
        [XmlElement("faultcode")]
        public string Faultcode { get; set; }

        [XmlElement("faultstring")]
        public string Faultstring { get; set; }
    }
}
