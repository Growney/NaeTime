using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.Persistence.Abstractions;
using NaeTime.PubSub;

namespace NaeTime.Persistence;
public class SessionService : ISubscriber
{
    private readonly IRepositoryFactory _repositoryFactory;

    public SessionService(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }
    public async Task<SessionsResponse> On(SessionsRequest request)
    {
        var sessionRepository = await _repositoryFactory.CreateSessionRepository();

        var details = await sessionRepository.GetDetails();

        return new SessionsResponse(
            details.Select(x => new SessionsResponse.Session(x.Id, x.Name,
            x.Type switch
            {
                Models.SessionType.OpenPractice => SessionsResponse.SessionType.OpenPractice,
                _ => throw new NotImplementedException()
            }, x.TrackId, x.MinimumLapMilliseconds, x.MaximumLapMilliseconds)));
    }
    public async Task<SessionResponse?> On(SessionRequest request)
    {
        var sessionRepository = await _repositoryFactory.CreateSessionRepository();

        var details = await sessionRepository.GetDetails(request.SessionId);

        if (details == null)
        {
            return null;
        }

        return new SessionResponse(details.Id, details.Name,
                       details.Type switch
                       {
                           Models.SessionType.OpenPractice => SessionResponse.SessionType.OpenPractice,
                           _ => throw new NotImplementedException()
                       }, details.TrackId, details.MinimumLapMilliseconds, details.MaximumLapMilliseconds);
    }
}
