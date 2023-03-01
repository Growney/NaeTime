using NaeTime.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Abstractions.Repositories
{
    public interface IPilotRepository
    {
        Task<Pilot?> GetAsync(Guid id);
    }
}
