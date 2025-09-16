using AutoMapper;
using TaskManagerAPI.Application.Dtos.Comment;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Profiles;

public class CommentProfile : Profile
{
    public CommentProfile()
    {
        CreateMap<Comment, CommentResponseDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.Username));

        CreateMap<CommentRequestDto, Comment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Author, opt => opt.Ignore())
            .ForMember(dest => dest.AuthorId, opt => opt.Ignore())
            .ForMember(dest => dest.Task, opt => opt.Ignore())
            .ForMember(dest => dest.IsSystemLog, opt => opt.Ignore());
    }
}