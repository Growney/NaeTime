using Microsoft.AspNetCore.Mvc;
using NaeTime.Abstractions.Factories;
using NaeTime.Abstractions.Models;
using NaeTime.Abstractions.Processors;
using NaeTime.Shared.Node;

namespace NaeTime.Server.Controllers
{
    public class NodeController : Controller
    {
        private readonly IRssiStreamReadingProcessorFactory _rssiStreamReadingProcessorFactory;
        private readonly IHandlerProcessor _handlerProcessor;

        public NodeController(IRssiStreamReadingProcessorFactory rssiStreamReadingProcessorFactory, IHandlerProcessor handlerProcessor)
        {
            _rssiStreamReadingProcessorFactory = rssiStreamReadingProcessorFactory ?? throw new ArgumentNullException(nameof(rssiStreamReadingProcessorFactory));
            _handlerProcessor = handlerProcessor ?? throw new ArgumentNullException(nameof(handlerProcessor));
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
                        if (stream.RssiValues != null)
                        {
                            try
                            {
                                var readingProcessor = await _rssiStreamReadingProcessorFactory.CreateProcessorAsync(stream.StreamId);
                                if (readingProcessor != null)
                                {
                                    readingProcessors.Add(readingProcessor);
                                    foreach (var readingDto in stream.RssiValues)
                                    {
                                        var reading = new RssiStreamReading()
                                        {
                                            StreamId = stream.StreamId,
                                            Tick = readingDto.Tick,
                                            Value = readingDto.Value
                                        };

                                        readingProcessor.ProcessReading(reading);
                                    }
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    await _rssiStreamReadingProcessorFactory.SaveChangedAsync();

                    foreach (var readingProcessor in readingProcessors)
                    {
                        _handlerProcessor.HandleProcessedStreams(readingProcessor);
                        _handlerProcessor.HandleProcessedFlights(readingProcessor);
                    }
                }
            }
            return Ok();
        }
    }
}
