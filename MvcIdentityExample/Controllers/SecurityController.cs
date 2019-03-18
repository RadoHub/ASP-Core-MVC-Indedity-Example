using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MvcIdentityExample.Identity;
using MvcIdentityExample.Models.Securiry;

namespace MvcIdentityExample.Controllers
{
    public class SecurityController : Controller
    {
        private UserManager<AppIdentityUser> _userManager;
        private SignInManager<AppIdentityUser> _signInManager;
        public SecurityController(UserManager<AppIdentityUser> userManager, SignInManager<AppIdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                return View(loginViewModel);
            }
            var user = await _userManager.FindByNameAsync(loginViewModel.UserName);
            if (user != null)
            {
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, "Confirm your email please");
                    return View(loginViewModel);
                }
            }
            var result = await _signInManager.PasswordSignInAsync(loginViewModel.UserName, loginViewModel.Password, false, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Employee");
            }
            ModelState.AddModelError(string.Empty, "Login Failed");
            return View(loginViewModel);
        }

        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("LoggedOut", "Employee");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerViewModel);
            }
            var user = new AppIdentityUser
            {
                UserName = registerViewModel.UserName,
                Email = registerViewModel.Email,
                UserAge = registerViewModel.Age
            };
            var result = await _userManager.CreateAsync(user, registerViewModel.Password);
            if (result.Succeeded)
            {
                var confirmationCode = _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callBackUrl = Url.Action("ConfirmEmail", "Security", new { userId = user.Id, code = confirmationCode });
                //Send Email codes here
                return RedirectToAction("Index", "Employee");
            }
            // soncu basarisiz ile ayni kayit sayfasina yonlendir model ile
            return View(registerViewModel);
        }


        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("Index", "Employee");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException("Unable To Find the user");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return View("ConfirmEmail");
            }
            return RedirectToAction("Index", "Employee");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return View();
            }
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return View();
            }
            var confirmationCode = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callBackUrl = Url.Action("ResetPassword", "Security", new { userid = user.Id, code = confirmationCode });
            //must be sent a callback url to email with configured service
            return RedirectToAction("ForgotPasswordEmailSent");
        }

        public IActionResult ForgotPasswordEmailSent()
        {
            return View();
        }

        public IActionResult ResetPassword(string userid, string code)
        {
            if (userid == null || code == null)
            {
                throw new ApplicationException("Code or User must be entered");
            }
            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(resetPasswordViewModel);
            }
            var user = await _userManager.FindByEmailAsync(resetPasswordViewModel.Email);
            if(user == null)
            {
                throw new ApplicationException("User not Found");
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordViewModel.Code, resetPasswordViewModel.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfim");
            }
            return View();
        }

        public IActionResult ResetPasswordConfirm()
        {
            return View();
        }
    }
}