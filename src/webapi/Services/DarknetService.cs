using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Digitalist.ObjectRecognition.Jobs;
using Digitalist.ObjectRecognition.Models;
using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Digitalist.ObjectRecognition.Services
{
  public class DarknetService
  {
    readonly ILogger _logger;
    readonly IntPtr _network;
    readonly AmazonS3Client _amazonS3Client;
    readonly IBackgroundJobClient _backgroundJobs;

    public DarknetService(ILogger<DarknetService> logger,
      IBackgroundJobClient backgroundJobs,
      AmazonS3Client amazonS3Client)
    {
      _logger = logger;
      _amazonS3Client = amazonS3Client;
      _backgroundJobs = backgroundJobs;
      _logger.LogInformation("Initializing darknet network");

      _network = NativeMethods.Darknet.initialize("cfg/yolov3.cfg", "yolov3.weights", 0);
    }

    public async Task<DarknetResult[]> Detect(string imagefile, float thresh, float hier_thresh)
    {
      try
      {
        var buckets = await _amazonS3Client.ListBucketsAsync();
        _logger.LogInformation($"Bucket count {buckets.Buckets.Count}");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed");
      }

      _logger.LogInformation($"Processing image {imagefile}");
      var outputFile = Path.GetTempFileName();

      NativeMethods.Darknet.detect(_network,
        "cfg/coco.data",
        imagefile,
        thresh,
        hier_thresh,
        outputFile);

      var contents = File.ReadAllText(outputFile);
      _logger.LogInformation(contents);
      return JsonConvert.DeserializeObject<DarknetResult[]>(contents);
    }

    public string Train(string trainimages, string cfgfile, string weightfile, int[] gpus, bool clear)
    {
      _logger.LogInformation($"Enqueueing new training job");

      return _backgroundJobs.Enqueue<DarknetTrainJob>(job =>
      job.Start(Guid.NewGuid().ToString(),
        trainimages, cfgfile, weightfile, gpus, clear));
    }
  }
}
