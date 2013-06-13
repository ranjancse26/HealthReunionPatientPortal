using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PatientPortal.Models
{
    public class DrodownItemsViewModel
    {
        public string SelectedItemId { get; set; }
        public IEnumerable<SelectListItem> Items { get; set; }
    }
}