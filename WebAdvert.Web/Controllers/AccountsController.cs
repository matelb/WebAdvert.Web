using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.Accounts;
using Amazon.AspNetCore.Identity.Cognito;
using Microsoft.AspNetCore.Authorization;

namespace WebAdvert.Web.Controllers
{
    public class AccountsController : Controller
    {

        private readonly SignInManager<CognitoUser> signInManager;

        private readonly UserManager<CognitoUser> userManager;

        private readonly CognitoUserPool pool;


        public AccountsController(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager, CognitoUserPool pool)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.pool = pool;
        }

        [Authorize]
        public async Task<IActionResult> SignUp()
        {
            var model = new SignupViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignupViewModel signupViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = pool.GetUser(signupViewModel.Email);
                if(user.Status != null)
                {
                    ModelState.AddModelError("UserExists", "User with this email already exists");
                    return View(signupViewModel);
                }

                user.Attributes.Add(CognitoAttribute.Name.AttributeName, signupViewModel.Email);
                var createdUser = await (userManager as CognitoUserManager<CognitoUser>).CreateAsync(user, signupViewModel.Password).ConfigureAwait(false);

                if (createdUser.Succeeded)
                {
                   return RedirectToAction("Confirm");
                }
            }
            return View();
        }
       
        public async Task<IActionResult> Confirm(ConfirmViewModel model)
        {
            return View(model);

        }

        [HttpPost]
        [ActionName("Confirm")]
        public async Task<IActionResult> Confirm_Post(ConfirmViewModel model)
        {
            if (ModelState.IsValid) { 
                var user = await userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
                if(user == null)
                {
                    ModelState.AddModelError("Not Found", "A user with the given email adress was not found");
                    return View(model);
                }

                var result = await (userManager as CognitoUserManager<CognitoUser>).ConfirmSignUpAsync(user, model.Code,true).ConfigureAwait(false) ;

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach(var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }
                    return View(model);
                }
            }

            return View(model);
        }


        public async Task<IActionResult> Login(LoginViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        [ActionName("Login")]
        public async Task<IActionResult> Login_Post(LoginViewModel model)
        {

            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("LoginError", "Email and password do not match");
                }
            }

            return View("Login",model);
        }
    }
}