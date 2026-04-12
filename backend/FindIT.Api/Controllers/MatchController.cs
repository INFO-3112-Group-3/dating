using FindIT.Api.DTOs;
using FindIT.Api.Entities;
using FindIT.Api.Helpers;
using FindIT.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FindIT.Api.Controllers
{
    [ApiController]
    [Route("api/matches")]
    public class MatchController : Controller
    {
        private readonly MatchesService _matchesService;
        private readonly UsersService _usersService;

        public MatchController(MatchesService matchesService, UsersService usersService)
        {
            _matchesService = matchesService;
            _usersService = usersService;
        }


        [HttpPost("{id}/connect/{targetId}")]
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
            return Ok(new { Message = "User declined." });
        }

        [HttpPatch("{id}/rate/{targetId}")]
        public async Task<IActionResult> Rate(string id, string targetId, [FromBody] int rating)
        {
            if (rating < 1 || rating > 5) return BadRequest(new { Message = "Rating must be between 1 and 5" });

            await _matchesService.SetRatingAsync(id, targetId, rating);
            return Ok(new { Message = "Rating submitted." });
        }

        [HttpGet("{id}/accepted")]
        public async Task<IActionResult> GetAcceptedMatches(string id)
        {
            var matches = await _matchesService.GetMatchesByStatusAsync(id, MatchStatus.Accepted);

            var results = new List<object>(); // Change to a generic list to hold both
            foreach (var m in matches)
            {
                var targetId = m.RequesterId == id ? m.TargetId : m.RequesterId;
                var user = await _usersService.GetByIdAsync(targetId);
                if (user != null)
                {
                    results.Add(new
                    {
                        Profile = user.ToPublicDto(),
                        Rating = m.Rating // Include the stored rating!
                    });
                }
            }
            return Ok(results);
        }
    }
}
