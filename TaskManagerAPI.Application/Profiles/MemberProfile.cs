using AutoMapper;
using TaskManagerAPI.Application.Dtos.Member;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Profiles;

public class MemberProfile : Profile
{
    public MemberProfile()
    {
        CreateMap<Member, MemberResponseDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));

        CreateMap<MemberRequestDto, Member>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AddedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Board, opt => opt.Ignore());
    }
}