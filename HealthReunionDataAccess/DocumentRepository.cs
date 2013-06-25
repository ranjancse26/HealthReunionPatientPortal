using HealthReunionDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for DocumentRepository
/// </summary>
public class DocumentRepository
{
    public void AddDocuments(Document document)
    {
        using (var dataContext = new HealthReunionEntities())
        {
            // Add document entity
            dataContext.Documents.Add(document);
            // Save changes so that it will insert records into database.
            dataContext.SaveChanges();
        }
    }

    public Document GetDocumentById(int documentId, string documentType)
    {
        using (var dataContext = new HealthReunionEntities())
        {
            return dataContext.Documents.Where(d => d.DocumentId == documentId && d.DocumentType.Equals(documentType)).First();
        }
    }

    public List<PatientDocumentsModel> GetDocuments(int patientId, int providerId)
    {
        var patientDocumentsList = new List<PatientDocumentsModel>();
        using (var dataContext = new HealthReunionEntities())
        {
            var patientDocuments = (from doc in dataContext.Documents
                                    join patient in dataContext.Patients
                                    on doc.PatientId equals patient.PatientId
                                    where doc.PatientId == patientId && doc.ProviderId == providerId
                                    select new { Patients = patient , Documents = doc}).ToList();

            foreach (var item in patientDocuments)
            {
                patientDocumentsList.Add(new PatientDocumentsModel
                {
                    FirstName = item.Patients.FirstName,
                    LastName = item.Patients.LastName,
                    MiddleName = item.Patients.MiddleName,
                    DocumentId = item.Documents.DocumentId,
                    DocumentType = item.Documents.DocumentType,
                    PatientId = item.Documents.PatientId,
                    ProviderId = item.Documents.ProviderId,
                    CreationTime = item.Documents.CreationTime,
                    DocumentText = System.Text.Encoding.UTF8.GetString(item.Documents.Document1)
                });
            }

            return patientDocumentsList;
        }
    }
}