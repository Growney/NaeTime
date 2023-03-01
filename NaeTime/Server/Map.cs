using AutoMapper;

namespace NaeTime.Server
{
    public static class Map
    {
        public static IServiceCollection AddNaeTimeDtoMapper(this IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.CreateBidirectionalMap<Abstractions.Models.Node, Shared.Client.NodeDto>();
                mc.CreateBidirectionalMap<Abstractions.Models.Node, Shared.Node.ConfigurationDto>();
                mc.CreateBidirectionalMap<Abstractions.Models.RX5808, Shared.Client.RX5808Dto>();
                mc.CreateBidirectionalMap<Abstractions.Models.RX5808, Shared.Node.RX5808Dto>();
                mc.CreateBidirectionalMap<Abstractions.Models.FlyingSession, Shared.Client.FlyingSessionDto>();
                mc.CreateBidirectionalMap<Abstractions.Models.Flight, Shared.Client.FlightDto>();
                mc.CreateBidirectionalMap<Abstractions.Models.Lap, Shared.Client.LapDto>();
                mc.CreateBidirectionalMap<Abstractions.Models.Split, Shared.Client.SplitDto>();
                mc.CreateBidirectionalMap<Abstractions.Models.RssiStream, Shared.Node.RssiStreamDto>();
                mc.CreateBidirectionalMap<Abstractions.Models.RssiStream, Shared.Client.RssiStreamDto>();
                mc.CreateBidirectionalMap<Abstractions.Models.RssiStreamPass, Shared.Client.RssiStreamPassDto>();
                mc.CreateBidirectionalMap<Abstractions.Models.RssiStreamReading, Shared.Client.RssiStreamReadingDto>();
                mc.CreateBidirectionalMap<Abstractions.Models.Track, Shared.Client.TrackDto>();
                mc.CreateBidirectionalMap<Abstractions.Models.TimedGate, Shared.Client.TimedGateDto>();
                mc.CreateBidirectionalMap<Abstractions.Models.Pilot, Shared.Client.PilotDto>();
            });
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            return services;
        }

        private static IMapperConfigurationExpression CreateBidirectionalMap<TSource, TDestination>(this IMapperConfigurationExpression mapper, Action<IMappingExpression<TSource, TDestination>>? fromExpression = null, Action<IMappingExpression<TDestination, TSource>>? toExpression = null)
        {
            if (fromExpression != null)
            {
                fromExpression(mapper.CreateMap<TSource, TDestination>());
            }
            else
            {
                mapper.CreateMap<TSource, TDestination>();
            }
            if (toExpression != null)
            {
                toExpression(mapper.CreateMap<TDestination, TSource>());
            }
            else
            {
                mapper.CreateMap<TDestination, TSource>();
            }
            return mapper;
        }
    }
}
