﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PatientPortal.Models
{
    public class Email
    {
        public DrodownItemsViewModel Providers { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }
    }
}