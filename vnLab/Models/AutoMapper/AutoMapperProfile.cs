using AutoMapper;
using vnLab.Data.Entities;
using vnLab.Helpers;

namespace vnLab.Models.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<PostCSV, PostCreateRequest>()
            .ForMember(dst => dst.Content, opt => opt.MapFrom(x => x.Body))
            .ForMember(dst => dst.Asked, opt => opt.MapFrom(x => x.CreationDate));
            CreateMap<PostCreateRequest, Post>()
            .ForMember(dst => dst.Modified, opt => opt.MapFrom(x => x.Asked))
            .ForMember(dst => dst.Tags, opt => opt.MapFrom(x => TextHelper.Join(",", x.Tags)));
            CreateMap<Post, PostCSV>();
            CreateMap<Post, PostViewModel>();
        }
    }
}