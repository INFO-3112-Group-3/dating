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
            Nickname = user.Nickname,
            Username = user.Username,
            FirstName = user.FirstName,
            Gender = user.Gender,
            Age = user.Age,
            PreferredContact = user.PreferredContact,
            ContactIdentifier = user.ContactIdentifier,
            City = user.City,
            Interests = user.Interests,
            Skills = user.Skills
        };
    }
}