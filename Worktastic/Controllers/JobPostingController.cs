using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worktastic.Data;
using Worktastic.Models;

namespace Worktastic.Controllers
{
    [Authorize]
    public class JobPostingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobPostingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                var allPostings = _context.JobPosts.ToList();
                return View(allPostings);
            }

            var jobsFromDb = _context.JobPosts.Where(x => x.OwnerName == User.Identity!.Name).ToList();
            return View(jobsFromDb);
        }

        [HttpGet]
        public IActionResult CreateEditForm(int id = 0)
        {
            if (id != 0)
            {
                var jobFromDb = _context.JobPosts.SingleOrDefault(x => x.Id == id);
                if (jobFromDb == null)
                    return NotFound();
                if (User.Identity!.Name != jobFromDb.OwnerName && !User.IsInRole("Admin"))
                    return Unauthorized();
                return View(jobFromDb);
            }
            return View(new JobPosting());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(JobPosting jobPosting, IFormFile? CompanyLogo)
        {
            if (!ModelState.IsValid)
                return View("CreateEditForm", jobPosting);

            if (CompanyLogo != null && CompanyLogo.Length > 0)
            {
                using var ms = new MemoryStream();
                await CompanyLogo.CopyToAsync(ms);
                jobPosting.CompanyLogo = ms.ToArray();
            }

            if (jobPosting.Id == 0)
            {
                jobPosting.OwnerName = User.Identity!.Name;
                _context.JobPosts.Add(jobPosting);
            }
            else
            {
                var existing = _context.JobPosts.Find(jobPosting.Id);
                if (existing == null)
                    return NotFound();
                if (existing.OwnerName != User.Identity!.Name && !User.IsInRole("Admin"))
                    return Unauthorized();

                // nicht überschreiben: Eigentümer bleibt der ursprüngliche User
                existing.JobTitle = jobPosting.JobTitle;
                existing.JobDescription = jobPosting.JobDescription;
                existing.JobLocation = jobPosting.JobLocation;
                existing.StartDate = jobPosting.StartDate;
                existing.Salary = jobPosting.Salary;
                existing.ContactName = jobPosting.ContactName;
                existing.ContactEmail = jobPosting.ContactEmail;
                existing.ContactPhone = jobPosting.ContactPhone;
                existing.ContactWebsite = jobPosting.ContactWebsite;
                if (jobPosting.CompanyLogo != null)
                    existing.CompanyLogo = jobPosting.CompanyLogo;
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (id == 0)
                return BadRequest();

            var jobFromDb = _context.JobPosts.SingleOrDefault(x => x.Id == id);
            if (jobFromDb == null)
                return NotFound();
            if (jobFromDb.OwnerName != User.Identity!.Name && !User.IsInRole("Admin"))
                return Unauthorized();

            _context.JobPosts.Remove(jobFromDb);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
