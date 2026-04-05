using FindIT.Api.Entities;
using FindIT.Api.Services;

namespace FindIT.Api.Database
{
  // Generates some trash set of skills, would be better to pull from a real list or API but this is just for testing/demo purposes
  // Since users can't eneter or add skills and must choose from the list (idk why thats better?)
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

      var baseSkills = new Dictionary<string, List<string>>
      {
        ["Programming Language"] = new()
            {
                "C#", "C++", "Java", "Python", "JavaScript", "TypeScript",
                "Go", "Rust", "Kotlin", "Swift"
            },

        ["Framework"] = new()
            {
                "React", "Angular", "Vue.js", "ASP.NET", "Django", "Spring Boot"
            },

        ["Cloud"] = new()
            {
                "AWS", "Azure", "Google Cloud", "Kubernetes", "Lambda", "EC2"
            }
      };

      foreach (var category in baseSkills)
      {
        foreach (var skill in category.Value)
        {
          foreach (var variant in Expand(skill))
          {
            list.Add(new SkillTags
            {
              Name = variant,
              Category = category.Key
            });
          }
        }
      }

      // pad to ~2000
      var extras = new[]
      {
            "System Design", "Microservices", "API Development",
            "Testing", "Debugging", "Performance Tuning"
        };

      var rand = new Random();

      while (list.Count < 2000)
      {
        var baseSkill = extras[rand.Next(extras.Length)];
        list.Add(new SkillTags
        {
          Name = $"{baseSkill} {Guid.NewGuid().ToString()[..6]}",
          Category = "Software Engineering"
        });
      }

      return list;
    }

    private IEnumerable<string> Expand(string skill)
    {
      return new[]
      {
            skill,
            $"{skill} Basics",
            $"{skill} Advanced",
            $"{skill} Fundamentals"
        };
    }
  }
}
