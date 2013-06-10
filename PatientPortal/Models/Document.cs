using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PatientPortal.Models
{
    public class Document
    {
        public int DocumentId { get; set; }
        public int PatientId { get; set; }
        public string DocumentType { get; set; }
        public string Date { get; set; }
    }
}