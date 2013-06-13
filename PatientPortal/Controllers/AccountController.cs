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
                bool isDefaultPassword = false;
                if (ModelState.IsValid && ValidateUser(model.UserName, model.Password, out isDefaultPassword))
                {
                    if (isDefaultPassword)
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

        private bool ValidateUser(string userName, string passWord, out bool isDefaultPassword)
        {
            try
            {
                passWord = EncryptDecrypt.EncryptData(passWord, EncryptDecrypt.ReadCert());
                using(var dataContext = new HealthReunionDataAccess.HealthReunionEntities())
                {
                    var user = (from u in dataContext.Users
                                   where u.UserName.Equals(userName) && u.Password.Equals(passWord) && u.ProviderId == null
                                   select u).FirstOrDefault();
                    if(user != null){
                        Session["PatientId"] = user.PatientId;
                        isDefaultPassword = user.IsDefaultPassword;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            isDefaultPassword = false;
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
