using HealthReunionDataAccess;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Transactions;
using System.Web;

/// <summary>
/// Summary description for ProviderRepository
/// </summary>
public class ProviderRepository
{
	public List<Provider> GetAllProviders()
    {
        using (var dataContext = new HealthReunionEntities())
        {
            return dataContext.Providers.ToList();
        }
    }

    public string GetProviderEmailById(int id)
    {
        using (var dataContext = new HealthReunionEntities())
        {
            return dataContext.Providers.Find(id).Email;
        }
    }

    public bool ValidateUser(string userName, string passWord, int providerID)
    {
        using (var dataContext = new HealthReunionEntities())
        {
            passWord = EncryptDecrypt.EncryptData(passWord, EncryptDecrypt.ReadCert());
            return dataContext.Users.Where(u => u.UserName.Trim() == userName && u.Password.Trim() == passWord && u.ProviderId == providerID).FirstOrDefault() != null;
        }
    }
    
    public bool CheckIfUserNameExists(string userName)
    {
        using (var dataContext = new HealthReunionEntities())
        {
            return dataContext.Users.Where(u => u.UserName.Trim() == userName && u.PatientId == null).FirstOrDefault() != null ;
        }
    }


    public bool CheckIfProviderNameExists(string providerName)
    {
        using (var dataContext = new HealthReunionEntities())
        {
            return dataContext.Providers.Where(p => p.ProviderName.Trim() == providerName).FirstOrDefault() != null;
        }
    }
    
    public void AddProviderWithDefaultUser(Provider provider, User user)
    {        
        using (TransactionScope scope = new TransactionScope())
        {
            using (var dataContext = new HealthReunionEntities())
            {

                if (CheckIfUserNameExists(user.UserName))
                    throw new Exception("User name already exist");
          
                if(CheckIfProviderNameExists(provider.ProviderName))
                    throw new Exception("Provider name already exist");

                // Add provider enity
                dataContext.Providers.Add(provider);

                // Save changes so that it will insert records into database.
                dataContext.SaveChanges();

                user.ProviderId = provider.ProviderId;

                user.Password = EncryptDecrypt.EncryptData(user.Password, EncryptDecrypt.ReadCert());
                                
                // Add user entity
                dataContext.Users.Add(user);

                dataContext.SaveChanges();

                // Complete the transaction if everything goes well.
                scope.Complete();
            }
        }
    }
}