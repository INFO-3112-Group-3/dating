using FindIT.Api.Entities;
using FindIT.Api.Services;

namespace FindIT.Api.Database
{
    // Generates some trash set of skills, would be better to pull from a real list or API but this is just for testing/demo purposes
    public class GenerateSkillTags
    {
        private readonly SkillTagsService _service;

        public GenerateSkillTags(SkillTagsService service)
        {
            _service = service;
        }

        public async Task SeedAsync()
        {
            var skills = GenerateSkills();

            await _service.DeleteAllAsync(); // optional clean slate
            await _service.CreateMultipleAsync(skills);
        }

        private List<SkillTags> GenerateSkills()
        {
            var list = new List<SkillTags>();
            var levels = new[] { "", "Basics", "Advanced", "Expert" };

            var categories = new Dictionary<string, List<string>>
            {
                ["Programming"] = new() { "C#", "Python", "JavaScript", "Rust", "Go", "TypeScript" },
                ["Frontend"] = new() { "React", "Angular", "Vue", "Tailwind", "UI/UX" },
                ["Backend"] = new() { "Node.js", "ASP.NET", "Express", "gRPC", "GraphQL" },
                ["DevOps"] = new() { "AWS", "Docker", "Kubernetes", "CI/CD", "Azure" },
                ["Soft Skills"] = new() { "Agile", "Mentoring", "System Design", "Public Speaking" }
            };

            foreach (var entry in categories)
            {
                foreach (var skillName in entry.Value)
                {
                    foreach (var level in levels)
                    {
                        list.Add(new SkillTags
                        {
                            Name = string.IsNullOrWhiteSpace(level) ? skillName : $"{skillName} {level}",
                            Category = entry.Key
                        });
                    }
                }
            }
            return list;
        }
    }
}
