using AutoMapper;
using TaskManagerAPI.Application.Dtos.Board;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Profiles;

public class BoardProfile : Profile
{
    public BoardProfile()
    {
        CreateMap<Board, BoardResponseDto>()
            .ForMember(dest => dest.IsOwner, opt => opt.Ignore())
            .ForMember(dest => dest.Lists, opt => opt.MapFrom(src => src.Lists.OrderBy(l => l.Order)))
            .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.Labels))
            .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.BoardUsers))
            .AfterMap((src, dest, context) =>
            {
                if (context.TryGetItems(out var items) &&
                    items.TryGetValue("IsOwner", out var isOwnerObj) &&
                    isOwnerObj is bool isOwner)
                {
                    dest.IsOwner = isOwner;
                }
            });

        CreateMap<Board, ShortBoardResponseDto>()
            .ForMember(dest => dest.IsOwner, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.Username))
            .AfterMap((src, dest, context) =>
            {
                if (context.TryGetItems(out var items) &&
                    items.TryGetValue("IsOwner", out var isOwnerObj) &&
                    isOwnerObj is bool isOwner)
                {
                    dest.IsOwner = isOwner;
                }
            });

        CreateMap<BoardRequestDto, Board>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Owner, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
            .ForMember(dest => dest.Lists, opt => opt.Ignore())
            .ForMember(dest => dest.Labels, opt => opt.Ignore())
            .ForMember(dest => dest.BoardUsers, opt => opt.Ignore());
    }
}