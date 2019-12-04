using System;
using System.IO;
using System.Threading.Tasks;
using Digitalist.ObjectRecognition.Jobs;
using Digitalist.ObjectRecognition.Services;
using Hangfire;
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
    readonly IBackgroundJobClient _backgroundJobs;

    public TrainNetworkController(
      ILogger<ObjectRecognitionController> logger,
      IBackgroundJobClient backgroundJobs,
      DarknetService darknetService)
    {
      _logger = logger;
      _backgroundJobs = backgroundJobs;
      _darknetService = darknetService;
    }

    [HttpPost("darknet")]
    public ActionResult<string> Darknet(
      string trainimages, string cfgfile, string weightfile, int[] gpus, bool clear)
    {
      var jobId = _darknetService.Train(trainimages, cfgfile, weightfile, gpus, clear);

      return new JsonResult(new
      {
        jobId
      });
    }

    [HttpPost("darknetS3")]
    public ActionResult<string> DarknetS3(string bucketName, string weightsFile, int[] gpus, bool clear)
    {
      var hubId = Guid.NewGuid();
      var outputDirectory = Path.Combine(Path.GetTempPath(), hubId.ToString());

      var weightsFileJob = _backgroundJobs.Enqueue<AmazonS3DownloadJob>(job =>
        job.File(bucketName, weightsFile, outputDirectory)
      );

      var configFileJob = _backgroundJobs.ContinueJobWith<AmazonS3DownloadJob>(weightsFileJob, job =>
        job.File(bucketName, "config.cfg", outputDirectory)
      );

      var trainingImagesPath = "images/train";
      var trainingImagesJob = _backgroundJobs.ContinueJobWith<AmazonS3DownloadJob>(configFileJob, job =>
        job.Directory(bucketName, trainingImagesPath, outputDirectory, null, null)
      );

      var trainingLabelsPath = "labels/train";
      var trainingLabelsJob = _backgroundJobs.ContinueJobWith<AmazonS3DownloadJob>(trainingImagesJob, job =>
        job.Directory(bucketName, trainingLabelsPath, outputDirectory, null, null)
      );

      var trainimages = Path.Combine(outputDirectory, "train.txt");

      var imageListJobId = _backgroundJobs.ContinueJobWith<GenerateImageListJob>(trainingLabelsJob, job =>
        job.Start(Path.Join(outputDirectory, "images", "train"), 
        Path.Join(outputDirectory, "labels", "train"),
        trainimages, null, null)
      );

      var jobId = _backgroundJobs.ContinueJobWith<DarknetTrainJob>(imageListJobId, job =>
      job.Start(trainimages, Path.Combine(outputDirectory, "config.cfg"),
        Path.Combine(outputDirectory, weightsFile), gpus, clear, null, null));

      return new JsonResult(new
      {
        jobId
      });
    }
  }
}