using DataDummyProvider.DTOs;
using DataDummyProvider.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiExample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        // GET: api/category
        [HttpGet]
        public ActionResult<List<CategoryDTO>> GetCategories()
        {
            return CategoryService.GetCategories();
        }

        // GET: api/category/1
        [HttpGet("{id}")]
        public ActionResult<CategoryDTO> GetCategory(int id)
        {
            return CategoryService.GetCategory(id);
        }

        // PUT: api/category
        [HttpPut]
        public IActionResult PutCategory(CategoryDTO category)
        {
            CategoryService.UpdateCategory(category);

            return NoContent();
        }

        // POST: api/category
        [HttpPost]
        public ActionResult<CategoryDTO> PostCategory(CategoryDTO category)
        {
            CategoryService.CreateCategory(category);

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        // DELETE: api/category
        [HttpDelete]
        public IActionResult DeleteCategory(CategoryDTO category)
        {
            CategoryService.DeleteCategory(category);

            return NoContent();
        }
    }
}
