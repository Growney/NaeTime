using Microsoft.EntityFrameworkCore;
using NaeTime.Abstractions.Models;
using NaeTime.Abstractions.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Core.Repositories
{
    public class RssiStreamReadingBatchRepository : IRssiStreamReadingBatchRepository
    {
        private readonly ApplicationDbContext _context;

        public RssiStreamReadingBatchRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<List<RssiStreamReadingBatch>> GetForFlightAsync(Guid flightId)
        {
            var flightStreams = (from flight in _context.Flights
                                 from stream in flight.RssiStreams
                                 where flight.Id == flightId
                                 select stream.Id);

            return (from batch in _context.RssiReadingBatches
                    where flightStreams.Contains(batch.Id)
                    select batch)
                    .ToListAsync();
        }

        public Task<List<RssiStreamReadingBatch>> GetForStreamAsync(Guid streamId)
            => (from batch in _context.RssiReadingBatches
                where batch.RssiStreamId == streamId
                select batch)
            .ToListAsync();

        public Task<List<RssiStreamReadingBatch>> GetUnprocessedForFlight(Guid flightId)
        {
            var flightStreams = (from flight in _context.Flights
                                 from stream in flight.RssiStreams
                                 where flight.Id == flightId
                                 select stream.Id);

            return (from batch in _context.RssiReadingBatches
                    where flightStreams.Contains(batch.Id) && batch.Processed == null
                    select batch)
                    .ToListAsync();
        }
        

        public Task<RssiStreamReadingBatch?> GetWithReadingsAsync(Guid id)
            => (from batch in _context.RssiReadingBatches
                where batch.Id == id
                select batch)
            .Include(x => x.Readings)
            .FirstOrDefaultAsync();
        public Task<List<RssiStreamReadingBatch>> GetWithReadingsAsync(IEnumerable<Guid> ids)
            => (from batch in _context.RssiReadingBatches
                where ids.Contains(batch.Id)
                select batch)
            .Include(x => x.Readings)
            .ToListAsync();

        public void Insert(RssiStreamReadingBatch batch) => _context.RssiReadingBatches.Add(batch);

        public void Update(RssiStreamReadingBatch batch) => _context.RssiReadingBatches.Add(batch);
    }
}
