using HealthReunionDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;

/// <summary>
/// Summary description for PatientRepository
/// </summary>
public class PatientRepository
{
	public PatientRepository()
	{
		//
		// TODO: Add constructor logic here
		//
	}

    public string GetEmailAddress(int patientID)
    {
        using (var dataContext = new HealthReunionEntities())
        {
            var patient = dataContext.Patients.Where(p => p.PatientId == patientID).FirstOrDefault();
            if (patient != null)
                return patient.Email;

            return string.Empty;
        }
    }

    public Patient GetPatientById(int patientID)
    {
        using (var dataContext = new HealthReunionEntities())
        {
            return dataContext.Patients.Where(p => p.PatientId == patientID).FirstOrDefault();
        }
    }

    public List<Patient> GetAllPatients(int providerId)
    {
        using (var dataContext = new HealthReunionEntities())
        {
            return dataContext.Patients.Where(p => p.ProviderId == providerId).ToList();
        }
    }

    public void AddPatient(Patient patient, string userName)
    {
        using (TransactionScope scope = new TransactionScope())
        {
            using (var dataContext = new HealthReunionEntities())
            {
                // Add provider enity
                dataContext.Patients.Add(patient);

                // Save changes so that it will insert records into database.
                dataContext.SaveChanges();

                var user = new User();
                user.UserName = userName;
                user.Password = "Password1";

                user.PatientId = patient.PatientId;

                // Add user entity
                dataContext.Users.Add(user);

                dataContext.SaveChanges();

                // Complete the transaction if everything goes well.
                scope.Complete();
            }
        }
    }
}