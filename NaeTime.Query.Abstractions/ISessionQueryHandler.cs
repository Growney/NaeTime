using NaeTime.Query.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Query.Abstractions;
public interface ISessionQueryHandler
{
    public Task<Session?> GetSession(Guid id);
    public Task<IEnumerable<Session>> GetAllSessions();
}
