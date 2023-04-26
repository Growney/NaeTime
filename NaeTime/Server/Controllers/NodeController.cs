using Microsoft.AspNetCore.Mvc;
using NaeTime.Shared.Node;

namespace NaeTime.Server.Controllers
{
    public class NodeController : Controller
    {
        public NodeController()
        {
        }

        [HttpPost("node/values")]
        public async Task<IActionResult> RssiValues([FromBody] NodeRssiStreamValuesDto valuesDto)
        {
            
            return Ok();
        }
    }
}
