using HealthReunionDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for PatientDocumentsModel
/// </summary>
public class PatientDocumentsModel : Document 
{
	public PatientDocumentsModel()
	{
		//
		// TODO: Add constructor logic here
		//
	}
      
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string MiddleName { get; set; }

    public string DocumentText { get; set; }
}