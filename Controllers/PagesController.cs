using Asp.Versioning;
using AspAuthentication.Auth.Attributes;
using AspAuthentication.Data;
using AspAuthentication.Data.DTOs;
using AspAuthentication.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspAuthentication.Controllers
{
    [ApiVersion(1.0)]
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // requires all routes to be authorized
    // abstract class ControllerBase properties: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase?view=aspnetcore-8.0
    public class PagesController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public PagesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Allow multiple roles: [Authorize(Roles="members,admin")]
        [Authorize(Roles = "Admin")]
        [HttpPost("new")]
        public async Task<ActionResult<Page>> CreatePage(PageDto pageDto)
        {
            // TODO: when will ModelState be invalid?
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var page = new Page
            {
                Id = pageDto.Id,
                Title = pageDto.Title,
                Author = pageDto.Author,
                Body = pageDto.Body,
            };

            _dbContext.Pages.Add(page);
            await _dbContext.SaveChangesAsync();

            // TODO: When to use and how to customize `CreatedAtAction` result
            return CreatedAtAction(
                nameof(CreatePage),
                new { id = page.Id },
                page
             );
        }

        [AllowAnonymous] // allows anonymous requests for the action endpoint, even inside [Authorize] controller
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PageDto>> GetPage(int id)
        {
            var page = await _dbContext.Pages.FindAsync(id);

            if (page is null)
            {
                return NotFound();
            }

            var pageDto = new PageDto
            {
                Id = page.Id,
                Author = page.Author,
                Body = page.Body,
                Title = page.Title,
            };

            return pageDto;
        }

        //[Authorize(Policy = "PolicyName")]  // Policy-Name: Built-In-Policy or Custom-Policy
        //[CustomRequireClaim("Some-Claim-Name", "Some-Claim-Value")] // Can also add constraints for specific claims or other custom logic through annotation attributes
        [HttpGet]
        public async Task<PagesDto> ListPages()
        {
            var pagesFromDb = await _dbContext.Pages.ToListAsync();

            var pagesDto = new PagesDto();

            foreach (var page in pagesFromDb)
            {
                var pageDto = new PageDto
                {
                    Id = page.Id,
                    Author = page.Author,
                    Body = page.Body,
                    Title = page.Title,
                };

                pagesDto.Pages.Add(pageDto);
            }

            return pagesDto;
        }
    }
}
