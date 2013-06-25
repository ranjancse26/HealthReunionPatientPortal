using HealthReunionDataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PatientPortal.Models
{
    public class AppointmentViewModel
    {        
        public int AppointmentId { get; set; }

        [UIHint("Hidden")]
        public string ProviderName { get; set; }
        
        [Display(Name = "Select Provider")]
        public int ProviderId { get; set; }
        
        public int PatientId { get; set; }

        [Required]
        [Display(Name = "Appointment Date")]
        public string AppointmentDate { get; set; }

        [Required]
        [Display(Name = "Reason for visit")]
        public string ReasonForVisit { get; set; }

        public DrodownItemsViewModel Providers { get; set; }

        public List<String> AvailableSlots { get; set; }

        public List<Appointment> AppointmentViewModelList { get; set; }
    }
}