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
            var posts = _context.JobPosts
                .Where(p => p.OwnerName == User.Identity!.Name)
                .ToList();
            return View(posts);
        }

        [HttpGet]
        public IActionResult CreateEditForm(int id = 0)
        {
            if (id == 0)
                return View(new JobPosting());

            var post = _context.JobPosts.Find(id);
            if (post == null || post.OwnerName != User.Identity!.Name)
                return NotFound();

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(JobPosting jobPosting, IFormFile? CompanyLogo)
        {
            if (!ModelState.IsValid)
                return View("CreateEditForm", jobPosting);

            jobPosting.OwnerName = User.Identity!.Name;

            if (CompanyLogo != null && CompanyLogo.Length > 0)
            {
                using var ms = new MemoryStream();
                await CompanyLogo.CopyToAsync(ms);
                jobPosting.CompanyLogo = ms.ToArray();
            }

            if (jobPosting.Id == 0)
            {
                _context.JobPosts.Add(jobPosting);
            }
            else
            {
                var existing = _context.JobPosts.Find(jobPosting.Id);
                if (existing == null || existing.OwnerName != User.Identity!.Name)
                    return NotFound();

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

        public IActionResult Delete(int id)
        {
            if (id == 0)
                return BadRequest();

            var jobFromDb = _context.JobPosts.SingleOrDefault(x => x.Id == id);
            if (jobFromDb == null)
                return NotFound();
            _context.JobPosts.Remove(jobFromDb);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
