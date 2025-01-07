using AutoMapper;
using Discussly.Server.Mapping;

namespace Discussly.Server.Services
{
    public static class MapperService
    {
        private static readonly Lazy<IMapper> LazyMapper = new(ConfigureMapper);

        private static IMapper ConfigureMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CommentMapperProfile>();
                cfg.AddProfile<UserMapperProfile>();
            });

            return config.CreateMapper();
        }

        public static IMapper Mapper => LazyMapper.Value;

        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            return Mapper.Map<TSource, TDestination>(source)!;
        }

        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return Mapper.Map(source, destination)!;
        }

        public static IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source)
        {
            return Mapper.ProjectTo<TDestination>(source);
        }
    }
}