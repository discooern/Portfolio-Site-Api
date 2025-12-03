using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioAPI.Models;

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

        [HttpPost]
        public async Task<ActionResult> CreateBlogPost([FromBody] BlogPostDTO blogPostDTO)
        {
            var newBlogPost = new BlogPost
            {
                Title = blogPostDTO.Title,
                Slug = blogPostDTO.Slug,
                ContentJson = blogPostDTO.ContentJson,
                Summary = blogPostDTO.Summary,
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

        [HttpPut]
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

        [HttpPatch]
        public async Task<ActionResult> PatchBlogPost(int id, [FromBody] BlogPostDTO patchedBlogPost)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            if (patchedBlogPost.Title != null)
            {
                blogPost.Title = patchedBlogPost.Title;
            }
            if (patchedBlogPost.ContentJson != null)
            {
                blogPost.ContentJson = patchedBlogPost.ContentJson;
            }
            if (patchedBlogPost.Slug != null)
            {
                blogPost.Slug = patchedBlogPost.Slug;
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

            //_context.BlogPosts.Remove(blogPost);

            //await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
