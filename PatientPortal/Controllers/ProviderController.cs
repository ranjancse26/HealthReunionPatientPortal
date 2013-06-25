using HealthReunionDataAccess;
using Newtonsoft.Json.Linq;
using PatientPortal.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace HealthCareProvider.Controllers
{
    public class ProviderController : Controller
    {
        [HttpGet]
        public ActionResult Index(int? page)
        {            
            var filter = new PatientPortal.Models.Filter();
            return View(filter);
        }

        private List<PatientPortal.Models.Provider> GetProviders(PatientPortal.Models.Filter filter)
        {
            var repository = new HealthCareProviderRepository();
            var providers = new List<PatientPortal.Models.Provider>();

            var data = repository.GetHealthCareProviderData(filter);
            JObject json = JObject.Parse(data);
            var jss = new JavaScriptSerializer();
            dynamic dynamicData = jss.Deserialize<dynamic>(json["response"]["data"].ToString());

            for (int i = 0; i < dynamicData.Length; i++)
            {
                var item = (dynamicData[i] as System.Collections.Generic.Dictionary<string, object>);
                var provider = new PatientPortal.Models.Provider();
                if (item.ContainsKey("name"))
                {
                    provider.Name = item["name"].ToString();
                }
                if (item.ContainsKey("locality"))
                {
                    provider.Locality = item["locality"].ToString();
                }
                if (item.ContainsKey("latitude"))
                {
                    provider.Latitude = item["latitude"].ToString();
                }
                if (item.ContainsKey("longitude"))
                {
                    provider.Longitude = item["longitude"].ToString();
                }
                if (item.ContainsKey("npi_id"))
                {
                    provider.Npi = item["npi_id"].ToString();
                }
                if (item.ContainsKey("address"))
                {
                    provider.Address = item["address"].ToString();
                }
                if (item.ContainsKey("region"))
                {
                    provider.Region = item["region"].ToString();
                }
                providers.Add(provider);
            }

            return providers;
        }
        
        [HttpPost]
        public ActionResult Index(PatientPortal.Models.Filter filter)
        {
            if (filter.Latitude != 0 && filter.Longitude != 0 && filter.Meters == 0)
            {
                return PartialView("ProvidersWebGrid", new List<PatientPortal.Models.Provider>()); 
            }
            var providers = GetProviders(filter);           
            return PartialView("ProvidersWebGrid", providers); 
        }

        [HttpGet]
        public ActionResult SendEmail()
        {
            if (Session["PatientId"] == null || Session["PatientId"].ToString() == "")
            {
                return RedirectToAction("Login", "Account");
            }
            
            var emailViewModel = new Email();
            emailViewModel.Providers = new DrodownItemsViewModel();
            emailViewModel.Providers.Items = GetProvidersToSendEmail();
            return View(emailViewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SendEmail(Email email)
        {
            StringBuilder errorStringBuilder = new StringBuilder();
            var emailViewModel = new Email();
                
            if (email.Subject == null)
            {
               errorStringBuilder.AppendLine("Subject cannot be empty");
            }
            if (email.MessageBody == null)
            {
                errorStringBuilder.AppendLine("\nEmail Body cannot be empty");
            }
            if (errorStringBuilder.ToString() != "")
            {
                ViewBag.ErrorMessage = errorStringBuilder.ToString();
            }
            else
            {
                ViewBag.ErrorMessage = "";
            }

            try
            {              
                if( ViewBag.ErrorMessage == "")
                    SendEmailToProvider(email);

                emailViewModel.Providers = new DrodownItemsViewModel();
                emailViewModel.Providers.Items = GetProvidersToSendEmail();         
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }
            
            if (ViewBag.ErrorMessage == "")
                ViewBag.SuccessMessage = "Email sent successfully!";

            return View(emailViewModel);
        }

        private void SendEmailToProvider(Email emailModel)
        {
            var fromEmailAddress = new PatientRepository().GetEmailAddress(int.Parse(Session["PatientId"].ToString()));
            var smptSendGrid = new SMTPApi(fromEmailAddress, new List<string>{ emailModel.Providers.SelectedItemId });
            smptSendGrid.SimpleEmail(emailModel.Subject, emailModel.MessageBody);
        }

        private List<SelectListItem> GetProvidersToSendEmail()
        {
            var providersList = new ProviderRepository().GetAllProviders();
            var selectedListItems = new List<SelectListItem>();

            foreach (var provider in providersList)
            {
                selectedListItems.Add(new SelectListItem { Text = provider.ProviderName, Value = provider.Email.ToString() });
            }
            return selectedListItems;
        }

        public ActionResult RequestAppointment()
        {
            if (Session["PatientId"] == null || Session["PatientId"].ToString() == "")
            {
                return RedirectToAction("Login", "Account");
            }

            DateTime dt = DateTime.Now.Date;     

            var appointmentModel = new AppointmentViewModel();
            appointmentModel.AppointmentDate = dt.ToString("yyyy-MM-dd");
            appointmentModel.Providers = new DrodownItemsViewModel();
            appointmentModel.Providers.Items = GetProvidersForAppointment();
            appointmentModel.AppointmentViewModelList = new AppointementRepository().GetAppointmentsByPatientID(int.Parse(Session["PatientId"].ToString()),
                                                                appointmentModel.AppointmentDate);
            return View(appointmentModel);
        }

        [HttpPost]
        public ActionResult CheckAvailability(int providerId, string date)
        {
            DateTime dateTime;            
            if (date != "" && DateTime.TryParse(date, out dateTime))
            {
                if (dateTime.Date >= DateTime.Now.Date)
                {
                    var availableSlots = new AppointementRepository().GetAvailableAppointmentBookings(providerId, date);
                    return PartialView("AppointmentAvailaibilitySlotsWebGrid", availableSlots);
                }
            }
            
            return PartialView("AppointmentAvailaibilitySlotsWebGrid", new List<string>());
         }

        [HttpPost]
        public ActionResult RequestAppointment(AppointmentViewModel appointmentViewModel, FormCollection formCollection)
        {
            bool isError = false;
            var appointmentSlots = new string[11];
            AppointementRepository appointmentRepository = new AppointementRepository();
                  
            if (formCollection["assignChkBx"] == null)
            {
                isError = true;
                ModelState.AddModelError("", "You need to select the time of appointment");
            }
            else
                appointmentSlots = formCollection["assignChkBx"].Split(',');
            
            var appointmentModel = new AppointmentViewModel();
            appointmentModel.Providers = new DrodownItemsViewModel();
            appointmentModel.Providers.Items = GetProvidersForAppointment();
            appointmentModel.AppointmentDate = appointmentViewModel.AppointmentDate;
            appointmentModel.ProviderId = appointmentViewModel.ProviderId;
            appointmentModel.ReasonForVisit = appointmentViewModel.ReasonForVisit;
            appointmentModel.AppointmentViewModelList = appointmentRepository.GetAppointmentsByPatientID(int.Parse(Session["PatientId"].ToString()), appointmentViewModel.AppointmentDate);

            if (isError == false)
            {
                try
                {
                    var appointementList = new List<Appointment>();
                    foreach (string time in appointmentSlots)
                    {
                        appointementList.Add(new Appointment
                        {
                            AppointmentId= 0,
                            AppointmentDate = DateTime.Parse(appointmentViewModel.AppointmentDate),
                            ProviderId = appointmentViewModel.ProviderId,
                            ReasonForVisit = appointmentViewModel.ReasonForVisit,
                            Status = "Booked",
                            Time = time,
                            PatientId = int.Parse(Session["PatientId"].ToString())
                        });
                    }
                    appointmentRepository.AddAppointments(appointementList);
                    appointmentModel.AppointmentViewModelList = appointmentRepository.GetAppointmentsByPatientID(int.Parse(Session["PatientId"].ToString()), appointmentViewModel.AppointmentDate);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
           
            return View(appointmentModel);
        }

        private List<SelectListItem> GetProvidersForAppointment()
        {
            var providersList = new ProviderRepository().GetAllProviders();
            var selectedListItems = new List<SelectListItem>();

            foreach (var provider in providersList)
            {
                selectedListItems.Add(new SelectListItem { Text = provider.ProviderName, Value = provider.ProviderId.ToString() });
            }
            return selectedListItems;
        }
    }
}
