using Microsoft.EntityFrameworkCore;
using NaeTime.Abstractions.Models;
using NaeTime.Abstractions.Repositories;

namespace NaeTime.Core.Repositories
{
    public class NodeRepository : INodeRepository
    {
        private readonly ApplicationDbContext _context;

        public NodeRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<List<Node>> GetForPilotAsync(Guid pilotId)
            => (from node
                in _context.Nodes
                where node.PilotId == pilotId
                select node)
            .Include(x => x.RX5808Receivers)
            .ToListAsync();
        public Task<List<Node>> GetAllAsync()
            => (from node
                in _context.Nodes
                select node)
            .Include(x => x.RX5808Receivers)
            .ToListAsync();
        public Task<Node?> GetAsync(Guid id)
           => (from node
               in _context.Nodes
               where node.Id == id
               select node)
            .Include(x => x.RX5808Receivers)
            .FirstOrDefaultAsync();

        public void Insert(Node node) => _context.Nodes.Add(node);
        public void Update(Node node) => _context.Nodes.Update(node);
    }
}
