using Microsoft.AspNetCore.Mvc;
using QuizApp.Data;
using QuizApp.Models;

namespace QuizApp.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Dashboard()
        {
            var name = HttpContext.Session.GetString("StudentName");
            if (string.IsNullOrEmpty(name))
                return RedirectToAction("Login");
            return View();
        }

        private readonly AppDbContext _context;

        public StudentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Student/Register
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User student)
        {
            // 🔍 Check if email already exists in the database
            bool emailExists = _context.Users.Any(u => u.Email == student.Email);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(student); // Return the form with error message
            }

            if (ModelState.IsValid)
            {
                _context.Users.Add(student);
                _context.SaveChanges();

                ViewBag.SuccessMessage = "Registration successful!"  ;
                ModelState.Clear(); // Optional: clears form fields
                return View();      // Return the same Register view
            }

            return View(student);
        }



        // GET: Student/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Student/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            var student = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (student != null)
            {
                //  Store session or temp data
                HttpContext.Session.SetString("StudentEmail", student.Email);
                HttpContext.Session.SetString("StudentName", student.Name);
                TempData["LoginMessage"] = "Welcome, " + student.Name;

                //  Redirect to Dashboard
                return RedirectToAction("Dashboard", "Student");
            }

            ViewBag.Message = "Invalid email or password.";
            return View();
        }


        //Logout action 
        public IActionResult Logout()
        {
            // Clear all session data
            HttpContext.Session.Clear();

            //  Optionally show a message
            TempData["LogoutMessage"] = "You have been logged out successfully.";

            //  Redirect to login page
            return RedirectToAction("Home");
        }



        // change pass action
        public IActionResult ChangePassword()
        {
            var email = HttpContext.Session.GetString("StudentEmail");
            var student = _context.Users.FirstOrDefault(u => u.Email == email);

            if (student == null)
                return RedirectToAction("Login");

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword)
        {
            var email = HttpContext.Session.GetString("StudentEmail");
            var student = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == currentPassword);

            if (student == null)
            {
                ViewBag.Message = "Current password is incorrect.";
                return View();
            }

            student.Password = newPassword;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Dashboard");
        }

       /* public Controller(AppDbContext db) => _db = db;
        public async Task<IActionResult> Index()
        {
            var quizzes = await _db.Quizzes
               .Select(q => new { q.Id, q.Title, q.Description, q.TimeLimitMinutes })
               .ToListAsync();
            return View(quizzes);
        }*/
    }
}
