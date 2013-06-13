using PatientPortal.RenderXSLTExample;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace PatientPortal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["PatientId"] == null || Session["PatientId"].ToString() == "")
            {
                return RedirectToAction("Login", "Account");
            }

            var documentRepository = new DocumentRepository();
            var documentModel = new List<PatientDocumentsModel>();
            documentModel = documentRepository.GetDocuments(int.Parse(Session["PatientId"].ToString()));

            var ccrXslPath = Server.MapPath("~/ccr.xsl");
            var ccdXslPath = Server.MapPath("~/ccd.xsl");
            foreach (var doc in documentModel)
            {
                if(doc.DocumentText.Contains("ContinuityOfCareRecord"))
                    doc.DocumentText = HtmlHelperExtensions.RenderXslt(ccrXslPath, doc.DocumentText).ToHtmlString();
                else if(doc.DocumentText.Contains("ClinicalDocument"))
                    doc.DocumentText = HtmlHelperExtensions.RenderXslt(ccdXslPath, doc.DocumentText).ToHtmlString();
            }

            ViewBag.TitleMessage = "Welcome to HealthReunion Patient Portal";
            return View(documentModel);
        }
           
        public ActionResult About()
        {
            ViewBag.TitleMessage = "Welcome to HealthReunion Patient Portal";
            ViewBag.Message = "Designed and Developed by Ranjan Dailata.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.TitleMessage = "Welcome to HealthReunion Patient Portal";
            return View();
        }

        public ActionResult HealthTopics()
        {
            ViewBag.TitleMessage = "Welcome to HealthReunion Patient Portal";
            return View();
        }

        [HttpPost]
        public ActionResult HealthTopics(FormCollection formCollection)
        {
            ViewBag.TitleMessage = "Welcome to HealthReunion Patient Portal";
            string webUrl = string.Empty;
            string healthTopicName = "";

            if (formCollection["txtHealthTopicName"] != "")
            {
                healthTopicName = formCollection["txtHealthTopicName"].ToString();
                if (healthTopicName == "")
                {
                    ViewBag.Result = "Please enter health topic name";
                    return View();
                }
                webUrl = "http://wsearch.nlm.nih.gov/ws/query?db=healthTopics&term=" + healthTopicName;
                ViewBag.HealthTopicName = healthTopicName;
            }

            var response = MakeRequest(webUrl);

            if (response != null)
            {
                if (response.ToString().Contains("spellingCorrection"))
                    ViewBag.Result = string.Format("Spelling Correction: {0}", ProcessSpellingCorrection(response));
                else
                    ViewBag.Result = ProcessResponse(response);
            }

            return View();
        }

        private string ProcessSpellingCorrection(XDocument healthTopicsResponse)
        {
            return healthTopicsResponse.Descendants("spellingCorrection").First().Value;
        }

        private string ProcessResponse(XDocument healthTopicsResponse)
        {
            if (healthTopicsResponse == null) return string.Empty;

            string formattedResponse = "";
            var fullSummaryNodes = (from node in healthTopicsResponse.Descendants("content")
                           where node.Attribute("name").Value == "FullSummary"
                           select node);
            foreach (var node in fullSummaryNodes)
            {
                formattedResponse += node.Value;
                formattedResponse += "<br/>";
            }
            return formattedResponse;
        }

        public static XDocument MakeRequest(string requestUrl)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                var xmlDoc = XDocument.Load(response.GetResponseStream());
                return (xmlDoc);
            }
            catch (Exception e)
            {
                 return null;
            }
        }
    }
}
