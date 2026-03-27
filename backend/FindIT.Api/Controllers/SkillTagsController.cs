using FindIT.Api.Entities;
using FindIT.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FindIT.Api.Controllers;

[ApiController]
[Route("api/skills")]
public class SkillTagsController : ControllerBase
{
    private readonly SkillTagsService _skillService;

    public SkillTagsController(SkillTagsService skillService)
    {
        _skillService = skillService;
    }

    [HttpGet]
    public async Task<List<SkillTags>> GetAll() => await _skillService.GetAsync();

    [HttpGet("category/{category}")]
    public async Task<List<SkillTags>> GetByCategory(string category) =>
        await _skillService.GetByCategoryAsync(category);

    [HttpPost]
    public async Task<IActionResult> Create(SkillTags tag)
    {
        await _skillService.CreateAsync(tag);
        return Ok("Skill added.");
    }
}