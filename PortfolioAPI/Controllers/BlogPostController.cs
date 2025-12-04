using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioAPI.Models;
using System.Text.RegularExpressions;

namespace PortfolioAPI.Controllers
{
    [Route("blogposts")]
    [ApiController]
    public class BlogPostController : Controller
    {
        private readonly PortfolioDbContext _context;

        public BlogPostController(PortfolioDbContext portfolioDbContext)
        {
            _context = portfolioDbContext;
        }

        [HttpGet("routes")]
        public async Task<ActionResult> GetRoutes()
        {
            var result = await _context.BlogPosts
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Slug
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("slug")]
        public async Task<ActionResult> GetBySlug(string slug)
        {
            var result = await _context.BlogPosts.FirstOrDefaultAsync(p => p.Slug == slug);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpGet("id")]
        public async Task<ActionResult> GetById(int id)
        {
            var result = await _context.BlogPosts.FirstOrDefaultAsync(p => p.Id == id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateBlogPost([FromBody] BlogPostDTO blogPostDTO)
        {
            string slug = FormatSlug(blogPostDTO.Title);

            var newBlogPost = new BlogPost
            {
                Title = blogPostDTO.Title,
                Slug = slug,
                ContentJson = blogPostDTO.ContentJson,
                Summary = "Insert epic summary here.",
                IsPublished = blogPostDTO.IsPublished,
                AuthorId = blogPostDTO.AuthorId ?? 0,
                CreatedAt = DateTime.Now
            };

            await _context.BlogPosts.AddAsync(newBlogPost);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return CreatedAtAction("CreateBlogPost", new { id = newBlogPost.Id }, newBlogPost);
        }

        [HttpPut("put")]
        public async Task<ActionResult> UpdateBlogPost(int id, [FromBody] BlogPostDTO updatedBlogPost)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            blogPost.Title = updatedBlogPost.Title;
            blogPost.ContentJson = updatedBlogPost.ContentJson;
            blogPost.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPatch("patch")]
        public async Task<ActionResult> PatchBlogPost([FromBody] BlogPost patchedBlogPost)
        {
            var blogPost = await _context.BlogPosts.FindAsync(patchedBlogPost.Id);

            if (blogPost == null)
            {
                return NotFound();
            }

            if (patchedBlogPost.Title != blogPost.Title)
            {
                blogPost.Title = patchedBlogPost.Title;

                string slug = FormatSlug(patchedBlogPost.Title);
                blogPost.Slug = slug;
            }
            if (patchedBlogPost.ContentJson != blogPost.ContentJson)
            {
                blogPost.ContentJson = patchedBlogPost.ContentJson;
            }
            blogPost.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("delete")]
        public async Task<ActionResult> DeleteBlogPost(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            _context.BlogPosts.Remove(blogPost);

            await _context.SaveChangesAsync();

            return Ok();
        }

        private string FormatSlug(string val)
        {
            // Convert to lower case
            string slug = val.ToLowerInvariant();

            // Remove invalid characters
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

            // Replace multiple spaces with a single hyphen
            slug = Regex.Replace(slug, @"\s+", "-").Trim('-');

            // Collapse multiple hyphens
            slug = Regex.Replace(slug, @"-+", "-");

            return slug;
        }
    }
}
