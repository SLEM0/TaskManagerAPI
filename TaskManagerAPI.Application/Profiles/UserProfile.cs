using AutoMapper;
using TaskManagerAPI.Application.Dtos.User;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserProfileDto>();

        CreateMap<UpdateProfileRequestDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsEmailConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.EmailConfirmationCode, opt => opt.Ignore())
            .ForMember(dest => dest.EmailConfirmationCodeExpires, opt => opt.Ignore())
            .ForMember(dest => dest.AvatarUrl, opt => opt.Ignore());
    }
}