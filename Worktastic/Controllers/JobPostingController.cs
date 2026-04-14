using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worktastic.Data;
using Worktastic.Models;

namespace Worktastic.Controllers
{
    [Authorize]
    public class JobPostingController : Controller
    {
        private readonly ApplicationDbContext _db;

        public JobPostingController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var posts = _db.JobPosts
                .Where(p => p.OwnerName == User.Identity!.Name)
                .ToList();
            return View(posts);
        }

        [HttpGet]
        public IActionResult CreateEditForm(int id = 0)
        {
            if (id == 0)
                return View(new JobPosting());

            var post = _db.JobPosts.Find(id);
            if (post == null || post.OwnerName != User.Identity!.Name)
                return NotFound();

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateEditForm(JobPosting model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Id == 0)
            {
                model.OwnerName = User.Identity!.Name;
                _db.JobPosts.Add(model);
            }
            else
            {
                var existing = _db.JobPosts.Find(model.Id);
                if (existing == null || existing.OwnerName != User.Identity!.Name)
                    return NotFound();

                existing.JobTitle = model.JobTitle;
                existing.JobDescription = model.JobDescription;
                existing.JobLocation = model.JobLocation;
                existing.StartDate = model.StartDate;
                existing.Salary = model.Salary;
                existing.ContactName = model.ContactName;
                existing.ContactEmail = model.ContactEmail;
                existing.ContactPhone = model.ContactPhone;
                existing.ContactWebsite = model.ContactWebsite;
            }

            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
