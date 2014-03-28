using System;
using System.Linq;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;
using Vim25Api;

namespace Mike.Vmware.Connect.Api
{
    internal static class FaultConverter
    {
        public static MethodFault CreateMethodFault(Exception exception)
        {
            MethodFault fault = null;

            if (exception == null)
                return null;

            SoapException soapException = exception as SoapException ?? exception.InnerException as SoapException;

            if (soapException != null)
            {
                fault = ConvertSoapException(soapException);
            }

            return fault;
        }

        private static MethodFault ConvertSoapException(SoapException soapException)
        {
            if (soapException != null && soapException.Detail != null)
            {
                XmlNode xmlNode =
                    soapException.Detail.ChildNodes.Cast<XmlNode>()
                        .FirstOrDefault(xmlNode2 => !(xmlNode2 is XmlWhitespace));

                if (xmlNode != null)
                {
                    string localName = xmlNode.LocalName;
                    MethodFault methodFault = GetMethodFault(xmlNode, typeof (VimService), "urn:vim25", localName);

                    return methodFault;
                }
            }
            return null;
        }

        private static MethodFault GetMethodFault(XmlNode faultDetail, Type vimServiceType, string faultNamespace,
            string faultName)
        {
            if (faultDetail.LocalName.EndsWith("Fault"))
            {
                faultName = faultName.Substring(0, faultName.Length - "Fault".Length);
            }

            MethodFault result;
            try
            {
                string name = vimServiceType.Namespace + "." + faultName;
                Type type = vimServiceType.Assembly.GetType(name, false);
                var xmlSerializer = new XmlSerializer(type, new XmlRootAttribute(faultDetail.LocalName)
                {
                    Namespace = faultNamespace
                });
                XmlReader xmlReader = new XmlNodeReader(faultDetail);
                result = xmlSerializer.Deserialize(xmlReader) as MethodFault;
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }
    }
}