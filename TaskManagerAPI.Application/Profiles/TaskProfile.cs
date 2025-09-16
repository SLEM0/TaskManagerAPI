
using AutoMapper;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Dtos.Member;
using TaskManagerAPI.Application.Dtos.Task;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Profiles;

public class TaskProfile : Profile
{
    public TaskProfile()
    {
        CreateMap<Domain.Entities.Task, TaskResponseDto>()
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));

        CreateMap<Domain.Entities.Task, ShortTaskResponseDto>();

        CreateMap<TaskRequestDto, Domain.Entities.Task>()
            .ForMember(dest => dest.Labels, opt => opt.Ignore())
            .ForMember(dest => dest.Members, opt => opt.Ignore())
            .ForMember(dest => dest.Comments, opt => opt.Ignore())
            .ForMember(dest => dest.Attachments, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TaskList, opt => opt.Ignore());
    }
}