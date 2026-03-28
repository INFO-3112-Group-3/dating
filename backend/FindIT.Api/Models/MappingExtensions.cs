using FindIT.Api.DTOs;
using FindIT.Api.Entities;

namespace FindIT.Api.Helpers;

public static class MappingExtensions
{
    public static UserPublicDto ToPublicDto(this User user)
    {
        return new UserPublicDto
        {
            Id = user.Id!,
            Salutation = user.Salutation,
            FirstName = user.FirstName,
            Gender = user.Gender,
            Age = user.Age,
            ContactInfo = user.ContactInfo,
            Interests = user.Interests,
            Skills = user.Skills,
            ContactMethod = user.ContactMethod,
            LastName = user.LastName,

        };
    }
}