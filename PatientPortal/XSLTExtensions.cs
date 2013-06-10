using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Xsl;

using System.Text;
using System.Diagnostics;
using System.IO;

namespace PatientPortal
{
    namespace RenderXSLTExample
    {
        /// <summary>
        /// A HTMLExtension method to render XML using XSL
        /// </summary>
        public static class HtmlHelperExtensions
        {
            public static MvcHtmlString RenderXslt(string xslPath, string xmlString, List<KeyValuePair<string, string>> parameters = null)
            {
                string xsltResult = string.Empty;

                try
                {
                    // XML Settings
                    XmlReaderSettings xmlSettings = new XmlReaderSettings();
                    xmlSettings.XmlResolver = null;
                    xmlSettings.IgnoreComments = true;
                    xmlSettings.DtdProcessing = DtdProcessing.Ignore;
                    xmlSettings.ValidationType = ValidationType.None;

                    // Attaches an action to the valiation event handler. This will write out error messages in the Output pane.
#if DEBUG
                    xmlSettings.ValidationEventHandler += (sender, e) =>
                    {
                        Debug.WriteLine(string.Format("{0}({1},{2}): {3} - {4}", e.Exception.SourceUri, e.Exception.LineNumber, e.Exception.LinePosition, e.Severity, e.Message));
                    };
#endif

                    // XSLT Settings
                    XmlReaderSettings xsltSettings = new XmlReaderSettings();
                    xsltSettings.XmlResolver = null;
                    xsltSettings.DtdProcessing = DtdProcessing.Ignore;
                    xsltSettings.ValidationType = ValidationType.None;

                    // Attaches an action to the valiation event handler. This will write out error messages in the Output pane.
#if DEBUG
                    xsltSettings.ValidationEventHandler += (sender, e) =>
                    {
                        Debug.WriteLine(string.Format("{0}({1},{2}): {3} - {4}", e.Exception.SourceUri, e.Exception.LineNumber, e.Exception.LinePosition, e.Severity, e.Message));
                    };
#endif

                    // Init params
                    XsltArgumentList xslArgs = new XsltArgumentList();
                    if (parameters != null)
                    {
                        foreach (KeyValuePair<string, string> param in parameters)
                            xslArgs.AddParam(param.Key, string.Empty, param.Value);
                    }

                    // Load XML
                    using (XmlReader reader = XmlReader.Create(new StringReader(xmlString), xmlSettings))
                    {
                        // Load XSL
                        XsltSettings xslSettings = new XsltSettings(true, true); // Need to enable the document() fucntion

                        using (XmlReader xslSource = XmlReader.Create(xslPath, xsltSettings))
                        {
                            XslCompiledTransform xsltDoc = new XslCompiledTransform();
                            xsltDoc.Load(xslSource, xslSettings, new XmlUrlResolver());

                            // Transform
                            using (var sw = new UTF8StringWriter())
                            {
                                XmlWriterSettings settings = new XmlWriterSettings();
                                settings.Encoding = Encoding.UTF8;
                                settings.OmitXmlDeclaration = true;

                                using (var xw = XmlWriter.Create(sw, settings))
                                {
                                    xsltDoc.Transform(reader, xslArgs, sw);
                                }

                                xsltResult = sw.ToString();
                            }
                        }
                    }
                }
                catch { } // custom error handling here

                // Return result
                return MvcHtmlString.Create(xsltResult);
            }
        }
    }

    public class UTF8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}