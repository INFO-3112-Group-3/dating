using FindIT.Api.Entities;
using FindIT.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FindIT.Api.Controllers
{
    [ApiController]
    [Route("api/matches")]
    public class MatchController : Controller
    {
        private readonly MatchesService _matchesService;

        public MatchController(MatchesService matchesService)
        {
            _matchesService = matchesService;
        }


        [HttpGet("{id}/connect/{targetId}")]
        public async Task<IActionResult> Connect(string id, string targetId)
        {
            var reverseMatch = await _matchesService.GetMatchAsync(targetId, id);

            if (reverseMatch != null && reverseMatch.Status == MatchStatus.Pending)
            {
                await _matchesService.UpdateStatusAsync(reverseMatch.Id!, MatchStatus.Accepted);
                return Ok(new { Message = "Match accepted!", IsMutual = true });
            }

            await _matchesService.CreateMatchAsync(id, targetId, MatchStatus.Pending);
            return Ok(new { Message = "Match request sent!", IsMutual = false });
        }

        [HttpPost("{id}/decline/{targetId}")]
        public async Task<IActionResult> Decline(string id, string targetId)
        {
            await _matchesService.CreateMatchAsync(id, targetId, MatchStatus.Declined);
            return Ok("User declined.");
        }

        [HttpPatch("{id}/rate/{targetId}")]
        public async Task<IActionResult> Rate(string id, string targetId, [FromBody] int rating)
        {
            if (rating < 1 || rating > 5) return BadRequest("Rating must be between 1 and 5");

            await _matchesService.SetRatingAsync(id, targetId, rating);
            return Ok();
        }
    }
}
