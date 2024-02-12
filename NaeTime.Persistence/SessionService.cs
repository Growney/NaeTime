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
            })));
    }
}
