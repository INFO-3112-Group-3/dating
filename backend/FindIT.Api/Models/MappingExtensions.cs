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
            LastName = user.LastName,
            Email = user.Email,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Age = user.Age,
            ContactMethod = user.ContactMethod,
            ContactInfo = user.ContactInfo,
            City = user.City,
            Interests = user.Interests,
            Skills = user.Skills,
            Preferences = user.Preferences
        };
    }
}