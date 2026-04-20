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
            if(User.IsInRole("Admin"))
            {
                var allPostings = _context.JobPosts.ToList();
                return View(allPostings);
            }

            var jobsFromDb = _context.JobPosts.Where(x => x.OwnerName == User.Identity.Name).ToList();
            return View(jobsFromDb);
        }

        public IActionResult CreateEditForm(int id)
        {
            if (id != 0)
            {
                var jobFromDb = _context.JobPosts.SingleOrDefault(x => x.Id == id);
                if (jobFromDb == null)
                {
                    return NotFound();
                }
                if (User.Identity.Name != jobFromDb.OwnerName && !User.IsInRole("Admin"))
                {
                    return Unauthorized();
                }
                return View(jobFromDb);
            }
            return View();
        }

        public IActionResult CreateEdit(JobPosting jobPosting, IFormFile CompanyLogo)
        {
            jobPosting.OwnerName = User.Identity.Name;
            if(CompanyLogo != null)
            {
                using(var memoryStream = new MemoryStream())
                {
                    CompanyLogo.CopyTo(memoryStream);
                    var byteArray = memoryStream.ToArray();
                    jobPosting.CompanyLogo = byteArray;
                }
            }
            
            if(jobPosting == null)
            {
                return NotFound();
            }
            if(jobPosting.Id == 0)
                _context.JobPosts.Add(jobPosting);
            else
            {
               var jobFromDB = _context.JobPosts.SingleOrDefault(x => x.Id == jobPosting.Id);
                if(jobFromDB == null)
                {
                    return NotFound();
                }
                //jobFromDb.Ownername = jobPosting.OwnerName; nicht machen: weil Eigentümer wird im Nachhinein geändert,
                //bei Rollen wird dann im schlimmsten Fall der admin als Eigentümer eingetragen
                jobFromDB.JobTitle = jobPosting.JobTitle;
                jobFromDB.JobDescription = jobPosting.JobDescription;
                jobFromDB.ContactName = jobPosting.ContactName;
                jobFromDB.ContactEmail = jobPosting.ContactEmail;
                jobFromDB.ContactPhone = jobPosting.ContactPhone;
                jobFromDB.ContactWebsite = jobPosting.ContactWebsite;
                jobFromDB.Salary = jobPosting.Salary;
                jobFromDB.StartDate = jobPosting.StartDate;
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            if (id == 0)
                return BadRequest();
            else
            {
                var jobFromDb = _context.JobPosts.SingleOrDefault(x => x.Id == id);
                if (jobFromDb == null)
                    return NotFound();
                _context.JobPosts.Remove(jobFromDb);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
        }
    }
}
