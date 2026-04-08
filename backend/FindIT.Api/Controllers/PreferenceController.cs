using FindIT.Api.Entities;
using FindIT.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FindIT.Api.Controllers
{
    [Route("api/preferences")]
    [ApiController]
    public class PreferenceController : ControllerBase
    {
        private readonly PreferenceService _prefService;

        public PreferenceController(PreferenceService service)
        {
            _prefService = service;
        }

        [HttpGet]
        public async Task<List<string>> GetAll () => await _prefService.GetAsync();

        [HttpPost]
        public async Task<IActionResult> Create(Preference pref)
        {
            await _prefService.CreateAsync(pref);
            return Ok("Preference added.");
        }   
    }
}
