using NaeTime.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Abstractions.Repositories
{
    public interface INodeRepository
    {
        Task<List<Node>> GetForPilotAsync(Guid pilotId);
        Task<List<Node>> GetAllAsync();
        Task<Node?> GetAsync(Guid id);

        void Insert(Node node);
        void Update(Node node);
    }
}
