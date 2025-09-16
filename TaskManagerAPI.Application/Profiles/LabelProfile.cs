using AutoMapper;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Profiles;

public class LabelProfile : Profile
{
    public LabelProfile()
    {
        CreateMap<Label, LabelResponseDto>();

        CreateMap<LabelRequestDto, Label>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Board, opt => opt.Ignore())
            .ForMember(dest => dest.BoardId, opt => opt.Ignore());
    }
}