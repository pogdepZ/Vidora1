using AutoMapper;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Entities;
using Vidora.Infrastructure.Api.Dtos.Requests;
using Vidora.Infrastructure.Api.Dtos.Responses.Datas;
using Vidora.Infrastructure.Api.Dtos.Responses.Metas;
using System.Collections.Generic;

namespace Vidora.Infrastructure.Api.Mapping;

public class MovieMappingProfile : Profile
{
    public MovieMappingProfile()
    {
        // Command -> Request
        CreateMap<SearchMovieCommand, SearchMovieRequest>();

        // Response Data -> Entity
        CreateMap<MovieData, Movie>()
            .ForMember(dest => dest.Genres,
                opt => opt.MapFrom(src => src.Genres ?? new List<string>()))
            .ForMember(dest => dest.Actors,
                opt => opt.MapFrom(src => src.Actors ?? new List<ActorData>()))
            .ForMember(dest => dest.UserRating,
                opt => opt.MapFrom(src => src.UserRating))
            .ForMember(dest => dest.IsInWatchlist,
                opt => opt.MapFrom(src => src.IsInWatchlist ?? false));

        CreateMap<GenreData, Genre>();

        // Pagination -> PaginationResult
        CreateMap<Pagination, PaginationResult>();

        CreateMap<ActorData, MovieMember>();
    }
}
