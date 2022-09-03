using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NaeTime.Abstractions;
using NaeTime.Abstractions.Factories;
using NaeTime.Abstractions.Models;
using NaeTime.Abstractions.Processors;
using NaeTime.Abstractions.Repositories;
using NaeTime.Core.Factories;
using NaeTime.Core.Processors;
using NaeTime.Core.Repositories;
using NaeTime.Core.Validation;

namespace NaeTime.Core.Extensions
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddNaeTimeDbContext(this IServiceCollection services, Action<DbContextOptionsBuilder>? optionsAction = null)
        {
            services.AddDbContext<ApplicationDbContext>(optionsAction);

            return services;
        }

        public static IServiceCollection AddNaeTimeUnitOfWork(this IServiceCollection services)
        {
            services.AddScoped<INaeTimeUnitOfWork, NaeTimeUnitOfWork>();
            services.AddScoped<INodeRepository, NodeRepository>();
            services.AddScoped<IFlightRepository, FlightRepository>();
            services.AddScoped<ITrackRepository, TrackRepository>();
            services.AddScoped<IFlyingSessionRepository, FlyingSessionRepository>();

            return services;
        }

        public static IServiceCollection AddNaeTimeValidators(this IServiceCollection services)
        {
            services.AddTransient<IValidator<Node>, NodeValidator>();

            return services;
        }

        public static IServiceCollection AddNaeTimeProcessors(this IServiceCollection services)
        {
            services.AddScoped<IRssiStreamReadingProcessorFactory, RssiStreamReadingProcessorFactory>();
            services.AddTransient<IHandlerProcessor, HandlerProcessor>();
            return services;
        }

        public static IServiceCollection AddNaeTimeHandlers(this IServiceCollection services)
        {

            return services;
        }

    }
}
