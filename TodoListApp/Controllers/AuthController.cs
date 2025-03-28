using Microsoft.AspNetCore.Mvc;
using System;
using TodoListApp.Models;

namespace TodoListApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly TodoDb _context;

        public AuthController(TodoDb context)
        {
            _context = context;
        }

        public IActionResult Login() => View();

        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                _context.users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            return View(user);
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            var foundUser = _context.users.FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);
            if (foundUser != null)
            {
                //Store the user ID in session
                HttpContext.Session.SetInt32("UserId", foundUser.UserId);
                return RedirectToAction("Index", "Home");
            }

            TempData["ToastMessage"] = "Invalid credentials!";
            return View(user);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }

    }
}
