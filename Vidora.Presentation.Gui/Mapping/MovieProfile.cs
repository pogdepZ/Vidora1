using AutoMapper;
using Vidora.Core.Entities;
using Vidora.Presentation.Gui.Models;

namespace Vidora.Presentation.Gui.Mapping;

public class MovieProfile : Profile
{
    public MovieProfile()
    {
        CreateMap<Core.Entities.Movie, Models.Movie>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.MovieId.ToString()))
            .ForMember(dest => dest.VideoUrl, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.DurationMinutes, opt => opt.Ignore());

        CreateMap<Core.Entities.Genre, Models.Genre>();
        
        CreateMap<Core.Entities.MovieMember, Models.MovieMember>();
    }
}
