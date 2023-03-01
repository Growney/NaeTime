using NaeTime.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Abstractions.Repositories
{
    public interface IRssiStreamReadingBatchRepository
    {
        Task<List<RssiStreamReadingBatch>> GetWithReadingsAsync(IEnumerable<Guid> ids);
        Task<RssiStreamReadingBatch?> GetWithReadingsAsync(Guid id);
        Task<List<RssiStreamReadingBatch>> GetForStreamAsync(Guid streamId);
        Task<List<RssiStreamReadingBatch>> GetForFlightAsync(Guid flightId);
        Task<List<RssiStreamReadingBatch>> GetUnprocessedForFlight(Guid flightId);

        void Insert(RssiStreamReadingBatch batch);
        void Update(RssiStreamReadingBatch batch);
    }
}
