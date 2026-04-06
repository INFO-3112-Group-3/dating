using FindIT.Api.DTOs;
using FindIT.Api.Entities;

namespace FindIT.Api.Helpers;

/// <summary>
/// I guess this converts user to public user dto? couldof just put it inside the user/{id} get endpoint.
/// </summary>
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
            Preferences = user.Preferences,
            Bio = user.Bio,
            ProfilePictureBase64 = user.ProfilePictureBase64
        };
    }
}