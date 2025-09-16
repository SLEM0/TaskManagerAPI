using AutoMapper;
using TaskManagerAPI.Application.Dtos.TaskList;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Profiles;

public class TaskListProfile : Profile
{
    public TaskListProfile()
    {
        CreateMap<TaskList, TaskListResponseDto>()
            .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src =>
                src.Tasks.OrderBy(t => t.Order)));

        CreateMap<TaskListRequestDto, TaskList>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Board, opt => opt.Ignore())
            .ForMember(dest => dest.BoardId, opt => opt.Ignore())
            .ForMember(dest => dest.Tasks, opt => opt.Ignore());
    }
}