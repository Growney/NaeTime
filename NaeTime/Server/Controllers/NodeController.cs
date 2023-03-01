using Microsoft.AspNetCore.Mvc;
using NaeTime.Abstractions;
using NaeTime.Abstractions.Models;
using NaeTime.Abstractions.Processors;
using NaeTime.Shared.Node;

namespace NaeTime.Server.Controllers
{
    public class NodeController : Controller
    {
        private readonly INaeTimeUnitOfWork _unitOfWork;

        public NodeController(INaeTimeUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        [HttpPost("node/values")]
        public async Task<IActionResult> RssiValues([FromBody] NodeRssiStreamValuesDto valuesDto)
        {
            if (valuesDto.Values != null)
            {
                if (valuesDto.Values != null)
                {
                    var readingProcessors = new List<IRssiStreamReadingProcessor>();
                    foreach (var stream in valuesDto.Values)
                    {
                        if (stream.RssiValues != null && stream.RssiValues.Count > 0)
                        {
                            var readings = new List<RssiStreamReading>();
                            int maxRssi = int.MinValue;
                            int minRssi = int.MaxValue;
                            long maxTick = long.MinValue;
                            long minTick = long.MaxValue; 

                            foreach(var readingDto in stream.RssiValues)
                            {
                                if(readingDto.Tick > maxTick)
                                {
                                    maxTick = readingDto.Tick;
                                }
                                if(readingDto.Tick < minTick)
                                {
                                    minTick = readingDto.Tick;
                                }
                                if(readingDto.Value > maxRssi)
                                {
                                    maxRssi = readingDto.Value;
                                }
                                if(readingDto.Value < minRssi)
                                {
                                    minRssi = readingDto.Value;
                                }

                                var reading = new RssiStreamReading()
                                {
                                    Tick = readingDto.Tick,
                                    Value = readingDto.Value,
                                };

                                readings.Add(reading);
                            }

                            var batch = new RssiStreamReadingBatch()
                            {
                                RssiStreamId = stream.StreamId,
                                Readings = readings,
                                MaxRssiValue = maxRssi,
                                MinRssiValue = minRssi,
                                MaxTick = maxTick,
                                MinTick = minTick,
                                ReadingCount = readings.Count
                            };
                            _unitOfWork.RssiStreamReadingBatches.Insert(batch);
                        }
                    }
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            return Ok();
        }
    }
}
