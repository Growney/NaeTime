using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Projection.Abstractions;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Persistence.EF.Abstractions
{
    public class EntityFrameworkProjectionPositionRepository : IProjectionPositionRepository
    {
        private readonly IProjectionDBContext _context;
        public EntityFrameworkProjectionPositionRepository(IProjectionDBContext context)
        {
            _context = context;
        }

        public async Task<StorePosition?> GetStorePosition(string key)
        {
            var position = await _context.StorePositions.FindAsync(key);
            if (position != null)
            {
                return new StorePosition(position.CommitPosition, position.PreparePosition);
            }
            else
            {
                return null;
            }
        }

        public async Task<StreamPosition?> GetStreamPosition(string key, string streamName)
        {
            var position = await _context.StreamPositions.FindAsync(key, streamName);
            if (position != null)
            {
                return new StreamPosition(position.Position);
            }
            else
            {
                return null;
            }
        }

        public async Task SetStorePosition(string key, StorePosition position)
        {
            var positionObj = await _context.StorePositions.FindAsync(key);
            if (positionObj != null)
            {
                positionObj.CommitPosition = position.CommitPosition;
                positionObj.PreparePosition = position.PreparePosition;
            }
            else
            {
                await _context.StorePositions.AddAsync(new Model.StorePosition()
                {
                    Key = key,
                    CommitPosition = position.CommitPosition,
                    PreparePosition = position.PreparePosition
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task SetStreamPosition(string key, string streamName, StreamPosition position)
        {
            var positionObj = await _context.StreamPositions.FindAsync(key, streamName);
            if (positionObj != null)
            {
                positionObj.Position = position.Position;
            }
            else
            {
                await _context.StreamPositions.AddAsync(new Model.StreamPosition()
                {
                    Key = key,
                    StreamName = streamName,
                    Position = position.Position
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
