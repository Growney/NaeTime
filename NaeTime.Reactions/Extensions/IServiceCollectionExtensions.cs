using NaeTime.Command.Abstractions;
using NaeTime.Events;
using NaeTime.Query.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddNaeTimeEventReactions(this IServiceCollection services)
    {
        services.AddConstantReaction<HardwareDetectionOccured>(async (serviceProvider, detection) =>
        {
            ISessionQueryHandler sessionQueryHandler = serviceProvider.GetRequiredService<ISessionQueryHandler>();

            NaeTime.Query.Abstractions.Models.Session? session = await sessionQueryHandler.GetActiveSession();

            if (session is null)
            {
                return;
            }
            IDetectionCommandHandler detectionCommandHandler = serviceProvider.GetRequiredService<IDetectionCommandHandler>();

            await detectionCommandHandler.AssignDetectionToSession(detection.DetectionId, session.Id);
        });

        services.AddConstantReaction<DetectionTriggered>(async (serviceProvider, detection) =>
        {
            ISessionQueryHandler sessionQueryHandler = serviceProvider.GetRequiredService<ISessionQueryHandler>();

            NaeTime.Query.Abstractions.Models.Session? session = await sessionQueryHandler.GetActiveSession();

            if (session is null)
            {
                return;
            }
            IDetectionCommandHandler detectionCommandHandler = serviceProvider.GetRequiredService<IDetectionCommandHandler>();

            await detectionCommandHandler.AssignDetectionToSession(detection.DetectionId, session.Id);
        });

        return services;
    }
}
