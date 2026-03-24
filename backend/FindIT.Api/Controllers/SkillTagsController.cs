using FindIT.Api.Models;
using FindIT.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FindIT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillTagsController : ControllerBase
    {

        private readonly SkillTagsService _skillTagsService;

        public SkillTagsController(SkillTagsService skillTagsService)
        {
            _skillTagsService = skillTagsService;
        }

        // GET: api/skilltags : Returns all skill tags
        [HttpGet]
        public async Task<List<SkillTags>> Get() => await _skillTagsService.GetAsync();

        // POST: api/skilltags : Creates a new skill tag
        [HttpPost]
        public async Task<IActionResult> Post(SkillTags newTag)
        {
            await _skillTagsService.CreateAsync(newTag);
            return CreatedAtAction(nameof(Get), new { id = newTag.Id }, newTag);
        }
    }
}
