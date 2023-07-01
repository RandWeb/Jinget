﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Jinget.Core.Utilities
{
    public class XmlUtility
    {
        /// <param name="omitXmlDeclaration">remove ?xml tag</param>
        /// <param name="namespaces">if null passed, removes unnecessary xsi and xsd namespaces</param>
        /// <returns></returns>
        public static string SerializeToXml(object input, bool omitXmlDeclaration = false, XmlSerializerNamespaces ns = null)
        {
            var settings = new XmlWriterSettings { OmitXmlDeclaration = omitXmlDeclaration };

            XmlSerializer serializer = new XmlSerializer(input.GetType());
            using var strWriter = new StringWriter();
            using XmlWriter writer = XmlWriter.Create(strWriter, settings);
            serializer.Serialize(writer, input, ns);
            return strWriter.ToString();
        }

        /// <summary>
        /// Serialize object to SOAP message xml string
        /// input type in this serialization method needs to be marked as Serializable
        /// </summary>
        public static string SerializeToSoapXml(object input)
        {
            using MemoryStream memStream = new MemoryStream();
            SoapFormatter formatter = new SoapFormatter();
            formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
            formatter.Serialize(memStream, input);
            return Encoding.UTF8.GetString(memStream.GetBuffer());
        }

        /// <summary>
        /// Find first descendant which it's local name is equal to T.Name and tries to deserialize that descendant to 'T' type
        /// </summary>
        /// <typeparam name="T">Name of 'T' should equal to a descendant's local name</typeparam>
        public static T DeserializeXmlDescendantsFirst<T>(string xmlInput)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            string subXmlString = XDocument.Parse(xmlInput).Root.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals(typeof(T).Name)).ToString();
            using var reader = new StringReader(subXmlString);
            return (T)serializer.Deserialize(reader);
        }

        /// <summary>
        /// Find all descendant which their local names are equal to T.Name and tries to deserialize that descendants to 'T' type
        /// </summary>
        /// <typeparam name="T">Name of 'T' should equal to a descendant's local name</typeparam>
        public static List<T> DeserializeXmlDescendantsAll<T>(string xmlInput)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            List<T> descendants = new List<T>();

            var xmlDescendants = XDocument.Parse(xmlInput).Root.Descendants().Where(x => x.Name.LocalName.Equals(typeof(T).Name));
            foreach (var item in xmlDescendants)
            {
                var reader = new StringReader(item.ToString());
                descendants.Add((T)serializer.Deserialize(reader));
            }

            return descendants;
        }
    }
}