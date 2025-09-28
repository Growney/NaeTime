using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Query;
public class SessionQueryHandler : ISessionQueryHandler
{
    public Task<IEnumerable<Session>> GetAllSessions()
    {
        throw new NotImplementedException();
    }

    public Task<Session?> GetSession(Guid id)
    {
        throw new NotImplementedException();
    }
}
