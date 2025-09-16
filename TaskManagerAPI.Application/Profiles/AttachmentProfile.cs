using AutoMapper;
using TaskManagerAPI.Application.Dtos.Attachment;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Application.Profiles;

public class AttachmentProfile : Profile
{
    public AttachmentProfile()
    {
        CreateMap<Attachment, AttachmentResponseDto>()
            .ForMember(dest => dest.FileUrl, opt => opt.MapFrom(src => $"/attachments/{src.FilePath}"));
    }
}