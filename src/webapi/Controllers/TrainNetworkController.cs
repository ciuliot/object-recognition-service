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
      IFormFile image, float thresh = 0.5f, float hier_thresh = 0.5f)
    {
      var filePath = Path.GetTempFileName();

      using (var stream = System.IO.File.Create(filePath))
      {
        await image.CopyToAsync(stream);
      }

      var results = await Task.Factory.StartNew(() =>
      {
        return _darknetService.Detect(filePath, thresh, hier_thresh);
      });

      return new JsonResult(results);
    }
  }
}