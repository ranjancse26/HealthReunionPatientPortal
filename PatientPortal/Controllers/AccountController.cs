using System;
using System.Linq;
using System.Web.Mvc;
using PatientPortal.Models;

namespace PatientPortal.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            ViewBag.TitleMessage = "Welcome to HealthReunion Patient Portal";
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid && ValidateUser(model.UserName, model.Password))
                {
                    if (model.Password.Equals("Password1"))
                    {
                        return RedirectToAction("ChangePassword");
                    }
                    else
                        return RedirectToLocal();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        public ActionResult ChangePassword()
        {
            ViewBag.TitleMessage = "Welcome to HealthReunion Patient Portal";
            var loginModel = new LocalPasswordModel();
            loginModel.UserId = int.Parse(Session["UserId"].ToString());
            return View(loginModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(LocalPasswordModel model)
        {
            try
            {
                if (model.NewPassword.Equals(model.OldPassword))
                    throw new Exception("Please use some other password, New Password should not be same as old password.");

                var userRepository = new UserRepository();
                var user = userRepository.GetUserById(int.Parse(Session["PatientId"].ToString()));
                user.Password = model.NewPassword;
                userRepository.UpdatePassword(user);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            return RedirectToLocal();
        }

        private bool ValidateUser(string userName, string passWord)
        {
            try
            {
                passWord = EncryptDecrypt.EncryptData(passWord, EncryptDecrypt.ReadCert());
                using(var dataContext = new HealthReunionDataAccess.HealthReunionEntities())
                {
                    var patient = (from user in dataContext.Users
                                   where user.UserName.Equals(userName) && user.Password.Equals(passWord) && user.ProviderId == null
                                   select user).FirstOrDefault();
                    if(patient != null){
                        Session["PatientId"] = patient.PatientId;
                        Session["UserId"] = patient.UserId;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
       }

        public ActionResult LogOff()
        {
            Session["PatientId"] = "";

            return RedirectToAction("Login", "Account");
        }
        
        private ActionResult RedirectToLocal()
        {          
            return RedirectToAction("Index", "Home");        
        }    
    }
}
