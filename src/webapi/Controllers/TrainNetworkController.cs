using System.IO;
using System.Threading.Tasks;
using Digitalist.ObjectRecognition.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Digitalist.ObjectRecognition.Controllers
{
  [Route("api/train"), ApiController]
  [Produces("application/json")]
  public class TrainNetworkController : ControllerBase
  {
    readonly ILogger _logger;
    readonly DarknetService _darknetService;

    public TrainNetworkController(
      ILogger<ObjectRecognitionController> logger,
      DarknetService darknetService)
    {
      _logger = logger;
      _darknetService = darknetService;
    }

    [HttpPost("darknet")]
    public async Task<ActionResult<string>> Darknet(
      string trainimages, string cfgfile, string weightfile, int[] gpus, bool clear)
    {
      var jobId = _darknetService.Train(trainimages, cfgfile, weightfile, gpus, clear);

      return new JsonResult(new {
        jobId
      });
    }
  }
}