using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SQLitePCL;
using Worktastic.Data;
using Worktastic.filters;
using Worktastic.Models;

namespace Worktastic.Controllers
{
    [Route("api/jobposting")]
    [ApiController]
    
    public class ApiJobPostingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ApiJobPostingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var jobsFromDb = _context.JobPosts.ToArray();
            return Ok(jobsFromDb);
        }

        [HttpGet("GetById")]
        public IActionResult GetById(int id)
        {
            var jobFromDb = _context.JobPosts.SingleOrDefault(x => x.Id == id);
            if (jobFromDb == null) return NotFound();
            return Ok(jobFromDb);
        }

        [ApiKeyAuthentication]
        [HttpPost("Create")]
        public IActionResult Create(JobPosting job)
        {
            if (job.Id != 0) return BadRequest("Es darf keine Id geben!");

            _context.JobPosts.Add(job);
            _context.SaveChanges();
            return Ok("Inserat eingetragen");
        }

        [HttpDelete("Delete")]
        public IActionResult Delete(int id)
        {
            var JobFromDb = _context.JobPosts.SingleOrDefault(x => x.Id == id);

            if (JobFromDb == null) return NotFound();

            _context.JobPosts.Remove(JobFromDb);
            _context.SaveChanges();

            return Ok("Inserat gelöscht!");
        }

        [HttpPut("Update")]
        public IActionResult Update(JobPosting job)
        {
            if (job.Id == 0) return BadRequest("Inserat hat keine Id");
            var JobFromDb = _context.JobPosts.SingleOrDefault(x => x.Id == job.Id);

            if (JobFromDb == null) return NotFound();

            var jobFromDB = _context.JobPosts.SingleOrDefault(x => x.Id == job.Id);
            if (jobFromDB == null)
            {
                return NotFound();
            }
            //jobFromDb.Ownername = jobPosting.OwnerName; nicht machen: weil Eigentümer wird im Nachhinein geändert,
            //bei Rollen wird dann im schlimmsten Fall der admin als Eigentümer eingetragen
            jobFromDB.JobTitle = job.JobTitle;
            jobFromDB.JobDescription = job.JobDescription;
            jobFromDB.ContactName = job.ContactName;
            jobFromDB.ContactEmail = job.ContactEmail;
            jobFromDB.ContactPhone = job.ContactPhone;
            jobFromDB.ContactWebsite = job.ContactWebsite;
            jobFromDB.Salary = job.Salary;
            jobFromDB.StartDate = job.StartDate;
            _context.SaveChanges();

            return Ok("Inserat verändert");
        }
    }
}
