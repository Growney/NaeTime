using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace EventSourcingCore.CommandHandler.ASPNET
{
    public interface IHttpRequestCommandHandler
    {
        Task Handle(HttpContext context);
    }
}
