using AutoMapper;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Entities;
using Vidora.Infrastructure.Api.Dtos.Requests;
using Vidora.Infrastructure.Api.Dtos.Responses.Datas;
using Vidora.Infrastructure.Api.Dtos.Responses.Metas;

namespace Vidora.Infrastructure.Api.Mapping;

public class MovieMappingProfile : Profile
{
    public MovieMappingProfile()
    {
        // Command -> Request
        CreateMap<SearchMovieCommand, SearchMovieRequest>();

        // Response Data -> Entity
        CreateMap<MovieData, Movie>();
        CreateMap<GenreData, Genre>();

        // Pagination -> PaginationResult
        CreateMap<Pagination, PaginationResult>();

        CreateMap<ActorData, MovieMember>();

        CreateMap<MovieData, Movie>()
            .ForMember(dest => dest.Actors,
                opt => opt.MapFrom(src => src.Actors));

    }
}
