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

    public int GetProviderIdForPatient(int patientID)
    {
        using (var dataContext = new HealthReunionEntities())
        {
            return dataContext.Patients.Where(p => p.PatientId == patientID).First().ProviderId;
        }
    }

    public string GetUserNameByPatientId(int patientID)
    {
        using (var dataContext = new HealthReunionEntities())
        {
            return dataContext.Users.Where(p => p.PatientId == patientID).First().UserName;
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

    public bool CheckIfUserNameExists(string userName)
    {
        using (var dataContext = new HealthReunionEntities())
        {
            return dataContext.Users.Where(u => u.UserName.Trim() == userName && u.ProviderId == null).FirstOrDefault() != null;
        }
    }

    public void UpdatePatient(Patient patient, string userName)
    {
        using (var dataContext = new HealthReunionEntities())
        {
            var patientToUpdate = dataContext.Patients.Where(p => p.PatientId == patient.PatientId && p.ProviderId == patient.ProviderId).First();
            patientToUpdate.ProviderId = patient.PatientId;
            patientToUpdate.MedicalRecordNumber = Guid.NewGuid();
            patientToUpdate.LastName = patient.LastName.Trim();
            patientToUpdate.FirstName = patient.FirstName.Trim();
            patientToUpdate.MiddleName = patient.MiddleName.Trim();
            patientToUpdate.DOB = patient.DOB;
            patientToUpdate.Address = patient.Address.Trim();
            patientToUpdate.Phone = patient.Phone.Trim();
            patientToUpdate.Email = patient.Email.Trim();
            patientToUpdate.City = patient.City.Trim();
            patientToUpdate.State = patient.State.Trim();
            patientToUpdate.Country = patient.Country;
            patientToUpdate.IsActive = true;
            dataContext.SaveChanges();
        }
    }

    public void AddPatient(Patient patient, string userName, string defaultPassword)
    {
        using (TransactionScope scope = new TransactionScope())
        {
            using (var dataContext = new HealthReunionEntities())
            {
                if (CheckIfUserNameExists(userName))
                    throw new Exception("User name already exist");
         
                // Add provider enity
                dataContext.Patients.Add(patient);

                // Save changes so that it will insert records into database.
                dataContext.SaveChanges();

                var user = new User();
                user.UserName = userName;
                user.Password = defaultPassword;

                user.PatientId = patient.PatientId;
                user.IsDefaultPassword = true;

                // Add user entity
                dataContext.Users.Add(user);

                dataContext.SaveChanges();

                // Complete the transaction if everything goes well.
                scope.Complete();
            }
        }
    }
}