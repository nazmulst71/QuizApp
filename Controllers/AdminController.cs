using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;

namespace QuizApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        public AdminController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }


        // Login action
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(User model)
        {
            var setupEmail = _config["AdminCredentials:Email"];
            var setupPassword = _config["AdminCredentials:Password"];

            if (model.Email == setupEmail && model.Password == setupPassword)
            {
                HttpContext.Session.SetString("AdminEmail", model.Email);
                return RedirectToAction("Dashboard");
            }

            ViewBag.Message = "Invalid credentials.";
            return View(model);
        }

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("AdminEmail") == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }




        // /Admin
        public async Task<IActionResult> Index()
        {
            var quizzes = await _db.Quizzes
                .Include(q => q.Questions)
                .ToListAsync();
            return View(quizzes);
        }

        // QUIZ CRUD
        public IActionResult CreateQuiz() => View(new Quiz());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuiz(Quiz model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.Quizzes.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditQuiz(int id)
        {
            var quiz = await _db.Quizzes.FindAsync(id);
            if (quiz == null) return NotFound();
            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuiz(Quiz model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _db.Quizzes.FindAsync(id);
            if (quiz == null) return NotFound();
            _db.Quizzes.Remove(quiz);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // QUESTIONS
        public async Task<IActionResult> ManageQuestions(int quizId)
        {
            var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) return NotFound();
            return View(quiz);
        }

        public IActionResult CreateQuestion(int quizId) =>
            View(new Question { QuizId = quizId });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuestion(Question q)
        {
            if (!ModelState.IsValid) return View(q);
            _db.Questions.Add(q);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(ManageQuestions), new { quizId = q.QuizId });
        }

        public async Task<IActionResult> EditQuestion(int id)
        {
            var q = await _db.Questions
                .Include(x => x.Options)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (q == null) return NotFound();
            return View(q);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(Question q)
        {
            if (!ModelState.IsValid) return View(q);
            _db.Update(q);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(ManageQuestions), new { quizId = q.QuizId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var q = await _db.Questions.FindAsync(id);
            if (q == null) return NotFound();
            var quizId = q.QuizId;
            _db.Questions.Remove(q);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(ManageQuestions), new { quizId });
        }

        // OPTIONS
        public IActionResult CreateOption(int questionId) =>
            View(new Option { QuestionId = questionId });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOption(Option o)
        {
            if (!ModelState.IsValid) return View(o);

            if (o.IsCorrect)
            {
                var others = _db.Options.Where(x => x.QuestionId == o.QuestionId);
                foreach (var x in others) x.IsCorrect = false;
            }

            _db.Options.Add(o);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(EditQuestion), new { id = o.QuestionId });
        }

        public async Task<IActionResult> EditOption(int id)
        {
            var o = await _db.Options.FindAsync(id);
            if (o == null) return NotFound();
            return View(o);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOption(Option o)
        {
            if (!ModelState.IsValid) return View(o);

            if (o.IsCorrect)
            {
                var others = _db.Options.Where(x => x.QuestionId == o.QuestionId && x.Id != o.Id);
                foreach (var x in others) x.IsCorrect = false;
            }

            _db.Update(o);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(EditQuestion), new { id = o.QuestionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOption(int id)
        {
            var o = await _db.Options.FindAsync(id);
            if (o == null) return NotFound();
            var questionId = o.QuestionId;
            _db.Options.Remove(o);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(EditQuestion), new { id = questionId });
        }
    }
}
