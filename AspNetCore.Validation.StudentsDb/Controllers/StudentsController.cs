using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Validation.DbContexts;
using Validation.Models;

namespace Validation.Controllers
{
    public class StudentsController : Controller
    {
        private readonly MyDbContext _context;

        public StudentsController(MyDbContext context)
        {
            _context = context;    
        }

        [AcceptVerbs("Get", "Post")] // дозволяє одному методу контролера обробляти запити і GET, і POST одночасно
        public IActionResult CheckEmail(string email)
        {
            if (email == "admin@ukr.net" || email == "admin@gmail.com")
                return Json(false); // remote-перевірка на стороні сервера має повертати джейсон
            return Json(true);
        }

        // GET: Students
        public async Task<IActionResult> Index()
        {
            return View(await _context.Students.ToListAsync());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .SingleOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        [HttpPost] // метод виконується тільки при POST-запиті (наприклад, відправка форми)
        [ValidateAntiForgeryToken] // перевіряє токен захисту від CSRF-атак, обов'язково
        public async Task<IActionResult> Create([Bind("Id,Name,Surname,Age,GPA,Email")] Student student) // приймає об’єкт студента з прив’язкою лише вказаних полів
        {
            if (student.Surname == "admin") // перевіряє, чи прізвище дорівнює "admin"
                ModelState.AddModelError("Surname", "admin - заборонене прізвище"); // додає помилку до конкретного поля Surname
            if (student.Name == student.Email) // перевіряє, чи ім'я збігається з email
                ModelState.AddModelError("", "ім’я та електронна адреса не повинні збігатися"); // додає загальну помилку моделі (не до конкретного поля)
                                                                                                // порожній рядок у першому параметрі означає, що помилка стосується всієї моделі, а не окремого поля
            if (ModelState.IsValid) // перевіряє, чи пройшла модель усі валідації та наші кастомні перевірки
            {
                _context.Add(student); // додає нового студента до контексту бази даних
                await _context.SaveChangesAsync(); // асинхронно зберігає зміни в базу даних
                return RedirectToAction("Index"); // перенаправляє користувача на список студентів
            }
            return View(student); // якщо є помилки — повертає ту саму форму з введеними даними та повідомленнями про помилки
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.SingleOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();
            
            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Surname,Age,GPA,Email")] Student student)
        {
            if (id != student.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction("Index");
            }
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.SingleOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.SingleOrDefaultAsync(m => m.Id == id);
            if (student != null) _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}